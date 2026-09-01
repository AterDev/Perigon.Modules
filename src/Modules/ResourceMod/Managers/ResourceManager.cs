using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using ResourceMod.Models;
using Share.Exceptions;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace ResourceMod.Managers;

public class ResourceManager(
    AppDbFactory dbContextFactory,
    ILogger<ResourceManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, Resource>(dbContextFactory, userContext, logger)
{
    private static readonly Regex NumberRegex = new(
        @"^[+-]?(?:\d+(?:\.\d*)?|\.\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<PageList<ResourceItemDto>> FilterAsync(ResourceFilterDto filter)
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
            .WhereNotNull(filter.EnvironmentId, r => r.EnvironmentId == filter.EnvironmentId)
            .WhereNotNull(filter.CategoryId, r => r.CategoryId == filter.CategoryId)
            .WhereNotNull(filter.GroupId, r => r.GroupId == filter.GroupId)
            .WhereNotNull(filter.DefinitionId, r => r.DefinitionId == filter.DefinitionId)
            .WhereNotNull(filter.TagName, r => r.TagNames.Contains(filter.TagName!));

        string searchKey = filter.SearchKey?.Trim() ?? string.Empty;
        if (searchKey.Length >= 2)
        {
            query = query.Where(r =>
                r.Definition.Name.Contains(searchKey) ||
                r.TagNames.Any(tag => tag.Contains(searchKey)) ||
                r.Values.Any(value => value.Value.Contains(searchKey)));
        }

        int count = await query.CountAsync();
        List<ResourceItemDto> data = await query
            .OrderByDescending(r => r.UpdatedTime)
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new ResourceItemDto
            {
                Id = r.Id,
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

    public async Task<Resource> AddAsync(ResourceAddDto input)
    {
        EnsureAdmin();

        Resource resource = new()
        {
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

    public async Task<bool> UpdateAsync(Guid id, ResourceUpdateDto input)
    {
        EnsureAdmin();

        Resource resource = await FindOwnedAsync(id)
            ?? throw new BusinessException("资源不存在", StatusCodes.Status404NotFound);
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

    private async Task PopulateAndValidateValuesAsync(Resource resource, List<ResourceValueDto> inputs)
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

        List<ResourceValueDto> normalizedValues =
            await ValidateAndNormalizeValuesAsync(resource.DefinitionId, inputs);

        List<ResValue> values = normalizedValues
            .Select(input => new ResValue
            {
                ResourceId = resource.Id,
                DefinitionPropertyId = input.DefinitionPropertyId,
                Value = input.Value,
                PropertyNameSnapshot = string.Empty,
                ValueTypeSnapshot = default,
                TenantId = _userContext.TenantId
            })
            .ToList();

        List<ResDefinitionProperty> properties = await _dbContext.ResDefinitionPropertyMaps
            .Where(map =>
                map.DefinitionId == resource.DefinitionId &&
                map.TenantId == _userContext.TenantId)
            .OrderBy(map => map.Sort)
            .Select(map => map.Property)
            .ToListAsync();
        Dictionary<Guid, ResDefinitionProperty> propertiesById = properties.ToDictionary(p => p.Id);
        foreach (ResValue value in values)
        {
            ResDefinitionProperty property = propertiesById[value.DefinitionPropertyId];
            value.PropertyNameSnapshot = property.Name;
            value.ValueTypeSnapshot = property.ValueType;
        }

        List<ResValue> existingValues = await _dbContext.ResValues
            .Where(value => value.ResourceId == resource.Id && value.TenantId == _userContext.TenantId)
            .ToListAsync();
        _dbContext.ResValues.RemoveRange(existingValues);
        resource.Values = values;
        await _dbContext.ResValues.AddRangeAsync(values);
    }

    public async Task<List<ResourceValueDto>> ValidateAndNormalizeValuesAsync(
        Guid definitionId,
        List<ResourceValueDto> inputs)
    {
        List<ResDefinitionProperty> properties = await _dbContext.ResDefinitionPropertyMaps
            .Where(map =>
                map.DefinitionId == definitionId &&
                map.TenantId == _userContext.TenantId)
            .OrderBy(map => map.Sort)
            .Select(map => map.Property)
            .ToListAsync();
        bool definitionDoesNotExist = properties.Count == 0 && !await _dbContext.ResDefinitions.AnyAsync(d =>
            d.Id == definitionId && d.TenantId == _userContext.TenantId);
        if (definitionDoesNotExist)
        {
            throw new BusinessException("资源定义不存在", StatusCodes.Status400BadRequest);
        }

        Dictionary<Guid, ResDefinitionProperty> propertiesById = properties.ToDictionary(p => p.Id);
        bool containsDuplicateOrUnknownValue = inputs
            .Select(v => v.DefinitionPropertyId)
            .Distinct()
            .Count() != inputs.Count ||
            inputs.Any(v => !propertiesById.ContainsKey(v.DefinitionPropertyId));
        if (containsDuplicateOrUnknownValue)
        {
            throw new BusinessException("资源属性包含重复或未知字段", StatusCodes.Status400BadRequest);
        }

        ResDefinitionProperty? missingRequiredProperty = properties.FirstOrDefault(p =>
            p.IsRequired && inputs.All(v =>
                v.DefinitionPropertyId != p.Id || string.IsNullOrEmpty(v.Value)));
        if (missingRequiredProperty != null)
        {
            throw new BusinessException(
                $"属性 {missingRequiredProperty.Name} 为必填项",
                StatusCodes.Status400BadRequest);
        }

        List<ResourceValueDto> normalizedValues = [];
        foreach (ResourceValueDto input in inputs)
        {
            ResDefinitionProperty property = propertiesById[input.DefinitionPropertyId];
            if (string.IsNullOrEmpty(input.Value))
            {
                continue;
            }

            if (input.Value.Length > Math.Min(property.MaxLength, 1000))
            {
                throw new BusinessException(
                    $"属性 {property.Name} 超过最大长度",
                    StatusCodes.Status400BadRequest);
            }

            normalizedValues.Add(new ResourceValueDto
            {
                DefinitionPropertyId = property.Id,
                Value = NormalizeValue(input.Value, property.ValueType, property.Name)
            });
        }

        return normalizedValues;
    }

    private static string NormalizeValue(string value, ResValueType type, string propertyName)
    {
        string trimmedValue = value.Trim();
        return type switch
        {
            ResValueType.String => value,
            ResValueType.Number when NumberRegex.IsMatch(trimmedValue) && decimal.TryParse(
                trimmedValue,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out decimal number) => number.ToString(CultureInfo.InvariantCulture),
            ResValueType.Boolean when bool.TryParse(trimmedValue, out bool boolean) =>
                boolean.ToString().ToLowerInvariant(),
            ResValueType.Date when DateOnly.TryParseExact(
                trimmedValue,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ResValueType.Uri when Uri.TryCreate(trimmedValue, UriKind.Absolute, out Uri? uri) => uri.AbsoluteUri,
            ResValueType.IPAddress when IPAddress.TryParse(trimmedValue, out IPAddress? address) => address.ToString(),
            _ => throw new BusinessException(
                $"属性 {propertyName} 的值格式无效",
                StatusCodes.Status400BadRequest)
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
