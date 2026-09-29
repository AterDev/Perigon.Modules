using EntityFramework.AppDbFactory;
using SystemMod.Models.SysDataScopeDtos;

namespace SystemMod.Managers;

/// <summary>
/// 管理租户内的数据权限范围。
/// </summary>
public class SysDataScopeManager(
    AppDbFactory dbContextFactory,
    ILogger<SysDataScopeManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SysDataScope>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<SysDataScopeItemDto>> FilterAsync(SysDataScopeFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Name, scope => scope.Name.Contains(filter.Name!))
            .WhereNotNull(filter.ResourceCode, scope => scope.ResourceCode == filter.ResourceCode)
            .WhereNotNull(filter.ScopeType, scope => scope.ScopeType == filter.ScopeType)
            .WhereNotNull(filter.GroupId, scope => scope.GroupId == filter.GroupId);

        return await PageListAsync<SysDataScopeFilterDto, SysDataScopeItemDto>(filter);
    }

    public async Task<SysDataScopeDetailDto?> GetAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(scope => scope.Id == id)
            .Select(scope => new SysDataScopeDetailDto
            {
                Id = scope.Id,
                Name = scope.Name,
                ResourceCode = scope.ResourceCode,
                TargetIds = scope.TargetIds,
                ScopeType = scope.ScopeType,
                GroupId = scope.GroupId,
            })
            .SingleOrDefaultAsync();
    }

    public async Task<SysDataScope> AddAsync(SysDataScopeAddDto dto)
    {
        await EnsureGroupExistsAsync(dto.GroupId);
        ValidateScopeType(dto.ScopeType);

        SysDataScope entity = dto.MapTo<SysDataScope>();
        entity.Name = dto.Name.Trim();
        entity.ResourceCode = dto.ResourceCode.Trim();
        entity.TargetIds = NormalizeTargetIds(dto.TargetIds, dto.ScopeType);
        await InsertAsync(entity);
        return entity;
    }

    public async Task<SysDataScope> UpdateAsync(Guid id, SysDataScopeUpdateDto dto)
    {
        SysDataScope current = await _dbSet.SingleOrDefaultAsync(scope => scope.Id == id)
            ?? throw new BusinessException(Localizer.NotFoundResource, StatusCodes.Status404NotFound);

        if (dto.GroupId is Guid groupId)
        {
            await EnsureGroupExistsAsync(groupId);
            current.GroupId = groupId;
        }

        if (dto.Name is not null)
        {
            current.Name = dto.Name.Trim();
        }

        if (dto.ResourceCode is not null)
        {
            current.ResourceCode = dto.ResourceCode.Trim();
        }

        if (dto.ScopeType is DataScopeType scopeType)
        {
            ValidateScopeType(scopeType);
            current.ScopeType = scopeType;
        }

        if (dto.TargetIds is not null)
        {
            current.TargetIds = NormalizeTargetIds(dto.TargetIds, current.ScopeType);
        }
        else if (dto.ScopeType is DataScopeType { } updatedScopeType && updatedScopeType != DataScopeType.Include)
        {
            current.TargetIds = [];
        }

        await _dbContext.SaveChangesAsync();
        return current;
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return await DeleteOrUpdateAsync([id], false);
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        return _dbSet.AnyAsync(scope => scope.Id == id && scope.TenantId == _userContext.TenantId);
    }

    private async Task EnsureGroupExistsAsync(Guid groupId)
    {
        bool groupExists = await _dbContext.SysDataScopeGroups.AnyAsync(group =>
            group.Id == groupId && group.TenantId == _userContext.TenantId);
        if (!groupExists)
        {
            throw new BusinessException(Localizer.NotFoundResource, StatusCodes.Status404NotFound);
        }
    }

    private static List<Guid> NormalizeTargetIds(IEnumerable<Guid> targetIds, DataScopeType scopeType)
    {
        return scopeType == DataScopeType.Include ? targetIds.Distinct().ToList() : [];
    }

    private static void ValidateScopeType(DataScopeType scopeType)
    {
        if (!Enum.IsDefined(scopeType))
        {
            throw new BusinessException("Invalid data scope type.", StatusCodes.Status400BadRequest);
        }
    }
}
