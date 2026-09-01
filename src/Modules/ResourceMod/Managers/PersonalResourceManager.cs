using System.Text.Json;
using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using ResourceMod.Models;
using Share.Exceptions;

namespace ResourceMod.Managers;

/// <summary>
/// 个人资源提交和公开申请审核。
/// </summary>
public class PersonalResourceManager(
    AppDbFactory dbContextFactory,
    ILogger<PersonalResourceManager> logger,
    IUserContext userContext,
    ResourceManager resourceManager
) : ManagerBase<DefaultDbContext, PersonalResource>(dbContextFactory, userContext, logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ResourceManager _resourceManager = resourceManager;

    public Task<PageList<PersonalResourceItemDto>> MineAsync(PersonalResourceFilterDto filter)
    {
        IQueryable<PersonalResource> query = _dbSet
            .Include(resource => resource.Definition)
            .Where(resource => resource.UserId == _userContext.UserId);
        return PageAsync(query, filter);
    }

    public Task<PageList<PersonalResourceItemDto>> ReviewListAsync(PersonalResourceFilterDto filter)
    {
        EnsureAdmin();

        IQueryable<PersonalResource> query = _dbSet
            .Include(resource => resource.Definition)
            .Where(resource =>
                resource.Status == PersonalResourceStatus.ApplyPublic &&
                resource.AuditStatus == PersonalResourceAuditStatus.Pending);
        return PageAsync(query, filter);
    }

    public async Task<PersonalResourceDetailDto?> GetAsync(Guid id)
    {
        PersonalResource? resource = await _dbSet
            .Include(item => item.Definition)
            .FirstOrDefaultAsync(item =>
                item.Id == id &&
                (item.UserId == _userContext.UserId ||
                 (_userContext.IsAdmin && item.Status == PersonalResourceStatus.ApplyPublic)));
        if (resource == null)
        {
            throw new BusinessException("个人资源不存在或无权访问", StatusCodes.Status403Forbidden);
        }

        return new PersonalResourceDetailDto
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
            Values = await BuildValueDetailsAsync(resource.DefinitionId, resource.ValuesJson)
        };
    }

    public async Task<PersonalResource> AddAsync(PersonalResourceAddDto input)
    {
        ValidateStatus(input.Status);
        List<ResourceValueDto> values =
            await _resourceManager.ValidateAndNormalizeValuesAsync(input.DefinitionId, input.Values);

        PersonalResource resource = new()
        {
            UserId = _userContext.UserId,
            DefinitionId = input.DefinitionId,
            Status = input.Status,
            AuditStatus = GetAuditStatus(input.Status),
            ValuesJson = JsonSerializer.Serialize(values, JsonOptions),
            TenantId = _userContext.TenantId
        };
        await _dbSet.AddAsync(resource);
        await _dbContext.SaveChangesAsync();
        return resource;
    }

    public async Task<bool> UpdateAsync(Guid id, PersonalResourceUpdateDto input)
    {
        ValidateStatus(input.Status);
        PersonalResource resource = await GetOwnedEntityAsync(id);
        if (resource.AuditStatus == PersonalResourceAuditStatus.Approved)
        {
            throw new BusinessException("审核通过的个人资源不能修改", StatusCodes.Status409Conflict);
        }

        List<ResourceValueDto> values =
            await _resourceManager.ValidateAndNormalizeValuesAsync(input.DefinitionId, input.Values);
        resource.DefinitionId = input.DefinitionId;
        resource.Status = input.Status;
        resource.AuditStatus = GetAuditStatus(input.Status);
        resource.ApprovedResourceId = null;
        resource.ReviewComment = null;
        resource.ValuesJson = JsonSerializer.Serialize(values, JsonOptions);
        resource.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        PersonalResource resource = await GetOwnedEntityAsync(id);
        if (resource.AuditStatus == PersonalResourceAuditStatus.Approved)
        {
            throw new BusinessException("审核通过的个人资源不能删除", StatusCodes.Status409Conflict);
        }

        resource.IsDeleted = true;
        resource.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveAsync(Guid id, PersonalResourceReviewDto input)
    {
        EnsureAdmin();
        PersonalResource resource = await GetPendingAsync(id);
        List<ResourceValueDto> values = DeserializeValues(resource.ValuesJson);
        Resource created = await _resourceManager.AddAsync(new ResourceAddDto
        {
            EnvironmentId = input.EnvironmentId,
            CategoryId = input.CategoryId,
            GroupId = input.GroupId,
            DefinitionId = resource.DefinitionId,
            TagNames = input.TagNames,
            Values = values
        });

        resource.Status = PersonalResourceStatus.ApplyPublic;
        resource.AuditStatus = PersonalResourceAuditStatus.Approved;
        resource.ApprovedResourceId = created.Id;
        resource.ReviewComment = input.ReviewComment;
        resource.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectAsync(Guid id, PersonalResourceRejectDto input)
    {
        EnsureAdmin();
        PersonalResource resource = await GetPendingAsync(id);
        resource.AuditStatus = PersonalResourceAuditStatus.Rejected;
        resource.ReviewComment = input.ReviewComment;
        resource.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        return _dbSet.AnyAsync(resource =>
            resource.Id == id &&
            (resource.UserId == _userContext.UserId ||
             (_userContext.IsAdmin && resource.Status == PersonalResourceStatus.ApplyPublic)));
    }

    private async Task<PageList<PersonalResourceItemDto>> PageAsync(
        IQueryable<PersonalResource> query,
        PersonalResourceFilterDto filter)
    {
        query = query
            .WhereNotNull(filter.Status, resource => resource.Status == filter.Status)
            .WhereNotNull(filter.AuditStatus, resource => resource.AuditStatus == filter.AuditStatus);

        int count = await query.CountAsync();
        List<PersonalResourceItemDto> data = await query
            .OrderByDescending(resource => resource.UpdatedTime)
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(resource => new PersonalResourceItemDto
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

        return new PageList<PersonalResourceItemDto>
        {
            Count = count,
            Data = data,
            PageIndex = filter.PageIndex
        };
    }

    private async Task<PersonalResource> GetOwnedEntityAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(resource =>
                   resource.Id == id && resource.UserId == _userContext.UserId)
               ?? throw new BusinessException("个人资源不存在", StatusCodes.Status404NotFound);
    }

    private async Task<PersonalResource> GetPendingAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(resource =>
                   resource.Id == id &&
                   resource.Status == PersonalResourceStatus.ApplyPublic &&
                   resource.AuditStatus == PersonalResourceAuditStatus.Pending)
               ?? throw new BusinessException("公开申请不存在或已审核", StatusCodes.Status409Conflict);
    }

    private async Task<List<ResourceValueDetailDto>> BuildValueDetailsAsync(
        Guid definitionId,
        string valuesJson)
    {
        List<ResourceValueDto> values = DeserializeValues(valuesJson);
        Dictionary<Guid, ResDefinitionProperty> properties = await _dbContext.ResDefinitionPropertyMaps
            .Where(map => map.DefinitionId == definitionId)
            .Select(map => map.Property)
            .ToDictionaryAsync(property => property.Id);

        return values
            .Where(value => properties.ContainsKey(value.DefinitionPropertyId))
            .Select(value =>
            {
                ResDefinitionProperty property = properties[value.DefinitionPropertyId];
                return new ResourceValueDetailDto
                {
                    DefinitionPropertyId = property.Id,
                    Name = property.Name,
                    ValueType = property.ValueType,
                    Value = value.Value
                };
            })
            .ToList();
    }

    private static List<ResourceValueDto> DeserializeValues(string valuesJson)
    {
        return JsonSerializer.Deserialize<List<ResourceValueDto>>(valuesJson, JsonOptions) ?? [];
    }

    private static PersonalResourceAuditStatus GetAuditStatus(PersonalResourceStatus status)
    {
        return status == PersonalResourceStatus.ApplyPublic
            ? PersonalResourceAuditStatus.Pending
            : PersonalResourceAuditStatus.NotRequired;
    }

    private static void ValidateStatus(PersonalResourceStatus status)
    {
        if (status != PersonalResourceStatus.Private &&
            status != PersonalResourceStatus.ApplyPublic)
        {
            throw new BusinessException("个人资源状态无效", StatusCodes.Status400BadRequest);
        }
    }

    private void EnsureAdmin()
    {
        if (!_userContext.IsAdmin)
        {
            throw new BusinessException("无审核个人资源权限", StatusCodes.Status403Forbidden);
        }
    }
}
