using EntityFramework.AppDbFactory;
using SystemMod.Models.SysDataScopeDtos;
using SystemMod.Models.SysDataScopeGroupDtos;
using SystemMod.Models.SysUserDataScopeGroupDtos;

namespace SystemMod.Managers;

/// <summary>
/// 管理数据权限组及其用户成员。
/// </summary>
public class SysDataScopeGroupManager(
    AppDbFactory dbContextFactory,
    ILogger<SysDataScopeGroupManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SysDataScopeGroup>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<SysDataScopeGroupItemDto>> FilterAsync(
        SysDataScopeGroupFilterDto filter
    )
    {
        Queryable = Queryable
            .WhereNotNull(filter.Name, group => group.Name.Contains(filter.Name!))
            .WhereNotNull(filter.IsEnabled, group => group.IsEnabled == filter.IsEnabled);

        return await PageListAsync<SysDataScopeGroupFilterDto, SysDataScopeGroupItemDto>(filter);
    }

    public async Task<SysDataScopeGroupDetailDto?> GetAsync(Guid id)
    {
        SysDataScopeGroup? group = await _dbSet
            .Where(item => item.Id == id)
            .SingleOrDefaultAsync();
        if (group is null)
        {
            return null;
        }

        group.DataScopes = await _dbContext.SysDataScopes
            .Where(scope => scope.GroupId == id)
            .ToListAsync();

        var groupLinks = await _dbContext.SysUserDataScopeGroups
            .Where(link => link.DataScopeGroupId == id)
            .ToListAsync();
        var tenantUserIds = (await _dbContext.SysUsers
            .Where(user => user.TenantId == _userContext.TenantId)
            .ToListAsync()).Select(user => user.Id).ToHashSet();
        List<Guid> userIds = groupLinks
            .Where(link => tenantUserIds.Contains(link.UserId))
            .Select(link => link.UserId)
            .ToList();

        return MapDetail(group, userIds);
    }

    public async Task<SysDataScopeGroup> AddAsync(SysDataScopeGroupAddDto dto)
    {
        SysDataScopeGroup entity = dto.MapTo<SysDataScopeGroup>();
        entity.Name = dto.Name.Trim();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<SysDataScopeGroup> UpdateAsync(Guid id, SysDataScopeGroupUpdateDto dto)
    {
        SysDataScopeGroup current = await _dbSet.Where(group => group.Id == id).SingleOrDefaultAsync()
            ?? throw new BusinessException(Localizer.NotFoundResource, StatusCodes.Status404NotFound);

        if (dto.Name is not null)
        {
            current.Name = dto.Name.Trim();
        }

        if (dto.Description is not null)
        {
            current.Description = dto.Description;
        }

        if (dto.IsEnabled is bool isEnabled)
        {
            current.IsEnabled = isEnabled;
        }

        await _dbSet
            .Where(group => group.Id == id && group.TenantId == _userContext.TenantId)
            .ExecuteUpdateAsync(update => update
                .Set(group => group.Name, current.Name)
                .Set(group => group.Description, current.Description)
                .Set(group => group.IsEnabled, current.IsEnabled));
        return current;
    }

    public async Task<List<Guid>> GetUserIdsAsync(Guid groupId)
    {
        await EnsureGroupExistsAsync(groupId);
        var links = await _dbContext.SysUserDataScopeGroups
            .Where(link => link.DataScopeGroupId == groupId)
            .ToListAsync();
        var tenantUserIds = (await _dbContext.SysUsers
            .Where(user => user.TenantId == _userContext.TenantId)
            .ToListAsync()).Select(user => user.Id).ToHashSet();
        return links.Where(link => tenantUserIds.Contains(link.UserId)).Select(link => link.UserId).ToList();
    }

    /// <summary>
    /// 将用户成员整体替换为指定列表，并验证成员属于当前租户。
    /// </summary>
    public async Task SetUsersAsync(Guid groupId, SysUserDataScopeGroupSetUsersDto dto)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            await EnsureGroupExistsAsync(groupId);
            Guid[] userIds = dto.UserIds.Distinct().ToArray();
            if (userIds.Length > 0)
            {
                int tenantUserCount = await _dbContext.SysUsers
                    .Where(user => userIds.Contains(user.Id) && user.TenantId == _userContext.TenantId)
                    .CountAsync();
                if (tenantUserCount != userIds.Length)
                {
                    throw new BusinessException("One or more users do not belong to the current tenant.",
                        StatusCodes.Status400BadRequest);
                }
            }

            await _dbContext.SysUserDataScopeGroups
                .Where(link => link.DataScopeGroupId == groupId && link.TenantId == _userContext.TenantId)
                .ExecuteDeleteAsync();

            await _dbContext.SysUserDataScopeGroups.BulkInsertAsync(userIds.Select(userId => new SysUserDataScopeGroup
            {
                UserId = userId,
                DataScopeGroupId = groupId,
                TenantId = _userContext.TenantId,
            }));
            return true;
        });
    }

    public async Task<List<SysUserDataScopeGroupItemDto>> GetUserDataScopeGroupsAsync(Guid userId)
    {
        var userGroupLinks = await _dbContext.SysUserDataScopeGroups
            .Where(link => link.UserId == userId && link.TenantId == _userContext.TenantId)
            .ToListAsync();
        var groupIds = userGroupLinks.Select(link => link.DataScopeGroupId).Distinct().ToArray();
        List<SysDataScopeGroup> groups = await _dbContext.SysDataScopeGroups
            .Where(group => group.IsEnabled && groupIds.Contains(group.Id))
            .ToListAsync();
        var scopes = await _dbContext.SysDataScopes
            .Where(scope => groupIds.Contains(scope.GroupId))
            .ToListAsync();
        foreach (var group in groups)
        {
            group.DataScopes = scopes.Where(scope => scope.GroupId == group.Id).ToList();
        }

        return groups.Select(group => new SysUserDataScopeGroupItemDto
        {
            Id = group.Id,
            Name = group.Name,
            DataScopes = group.DataScopes.Select(MapDataScope).ToList(),
        }).ToList();
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        await EnsureGroupExistsAsync(id);

        bool hasScopes = await _dbContext.SysDataScopes.Where(scope => scope.GroupId == id).AnyAsync();
        bool hasUsers = await _dbContext.SysUserDataScopeGroups.Where(link => link.DataScopeGroupId == id).AnyAsync();
        if (hasScopes || hasUsers)
        {
            throw new BusinessException(
                "Remove the group's data scopes and users before deleting it.",
                StatusCodes.Status409Conflict);
        }

        return await DeleteOrUpdateAsync([id], false);
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        return _dbSet.Where(group => group.Id == id && group.TenantId == _userContext.TenantId).AnyAsync();
    }

    private async Task EnsureGroupExistsAsync(Guid groupId)
    {
        if (!await HasPermissionAsync(groupId))
        {
            throw new BusinessException(Localizer.NotFoundResource, StatusCodes.Status404NotFound);
        }
    }

    private static SysDataScopeGroupDetailDto MapDetail(SysDataScopeGroup group, List<Guid> userIds)
    {
        return new SysDataScopeGroupDetailDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            IsEnabled = group.IsEnabled,
            UserIds = userIds,
            DataScopes = group.DataScopes.Select(MapDataScope).ToList(),
        };
    }

    private static SysDataScopeItemDto MapDataScope(SysDataScope scope)
    {
        return new SysDataScopeItemDto
        {
            Id = scope.Id,
            Name = scope.Name,
            ResourceCode = scope.ResourceCode,
            TargetIds = scope.TargetIds,
            ScopeType = scope.ScopeType,
            GroupId = scope.GroupId,
        };
    }
}
