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
            .AsNoTracking()
            .Include(item => item.DataScopes)
            .SingleOrDefaultAsync(item => item.Id == id);
        if (group is null)
        {
            return null;
        }

        List<Guid> userIds = await _dbContext.SysUserDataScopeGroups
            .AsNoTracking()
            .Where(link => link.DataScopeGroupId == id && link.User.TenantId == _userContext.TenantId)
            .Select(link => link.UserId)
            .ToListAsync();

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
        SysDataScopeGroup current = await _dbSet.SingleOrDefaultAsync(group => group.Id == id)
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

        await _dbContext.SaveChangesAsync();
        return current;
    }

    public async Task<List<Guid>> GetUserIdsAsync(Guid groupId)
    {
        await EnsureGroupExistsAsync(groupId);
        return await _dbContext.SysUserDataScopeGroups
            .AsNoTracking()
            .Where(link => link.DataScopeGroupId == groupId && link.User.TenantId == _userContext.TenantId)
            .Select(link => link.UserId)
            .ToListAsync();
    }

    /// <summary>
    /// 将用户成员整体替换为指定列表，并验证成员属于当前租户。
    /// </summary>
    public async Task SetUsersAsync(Guid groupId, SysUserDataScopeGroupSetUsersDto dto)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            await EnsureGroupExistsAsync(groupId);
            List<Guid> userIds = dto.UserIds.Distinct().ToList();
            if (userIds.Count > 0)
            {
                int tenantUserCount = await _dbContext.SysUsers
                    .CountAsync(user => userIds.Contains(user.Id) && user.TenantId == _userContext.TenantId);
                if (tenantUserCount != userIds.Count)
                {
                    throw new BusinessException("One or more users do not belong to the current tenant.",
                        StatusCodes.Status400BadRequest);
                }
            }

            await _dbContext.SysUserDataScopeGroups
                .Where(link => link.DataScopeGroupId == groupId)
                .ExecuteDeleteAsync();

            _dbContext.SysUserDataScopeGroups.AddRange(userIds.Select(userId => new SysUserDataScopeGroup
            {
                UserId = userId,
                DataScopeGroupId = groupId,
            }));
            await _dbContext.SaveChangesAsync();
            return true;
        });
    }

    public async Task<List<SysUserDataScopeGroupItemDto>> GetUserDataScopeGroupsAsync(Guid userId)
    {
        List<SysDataScopeGroup> groups = await _dbContext.SysDataScopeGroups
            .AsNoTracking()
            .Where(group => group.IsEnabled && group.Users.Any(link =>
                link.UserId == userId && link.User.TenantId == _userContext.TenantId))
            .Include(group => group.DataScopes)
            .ToListAsync();

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

        bool hasScopes = await _dbContext.SysDataScopes.AnyAsync(scope => scope.GroupId == id);
        bool hasUsers = await _dbContext.SysUserDataScopeGroups.AnyAsync(link => link.DataScopeGroupId == id);
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
        return _dbSet.AnyAsync(group => group.Id == id && group.TenantId == _userContext.TenantId);
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
