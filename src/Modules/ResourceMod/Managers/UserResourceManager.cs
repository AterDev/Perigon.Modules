using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using Perigon.PostgreSQL;
using ResourceMod.Models;
using Share.Exceptions;

namespace ResourceMod.Managers;

/// <summary>用户资源提交和公开申请审核。</summary>
public class UserResourceManager(
    AppDbFactory dbContextFactory,
    ILogger<UserResourceManager> logger,
    IUserContext userContext,
    ResourceManager resourceManager
) : ManagerBase<DefaultDbContext, UserResource>(dbContextFactory, userContext, logger)
{
    private readonly ResourceManager _resourceManager = resourceManager;

    public Task<PageList<UserResourceItemDto>> MineAsync(UserResourceFilterDto filter)
        => PageAsync(_dbSet.Where(resource => resource.UserId == _userContext.UserId), filter);

    public Task<PageList<UserResourceItemDto>> ReviewListAsync(UserResourceFilterDto filter)
    {
        EnsureAdmin();
        return PageAsync(_dbSet.Where(resource => resource.StatusValue == (int)UserResourceStatus.ApplyPublic && resource.AuditStatusValue == (int)UserResourceAuditStatus.Pending), filter);
    }

    public async Task<UserResourceDetailDto?> GetAsync(Guid id)
    {
        UserResource? resource = await _dbSet.FirstOrDefaultAsync(item => item.Id == id &&
            (item.UserId == _userContext.UserId || (_userContext.IsAdmin && item.StatusValue == (int)UserResourceStatus.ApplyPublic)));
        if (resource == null) throw new BusinessException("用户资源不存在或无权访问", StatusCodes.Status403Forbidden);
        ResDefinition definition = await _dbContext.ResDefinitions.FirstOrDefaultAsync(item => item.Id == resource.DefinitionId)
            ?? throw new BusinessException("资源定义不存在", StatusCodes.Status409Conflict);
        List<UserResValue> values = await _dbContext.UserResValues.Where(item => item.UserResourceId == id)
            .OrderBy(item => item.CreatedTime).ToListAsync();
        return new UserResourceDetailDto { Id = resource.Id, UserId = resource.UserId, DefinitionId = resource.DefinitionId,
            DefinitionName = definition.Name, Status = resource.Status, AuditStatus = resource.AuditStatus,
            ApprovedResourceId = resource.ApprovedResourceId, ReviewComment = resource.ReviewComment, UpdatedTime = resource.UpdatedTime,
            Values = values.Select(value => new ResourceValueDetailDto { DefinitionPropertyId = value.DefinitionPropertyId,
                Name = value.PropertyNameSnapshot, ValueType = value.ValueTypeSnapshot, Value = value.Value }).ToList() };
    }

    public async Task<UserResource> AddAsync(UserResourceAddDto input)
    {
        ValidateStatus(input.Status);
        List<ResourceValueDto> values = await _resourceManager.ValidateAndNormalizeValuesAsync(_dbContext, input.DefinitionId, input.Values);
        UserResource resource = new() { UserId = _userContext.UserId, DefinitionId = input.DefinitionId, StatusValue = (int)input.Status,
            AuditStatusValue = (int)GetAuditStatus(input.Status), TenantId = _userContext.TenantId };
        List<UserResValue> entities = await BuildValuesAsync(resource.Id, resource.DefinitionId, values);
        return await _dbContext.TransactionAsync(async _ =>
        {
            await _dbSet.InsertAsync(resource);
            if (entities.Count != 0) await _dbContext.UserResValues.BulkInsertAsync(entities);
            return resource;
        });
    }

    public async Task<bool> UpdateAsync(Guid id, UserResourceUpdateDto input)
    {
        ValidateStatus(input.Status);
        UserResource resource = await GetOwnedEntityAsync(id);
        if (resource.AuditStatus == UserResourceAuditStatus.Approved) throw new BusinessException("审核通过的用户资源不能修改", StatusCodes.Status409Conflict);
        List<ResourceValueDto> values = await _resourceManager.ValidateAndNormalizeValuesAsync(_dbContext, input.DefinitionId, input.Values);
        List<UserResValue> entities = await BuildValuesAsync(id, input.DefinitionId, values);
        int status = (int)input.Status;
        int auditStatus = (int)GetAuditStatus(input.Status);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return await _dbContext.TransactionAsync(async _ =>
        {
            await _dbSet.Where(item => item.Id == id).ExecuteUpdateAsync(s => s.Set(item => item.DefinitionId, input.DefinitionId)
                .Set(item => item.StatusValue, status).Set(item => item.AuditStatusValue, auditStatus)
                .Set(item => item.ApprovedResourceId, (Guid?)null).Set(item => item.ReviewComment, (string?)null)
                .Set(item => item.UpdatedTime, now));
            await _dbContext.UserResValues.Where(value => value.UserResourceId == id).ExecuteUpdateAsync(s => s.Set(value => value.IsDeleted, true));
            if (entities.Count != 0) await _dbContext.UserResValues.BulkInsertAsync(entities);
            return true;
        });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        UserResource resource = await GetOwnedEntityAsync(id);
        if (resource.AuditStatus == UserResourceAuditStatus.Approved) throw new BusinessException("审核通过的用户资源不能删除", StatusCodes.Status409Conflict);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return await _dbContext.TransactionAsync(async _ =>
        {
            await _dbContext.UserResValues.Where(value => value.UserResourceId == id).ExecuteUpdateAsync(s => s.Set(value => value.IsDeleted, true).Set(value => value.UpdatedTime, now));
            await _dbSet.Where(item => item.Id == id).ExecuteUpdateAsync(s => s.Set(item => item.IsDeleted, true).Set(item => item.UpdatedTime, now));
            return true;
        });
    }

    public async Task<bool> ApproveAsync(Guid id, UserResourceReviewDto input)
    {
        EnsureAdmin();
        string? comment = NormalizeComment(input.ReviewComment);
        return await _dbContext.TransactionAsync(async _ =>
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            int approvedStatus = (int)UserResourceAuditStatus.Approved;
            int claimed = await _dbSet.Where(resource => resource.Id == id && resource.StatusValue == (int)UserResourceStatus.ApplyPublic && resource.AuditStatusValue == (int)UserResourceAuditStatus.Pending)
                .ExecuteUpdateAsync(s => s.Set(resource => resource.AuditStatusValue, approvedStatus).Set(resource => resource.ReviewComment, comment).Set(resource => resource.UpdatedTime, now));
            if (claimed == 0)
            {
                UserResource? current = await _dbSet.FirstOrDefaultAsync(resource => resource.Id == id);
                if (current == null) throw new BusinessException("公开申请不存在", StatusCodes.Status404NotFound);
                if (current.AuditStatusValue == (int)UserResourceAuditStatus.Approved && current.ApprovedResourceId.HasValue) return true;
                throw new BusinessException("公开申请不存在或已审核", StatusCodes.Status409Conflict);
            }
            UserResource resource = await _dbSet.FirstOrDefaultAsync(item => item.Id == id)
                ?? throw new BusinessException("公开申请不存在或已审核", StatusCodes.Status409Conflict);
            List<UserResValue> values = await _dbContext.UserResValues.Where(value => value.UserResourceId == id).OrderBy(value => value.CreatedTime).ToListAsync();
            Resource created = await _resourceManager.AddAsync(_dbContext, new ResourceAddDto { EnvironmentId = input.EnvironmentId,
                CategoryId = input.CategoryId, GroupId = input.GroupId, DefinitionId = resource.DefinitionId, TagNames = input.TagNames,
                Values = values.Select(value => new ResourceValueDto { DefinitionPropertyId = value.DefinitionPropertyId, Value = value.Value }).ToList() }, saveChanges: false);
            await _dbSet.Where(item => item.Id == id).ExecuteUpdateAsync(s => s.Set(item => item.ApprovedResourceId, created.Id).Set(item => item.UpdatedTime, now));
            return true;
        });
    }

    public async Task<bool> RejectAsync(Guid id, UserResourceRejectDto input)
    {
        EnsureAdmin();
        _ = await GetPendingAsync(id);
        string? comment = NormalizeComment(input.ReviewComment);
        int rejectedStatus = (int)UserResourceAuditStatus.Rejected;
        await _dbSet.Where(item => item.Id == id).ExecuteUpdateAsync(s => s.Set(item => item.AuditStatusValue, rejectedStatus)
            .Set(item => item.ReviewComment, comment).Set(item => item.UpdatedTime, DateTimeOffset.UtcNow));
        return true;
    }

    public override Task<bool> HasPermissionAsync(Guid id) => _dbSet.AnyAsync(resource => resource.Id == id &&
        (resource.UserId == _userContext.UserId || (_userContext.IsAdmin && resource.StatusValue == (int)UserResourceStatus.ApplyPublic)));

    private async Task<PageList<UserResourceItemDto>> PageAsync(IQueryable<UserResource> query, UserResourceFilterDto filter)
    {
        query = query.WhereNotNull(filter.Status, resource => resource.StatusValue == (int?)filter.Status).WhereNotNull(filter.AuditStatus, resource => resource.AuditStatusValue == (int?)filter.AuditStatus);
        int count = await query.CountAsync();
        List<UserResource> resources = await query.OrderByDescending(resource => resource.UpdatedTime).Skip((filter.PageIndex - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        Guid[] definitionIds = resources.Select(item => item.DefinitionId).Distinct().ToArray();
        Dictionary<Guid, string> definitions = (await _dbContext.ResDefinitions.Where(item => definitionIds.Contains(item.Id)).ToListAsync()).ToDictionary(item => item.Id, item => item.Name);
        return new PageList<UserResourceItemDto> { Count = count, PageIndex = filter.PageIndex, Data = resources.Select(resource => new UserResourceItemDto { Id = resource.Id,
            UserId = resource.UserId, DefinitionId = resource.DefinitionId, DefinitionName = definitions[resource.DefinitionId], Status = resource.Status,
            AuditStatus = resource.AuditStatus, ApprovedResourceId = resource.ApprovedResourceId, ReviewComment = resource.ReviewComment, UpdatedTime = resource.UpdatedTime }).ToList() };
    }

    private async Task<UserResource> GetOwnedEntityAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(resource => resource.Id == id && resource.UserId == _userContext.UserId)
            ?? throw new BusinessException("用户资源不存在", StatusCodes.Status404NotFound);
    }

    private async Task<UserResource> GetPendingAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(resource => resource.Id == id && resource.StatusValue == (int)UserResourceStatus.ApplyPublic && resource.AuditStatusValue == (int)UserResourceAuditStatus.Pending)
            ?? throw new BusinessException("公开申请不存在或已审核", StatusCodes.Status409Conflict);
    }

    private async Task<List<UserResValue>> BuildValuesAsync(Guid userResourceId, Guid definitionId, List<ResourceValueDto> normalizedValues)
    {
        List<ResDefinitionPropertyMap> maps = await _dbContext.ResDefinitionPropertyMaps.Where(map => map.DefinitionId == definitionId).OrderBy(map => map.Sort).ToListAsync();
        Guid[] propertyIds = maps.Select(map => map.PropertyId).ToArray();
        Dictionary<Guid, ResDefinitionProperty> lookup = (await _dbContext.ResDefinitionProperties.Where(property => propertyIds.Contains(property.Id)).ToListAsync()).ToDictionary(property => property.Id);
        return normalizedValues.Select(value => { ResDefinitionProperty property = lookup[value.DefinitionPropertyId]; return new UserResValue { UserResourceId = userResourceId,
            DefinitionPropertyId = property.Id, Value = value.Value, PropertyNameSnapshot = property.Name, ValueTypeSnapshotValue = property.ValueTypeValue, TenantId = _userContext.TenantId }; }).ToList();
    }

    private static UserResourceAuditStatus GetAuditStatus(UserResourceStatus status) => status == UserResourceStatus.ApplyPublic ? UserResourceAuditStatus.Pending : UserResourceAuditStatus.NotRequired;
    private static string? NormalizeComment(string? comment) => string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    private static void ValidateStatus(UserResourceStatus status)
    {
        if (status != UserResourceStatus.Private && status != UserResourceStatus.ApplyPublic) throw new BusinessException("用户资源状态无效", StatusCodes.Status400BadRequest);
    }
    private void EnsureAdmin()
    {
        if (!_userContext.IsAdmin) throw new BusinessException("无审核用户资源权限", StatusCodes.Status403Forbidden);
    }
}
