using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using ResourceMod.Models;
using Share.Exceptions;

namespace ResourceMod.Managers;

/// <summary>
/// 用户资源提交和公开申请审核。
/// </summary>
public class UserResourceManager(
    AppDbFactory dbContextFactory,
    ILogger<UserResourceManager> logger,
    IUserContext userContext,
    ResourceManager resourceManager
) : ManagerBase<DefaultDbContext, UserResource>(dbContextFactory, userContext, logger)
{
    private readonly ResourceManager _resourceManager = resourceManager;

    public Task<PageList<UserResourceItemDto>> MineAsync(UserResourceFilterDto filter)
    {
        IQueryable<UserResource> query = _dbSet
            .Include(resource => resource.Definition)
            .Where(resource => resource.UserId == _userContext.UserId);
        return PageAsync(query, filter);
    }

    public Task<PageList<UserResourceItemDto>> ReviewListAsync(UserResourceFilterDto filter)
    {
        EnsureAdmin();

        IQueryable<UserResource> query = _dbSet
            .Include(resource => resource.Definition)
            .Where(resource =>
                resource.Status == UserResourceStatus.ApplyPublic &&
                resource.AuditStatus == UserResourceAuditStatus.Pending);
        return PageAsync(query, filter);
    }

    public async Task<UserResourceDetailDto?> GetAsync(Guid id)
    {
        UserResource? resource = await _dbSet
            .Include(item => item.Definition)
            .Include(item => item.Values)
            .FirstOrDefaultAsync(item =>
                item.Id == id &&
                (item.UserId == _userContext.UserId ||
                 (_userContext.IsAdmin && item.Status == UserResourceStatus.ApplyPublic)));
        if (resource == null)
        {
            throw new BusinessException("用户资源不存在或无权访问", StatusCodes.Status403Forbidden);
        }

        return new UserResourceDetailDto
        {
            Id = resource.Id,
            UserId = resource.UserId,
            DefinitionId = resource.DefinitionId,
            DefinitionName = resource.Definition.Name,
            Status = resource.Status,
            AuditStatus = resource.AuditStatus,
            ApprovedResourceId = resource.ApprovedResourceId,
            ReviewComment = resource.ReviewComment,
            UpdatedTime = resource.UpdatedTime,
            Values = resource.Values
                .OrderBy(value => value.CreatedTime)
                .Select(value => new ResourceValueDetailDto
                {
                    DefinitionPropertyId = value.DefinitionPropertyId,
                    Name = value.PropertyNameSnapshot,
                    ValueType = value.ValueTypeSnapshot,
                    Value = value.Value
                })
                .ToList()
        };
    }

    public async Task<UserResource> AddAsync(UserResourceAddDto input)
    {
        ValidateStatus(input.Status);
        List<ResourceValueDto> normalizedValues = await _resourceManager
            .ValidateAndNormalizeValuesAsync(_dbContext, input.DefinitionId, input.Values);

        UserResource resource = new()
        {
            UserId = _userContext.UserId,
            DefinitionId = input.DefinitionId,
            Status = input.Status,
            AuditStatus = GetAuditStatus(input.Status),
            TenantId = _userContext.TenantId
        };
        resource.Values = await BuildValuesAsync(resource.Id, resource.DefinitionId, normalizedValues);

        await _dbSet.AddAsync(resource);
        await _dbContext.SaveChangesAsync();
        return resource;
    }

    public async Task<bool> UpdateAsync(Guid id, UserResourceUpdateDto input)
    {
        ValidateStatus(input.Status);
        UserResource resource = await GetOwnedEntityAsync(id);
        if (resource.AuditStatus == UserResourceAuditStatus.Approved)
        {
            throw new BusinessException("审核通过的用户资源不能修改", StatusCodes.Status409Conflict);
        }

        List<ResourceValueDto> normalizedValues = await _resourceManager
            .ValidateAndNormalizeValuesAsync(_dbContext, input.DefinitionId, input.Values);
        List<UserResValue> existingValues = await _dbContext.UserResValues
            .Where(value => value.UserResourceId == resource.Id)
            .ToListAsync();
        _dbContext.UserResValues.RemoveRange(existingValues);

        resource.DefinitionId = input.DefinitionId;
        resource.Status = input.Status;
        resource.AuditStatus = GetAuditStatus(input.Status);
        resource.ApprovedResourceId = null;
        resource.ReviewComment = null;
        resource.Values = await BuildValuesAsync(resource.Id, resource.DefinitionId, normalizedValues);
        resource.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.UserResValues.AddRangeAsync(resource.Values);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        UserResource resource = await GetOwnedEntityAsync(id);
        if (resource.AuditStatus == UserResourceAuditStatus.Approved)
        {
            throw new BusinessException("审核通过的用户资源不能删除", StatusCodes.Status409Conflict);
        }

        List<UserResValue> values = await _dbContext.UserResValues
            .Where(value => value.UserResourceId == resource.Id)
            .ToListAsync();
        foreach (UserResValue value in values)
        {
            value.IsDeleted = true;
            value.UpdatedTime = DateTimeOffset.UtcNow;
        }

        resource.IsDeleted = true;
        resource.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveAsync(Guid id, UserResourceReviewDto input)
    {
        EnsureAdmin();
        string? reviewComment = NormalizeComment(input.ReviewComment);

        return await ExecuteInTransactionAsync(async () =>
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            int claimed = await _dbSet
                .Where(resource =>
                    resource.Id == id &&
                    resource.Status == UserResourceStatus.ApplyPublic &&
                    resource.AuditStatus == UserResourceAuditStatus.Pending)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(resource => resource.AuditStatus, UserResourceAuditStatus.Approved)
                    .SetProperty(resource => resource.ReviewComment, reviewComment)
                    .SetProperty(resource => resource.UpdatedTime, now));

            if (claimed == 0)
            {
                UserResource? current = await _dbSet.FirstOrDefaultAsync(resource => resource.Id == id);
                if (current == null)
                {
                    throw new BusinessException("公开申请不存在", StatusCodes.Status404NotFound);
                }

                if (current.AuditStatus == UserResourceAuditStatus.Approved &&
                    current.ApprovedResourceId.HasValue)
                {
                    return true;
                }

                throw new BusinessException("公开申请不存在或已审核", StatusCodes.Status409Conflict);
            }

            UserResource resource = await _dbSet
                .Include(item => item.Values)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.Status == UserResourceStatus.ApplyPublic &&
                    item.AuditStatus == UserResourceAuditStatus.Approved)
                ?? throw new BusinessException("公开申请不存在或已审核", StatusCodes.Status409Conflict);

            Resource created = await _resourceManager.AddAsync(
                _dbContext,
                new ResourceAddDto
                {
                    EnvironmentId = input.EnvironmentId,
                    CategoryId = input.CategoryId,
                    GroupId = input.GroupId,
                    DefinitionId = resource.DefinitionId,
                    TagNames = input.TagNames,
                    Values = resource.Values
                        .OrderBy(value => value.CreatedTime)
                        .Select(value => new ResourceValueDto
                        {
                            DefinitionPropertyId = value.DefinitionPropertyId,
                            Value = value.Value
                        })
                        .ToList()
                },
                saveChanges: false);

            resource.ApprovedResourceId = created.Id;
            resource.UpdatedTime = now;
            await _dbContext.SaveChangesAsync();
            return true;
        });
    }

    public async Task<bool> RejectAsync(Guid id, UserResourceRejectDto input)
    {
        EnsureAdmin();
        UserResource resource = await GetPendingAsync(id);
        resource.AuditStatus = UserResourceAuditStatus.Rejected;
        resource.ReviewComment = NormalizeComment(input.ReviewComment);
        resource.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        return _dbSet.AnyAsync(resource =>
            resource.Id == id &&
            (resource.UserId == _userContext.UserId ||
             (_userContext.IsAdmin && resource.Status == UserResourceStatus.ApplyPublic)));
    }

    private async Task<PageList<UserResourceItemDto>> PageAsync(
        IQueryable<UserResource> query,
        UserResourceFilterDto filter)
    {
        query = query
            .WhereNotNull(filter.Status, resource => resource.Status == filter.Status)
            .WhereNotNull(filter.AuditStatus, resource => resource.AuditStatus == filter.AuditStatus);

        int count = await query.CountAsync();
        List<UserResourceItemDto> data = await query
            .OrderByDescending(resource => resource.UpdatedTime)
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(resource => new UserResourceItemDto
            {
                Id = resource.Id,
                UserId = resource.UserId,
                DefinitionId = resource.DefinitionId,
                DefinitionName = resource.Definition.Name,
                Status = resource.Status,
                AuditStatus = resource.AuditStatus,
                ApprovedResourceId = resource.ApprovedResourceId,
                ReviewComment = resource.ReviewComment,
                UpdatedTime = resource.UpdatedTime
            })
            .ToListAsync();

        return new PageList<UserResourceItemDto>
        {
            Count = count,
            Data = data,
            PageIndex = filter.PageIndex
        };
    }

    private async Task<UserResource> GetOwnedEntityAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(resource =>
                   resource.Id == id && resource.UserId == _userContext.UserId)
               ?? throw new BusinessException("用户资源不存在", StatusCodes.Status404NotFound);
    }

    private async Task<UserResource> GetPendingAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(resource =>
                   resource.Id == id &&
                   resource.Status == UserResourceStatus.ApplyPublic &&
                   resource.AuditStatus == UserResourceAuditStatus.Pending)
               ?? throw new BusinessException("公开申请不存在或已审核", StatusCodes.Status409Conflict);
    }

    private async Task<List<UserResValue>> BuildValuesAsync(
        Guid userResourceId,
        Guid definitionId,
        List<ResourceValueDto> normalizedValues)
    {
        List<ResDefinitionProperty> properties = await _dbContext.ResDefinitionPropertyMaps
            .Where(map =>
                map.DefinitionId == definitionId &&
                map.TenantId == _userContext.TenantId)
            .OrderBy(map => map.Sort)
            .Select(map => map.Property)
            .ToListAsync();
        Dictionary<Guid, ResDefinitionProperty> propertiesById = properties.ToDictionary(property => property.Id);

        return normalizedValues
            .Select(value =>
            {
                ResDefinitionProperty property = propertiesById[value.DefinitionPropertyId];
                return new UserResValue
                {
                    UserResourceId = userResourceId,
                    DefinitionPropertyId = property.Id,
                    Value = value.Value,
                    PropertyNameSnapshot = property.Name,
                    ValueTypeSnapshot = property.ValueType,
                    TenantId = _userContext.TenantId
                };
            })
            .ToList();
    }

    private static UserResourceAuditStatus GetAuditStatus(UserResourceStatus status)
    {
        return status == UserResourceStatus.ApplyPublic
            ? UserResourceAuditStatus.Pending
            : UserResourceAuditStatus.NotRequired;
    }

    private static string? NormalizeComment(string? comment)
    {
        return string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }

    private static void ValidateStatus(UserResourceStatus status)
    {
        if (status != UserResourceStatus.Private &&
            status != UserResourceStatus.ApplyPublic)
        {
            throw new BusinessException("用户资源状态无效", StatusCodes.Status400BadRequest);
        }
    }

    private void EnsureAdmin()
    {
        if (!_userContext.IsAdmin)
        {
            throw new BusinessException("无审核用户资源权限", StatusCodes.Status403Forbidden);
        }
    }
}
