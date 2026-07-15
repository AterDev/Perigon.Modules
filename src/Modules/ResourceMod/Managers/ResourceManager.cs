using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using ResourceMod.Models;
using Share.Exceptions;
using System.Globalization;
using System.Net;

namespace ResourceMod.Managers;

public class ResourceManager(
    TenantDbFactory dbContextFactory,
    ILogger<ResourceManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, Resource>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ResourceItemDto>> FilterAsync(ResourceFilter filter)
    {
        IQueryable<Resource> query = _dbContext.Resources
            .Include(r => r.Environment)
            .Include(r => r.Category)
            .Include(r => r.Group)
            .Include(r => r.Definition)
            .Where(r => r.TenantId == _userContext.TenantId);

        if (!_userContext.IsAdmin)
        {
            List<Guid> roleIds = await GetCurrentRoleIdsAsync();
            query = query.Where(r => _dbContext.ResPermissions.Any(p =>
                p.TenantId == _userContext.TenantId &&
                roleIds.Contains(p.RoleId) &&
                p.EnvironmentId == r.EnvironmentId &&
                p.CategoryId == r.CategoryId));
        }

        query = query
            .WhereNotNull(filter.Name, r => r.Name.Contains(filter.Name!))
            .WhereNotNull(filter.EnvironmentId, r => r.EnvironmentId == filter.EnvironmentId)
            .WhereNotNull(filter.CategoryId, r => r.CategoryId == filter.CategoryId)
            .WhereNotNull(filter.GroupId, r => r.GroupId == filter.GroupId)
            .WhereNotNull(filter.DefinitionId, r => r.DefinitionId == filter.DefinitionId)
            .WhereNotNull(filter.TagName, r => r.TagNames.Contains(filter.TagName!));

        int count = await query.CountAsync();
        List<ResourceItemDto> data = await query
            .OrderByDescending(r => r.UpdatedTime)
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new ResourceItemDto
            {
                Id = r.Id,
                Name = r.Name,
                IconUrl = r.IconUrl,
                EnvironmentId = r.EnvironmentId,
                EnvironmentName = r.Environment.Name,
                CategoryId = r.CategoryId,
                CategoryName = r.Category.Name,
                GroupId = r.GroupId,
                GroupName = r.Group == null ? null : r.Group.Name,
                DefinitionId = r.DefinitionId,
                DefinitionName = r.Definition.Name,
                TagNames = r.TagNames,
                UpdatedTime = r.UpdatedTime
            })
            .ToListAsync();

        return new PageList<ResourceItemDto>
        {
            Count = count,
            Data = data,
            PageIndex = filter.PageIndex
        };
    }

    public async Task<ResourceDetailDto?> GetAsync(Guid id)
    {
        Resource? resource = await GetVisibleQuery()
            .Include(r => r.Environment)
            .Include(r => r.Category)
            .Include(r => r.Group)
            .Include(r => r.Definition)
            .Include(r => r.Values)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (resource == null)
        {
            throw new BusinessException("资源不存在或无权访问", StatusCodes.Status403Forbidden);
        }

        return new ResourceDetailDto
        {
            Id = resource.Id,
            Name = resource.Name,
            IconUrl = resource.IconUrl,
            Description = resource.Description,
            EnvironmentId = resource.EnvironmentId,
            EnvironmentName = resource.Environment.Name,
            CategoryId = resource.CategoryId,
            CategoryName = resource.Category.Name,
            GroupId = resource.GroupId,
            GroupName = resource.Group?.Name,
            DefinitionId = resource.DefinitionId,
            DefinitionName = resource.Definition.Name,
            TagNames = resource.TagNames,
            UpdatedTime = resource.UpdatedTime,
            Values = resource.Values
                .OrderBy(v => v.PropertyNameSnapshot)
                .Select(v => new ResourceValueDetailDto
                {
                    DefinitionPropertyId = v.DefinitionPropertyId,
                    Name = v.PropertyNameSnapshot,
                    ValueType = v.ValueTypeSnapshot,
                    Value = v.Value
                })
                .ToList()
        };
    }

    public async Task<Resource> AddAsync(ResourceInput input)
    {
        EnsureAdmin();

        Resource resource = new()
        {
            Name = input.Name,
            IconUrl = input.IconUrl,
            Description = input.Description,
            EnvironmentId = input.EnvironmentId,
            CategoryId = input.CategoryId,
            GroupId = input.GroupId,
            DefinitionId = input.DefinitionId,
            TagNames = input.TagNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            TenantId = _userContext.TenantId
        };

        await PopulateAndValidateValuesAsync(resource, input.Values);
        await _dbContext.Resources.AddAsync(resource);
        await _dbContext.SaveChangesAsync();

        return resource;
    }

    public async Task<bool> UpdateAsync(Guid id, ResourceInput input)
    {
        EnsureAdmin();

        Resource resource = await FindOwnedAsync(id)
            ?? throw new BusinessException("资源不存在", StatusCodes.Status404NotFound);
        resource.Name = input.Name;
        resource.IconUrl = input.IconUrl;
        resource.Description = input.Description;
        resource.EnvironmentId = input.EnvironmentId;
        resource.CategoryId = input.CategoryId;
        resource.GroupId = input.GroupId;
        resource.DefinitionId = input.DefinitionId;
        resource.TagNames = input.TagNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        await PopulateAndValidateValuesAsync(resource, input.Values);
        resource.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        EnsureAdmin();

        Resource resource = await FindOwnedAsync(id)
            ?? throw new BusinessException("资源不存在", StatusCodes.Status404NotFound);
        resource.IsDeleted = true;
        resource.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        return await GetVisibleQuery().AnyAsync(r => r.Id == id);
    }

    private IQueryable<Resource> GetVisibleQuery()
    {
        IQueryable<Resource> query = _dbContext.Resources
            .Where(r => r.TenantId == _userContext.TenantId);
        if (_userContext.IsAdmin)
        {
            return query;
        }

        List<Guid> roleIds = _dbContext.SystemUserRoles
            .Where(ur => ur.UserId == _userContext.UserId)
            .Select(ur => ur.RoleId)
            .ToList();
        return query.Where(r => _dbContext.ResPermissions.Any(p =>
            p.TenantId == _userContext.TenantId &&
            roleIds.Contains(p.RoleId) &&
            p.EnvironmentId == r.EnvironmentId &&
            p.CategoryId == r.CategoryId));
    }

    private async Task<List<Guid>> GetCurrentRoleIdsAsync()
    {
        return await _dbContext.SystemUserRoles
            .Where(ur => ur.UserId == _userContext.UserId)
            .Select(ur => ur.RoleId)
            .ToListAsync();
    }

    private async Task<Resource?> FindOwnedAsync(Guid id)
    {
        return await _dbContext.Resources
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == _userContext.TenantId);
    }

    private async Task PopulateAndValidateValuesAsync(Resource resource, List<ResourceValueInput> inputs)
    {
        bool environmentExists = await _dbContext.ResEnvironments.AnyAsync(e =>
            e.Id == resource.EnvironmentId && e.TenantId == _userContext.TenantId);
        bool categoryExists = await _dbContext.ResCategories.AnyAsync(c =>
            c.Id == resource.CategoryId && c.TenantId == _userContext.TenantId);
        if (!environmentExists || !categoryExists)
        {
            throw new BusinessException("环境或分类不存在", StatusCodes.Status400BadRequest);
        }

        bool groupIsInvalid = resource.GroupId != null && !await _dbContext.ResGroups.AnyAsync(g =>
            g.Id == resource.GroupId &&
            g.CategoryId == resource.CategoryId &&
            g.TenantId == _userContext.TenantId);
        if (groupIsInvalid)
        {
            throw new BusinessException("资源分组不属于所选分类", StatusCodes.Status400BadRequest);
        }

        List<ResDefinitionProperty> properties = await _dbContext.ResDefinitionProperties
            .Where(p => p.DefinitionId == resource.DefinitionId && p.TenantId == _userContext.TenantId)
            .OrderBy(p => p.Sort)
            .ToListAsync();
        bool definitionDoesNotExist = properties.Count == 0 && !await _dbContext.ResDefinitions.AnyAsync(d =>
            d.Id == resource.DefinitionId && d.TenantId == _userContext.TenantId);
        if (definitionDoesNotExist)
        {
            throw new BusinessException("资源定义不存在", StatusCodes.Status400BadRequest);
        }

        bool containsDuplicateOrUnknownValue = inputs
            .Select(v => v.DefinitionPropertyId)
            .Distinct()
            .Count() != inputs.Count ||
            inputs.Any(v => !properties.Any(p => p.Id == v.DefinitionPropertyId));
        if (containsDuplicateOrUnknownValue)
        {
            throw new BusinessException("资源属性包含重复或未知字段", StatusCodes.Status400BadRequest);
        }

        bool missesRequiredValue = properties.Any(p =>
            p.IsRequired && inputs.All(v => v.DefinitionPropertyId != p.Id));
        if (missesRequiredValue)
        {
            throw new BusinessException("缺少必填资源属性", StatusCodes.Status400BadRequest);
        }

        await _dbContext.ResValues
            .Where(value => value.ResourceId == resource.Id && value.TenantId == _userContext.TenantId)
            .ExecuteDeleteAsync();
        resource.Values = [];
        foreach (ResourceValueInput input in inputs)
        {
            ResDefinitionProperty property = properties.Single(p => p.Id == input.DefinitionPropertyId);
            if (input.Value.Length > Math.Min(property.MaxLength, 1000))
            {
                throw new BusinessException(
                    $"属性 {property.Name} 超过最大长度",
                    StatusCodes.Status400BadRequest);
            }

            ResValue value = new()
            {
                ResourceId = resource.Id,
                DefinitionPropertyId = property.Id,
                Value = NormalizeValue(input.Value, property.ValueType),
                PropertyNameSnapshot = property.Name,
                ValueTypeSnapshot = property.ValueType,
                TenantId = _userContext.TenantId
            };
            _dbContext.ResValues.Add(value);
            resource.Values.Add(value);
        }
    }

    private static string NormalizeValue(string value, ResValueType type)
    {
        return type switch
        {
            ResValueType.String => value,
            ResValueType.Number when decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal number) => number.ToString(CultureInfo.InvariantCulture),
            ResValueType.Boolean when bool.TryParse(value, out bool boolean) =>
                boolean.ToString().ToLowerInvariant(),
            ResValueType.Date when DateOnly.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ResValueType.Uri when Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) => uri.AbsoluteUri,
            ResValueType.IPAddress when IPAddress.TryParse(value, out IPAddress? address) => address.ToString(),
            _ => throw new BusinessException("资源属性值格式无效", StatusCodes.Status400BadRequest)
        };
    }

    private void EnsureAdmin()
    {
        if (!_userContext.IsAdmin)
        {
            throw new BusinessException("无管理资源权限", StatusCodes.Status403Forbidden);
        }
    }
}
