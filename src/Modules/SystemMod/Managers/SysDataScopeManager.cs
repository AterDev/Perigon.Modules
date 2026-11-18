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
            .WhereNotNull(filter.ScopeType, scope => scope.ScopeTypeValue == (int)filter.ScopeType!.Value)
            .WhereNotNull(filter.GroupId, scope => scope.GroupId == filter.GroupId);

        return await PageListAsync<SysDataScopeFilterDto, SysDataScopeItemDto>(filter, MapItem);
    }

    public async Task<SysDataScopeDetailDto?> GetAsync(Guid id)
    {
        SysDataScope? entity = await _dbSet
            .Where(scope => scope.Id == id)
            .SingleOrDefaultAsync();

        return entity is null ? null : MapDetail(entity);
    }

    public async Task<SysDataScope> AddAsync(SysDataScopeAddDto dto)
    {
        await EnsureGroupExistsAsync(dto.GroupId);
        ValidateScopeType(dto.ScopeType);

        SysDataScope entity = new() { Name = dto.Name, ResourceCode = dto.ResourceCode, TargetIds = dto.TargetIds, ScopeType = dto.ScopeType, GroupId = dto.GroupId };
        entity.Name = dto.Name.Trim();
        entity.ResourceCode = dto.ResourceCode.Trim();
        entity.TargetIds = NormalizeTargetIds(dto.TargetIds, dto.ScopeType);
        await InsertAsync(entity);
        return entity;
    }

    public async Task<SysDataScope> UpdateAsync(Guid id, SysDataScopeUpdateDto dto)
    {
        SysDataScope current = await _dbSet.Where(scope => scope.Id == id).SingleOrDefaultAsync()
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

        await _dbSet
            .Where(scope => scope.Id == id && scope.TenantId == _userContext.TenantId)
            .ExecuteUpdateAsync(update => update
                .Set(scope => scope.GroupId, current.GroupId)
                .Set(scope => scope.Name, current.Name)
                .Set(scope => scope.ResourceCode, current.ResourceCode)
                .Set(scope => scope.ScopeTypeValue, current.ScopeTypeValue)
                .Set(scope => scope.TargetIds, current.TargetIds));
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
        return _dbSet.Where(scope => scope.Id == id && scope.TenantId == _userContext.TenantId).AnyAsync();
    }

    private async Task EnsureGroupExistsAsync(Guid groupId)
    {
        bool groupExists = await _dbContext.SysDataScopeGroups
            .Where(group => group.Id == groupId && group.TenantId == _userContext.TenantId)
            .AnyAsync();
        if (!groupExists)
        {
            throw new BusinessException(Localizer.NotFoundResource, StatusCodes.Status404NotFound);
        }
    }

    private static List<Guid> NormalizeTargetIds(IEnumerable<Guid> targetIds, DataScopeType scopeType)
    {
        return scopeType == DataScopeType.Include ? targetIds.Distinct().ToList() : [];
    }

    private static SysDataScopeItemDto MapItem(SysDataScope x) => new() { Id = x.Id, Name = x.Name, ResourceCode = x.ResourceCode, TargetIds = x.TargetIds, ScopeType = x.ScopeType, GroupId = x.GroupId };
    private static SysDataScopeDetailDto MapDetail(SysDataScope x) => new() { Id = x.Id, Name = x.Name, ResourceCode = x.ResourceCode, TargetIds = x.TargetIds, ScopeType = x.ScopeType, GroupId = x.GroupId };

    private static void ValidateScopeType(DataScopeType scopeType)
    {
        if (!Enum.IsDefined(scopeType))
        {
            throw new BusinessException("Invalid data scope type.", StatusCodes.Status400BadRequest);
        }
    }
}
