using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using Perigon.PostgreSQL;
using ResourceMod.Models;
using Share.Exceptions;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace ResourceMod.Managers;

/// <summary>常规资源查询及管理。所有关系数据均通过显式查询加载。</summary>
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
        IQueryable<Resource> query = GetVisibleQuery(_dbContext);
        string searchKey = filter.SearchKey?.Trim() ?? string.Empty;
        query = query
            .WhereNotNull(filter.EnvironmentId, r => r.EnvironmentId == filter.EnvironmentId)
            .WhereNotNull(filter.CategoryId, r => r.CategoryId == filter.CategoryId)
            .WhereNotNull(filter.GroupId, r => r.GroupId == filter.GroupId)
            .WhereNotNull(filter.DefinitionId, r => r.DefinitionId == filter.DefinitionId)
            .WhereNotNull(filter.TagName, r => r.TagNames.Contains(filter.TagName!));

        if (searchKey.Length >= 2)
        {
            query = query.Where(resource =>
                _dbContext.ResDefinitions.Any(definition =>
                    definition.Id == resource.DefinitionId && definition.Name.Contains(searchKey))
                || _dbContext.ResValues.Any(value =>
                    value.ResourceId == resource.Id && value.Value.Contains(searchKey))
                || resource.TagNames.Any(tag => tag.Contains(searchKey)));
        }

        int count = await query.CountAsync();
        List<Resource> resources = await query.OrderByDescending(r => r.UpdatedTime)
            .Skip((filter.PageIndex - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return new PageList<ResourceItemDto> { Count = count, PageIndex = filter.PageIndex, Data = await ToItemsAsync(resources) };
    }

    public async Task<ResourceDetailDto?> GetAsync(Guid id)
    {
        Resource? resource = await GetVisibleQuery(_dbContext).FirstOrDefaultAsync(r => r.Id == id);
        if (resource == null) throw new BusinessException("资源不存在或无权访问", StatusCodes.Status403Forbidden);
        ResEnvironment environment = await _dbContext.ResEnvironments.FirstOrDefaultAsync(item => item.Id == resource.EnvironmentId)
            ?? throw new BusinessException("环境不存在", StatusCodes.Status409Conflict);
        ResCategory category = await _dbContext.ResCategories.FirstOrDefaultAsync(item => item.Id == resource.CategoryId)
            ?? throw new BusinessException("分类不存在", StatusCodes.Status409Conflict);
        ResGroup? group = resource.GroupId.HasValue ? await _dbContext.ResGroups.FirstOrDefaultAsync(item => item.Id == resource.GroupId.Value) : null;
        ResDefinition definition = await _dbContext.ResDefinitions.FirstOrDefaultAsync(item => item.Id == resource.DefinitionId)
            ?? throw new BusinessException("资源定义不存在", StatusCodes.Status409Conflict);
        List<ResValue> values = await _dbContext.ResValues.Where(value => value.ResourceId == resource.Id)
            .OrderBy(value => value.PropertyNameSnapshot).ToListAsync();
        return new ResourceDetailDto
        {
            Id = resource.Id, EnvironmentId = resource.EnvironmentId, EnvironmentName = environment.Name,
            CategoryId = resource.CategoryId, CategoryName = category.Name, GroupId = resource.GroupId,
            GroupName = group?.Name, DefinitionId = resource.DefinitionId, DefinitionName = definition.Name,
            TagNames = resource.TagNames.ToList(), UpdatedTime = resource.UpdatedTime,
            Values = values.Select(value => new ResourceValueDetailDto { DefinitionPropertyId = value.DefinitionPropertyId,
                Name = value.PropertyNameSnapshot, ValueType = value.ValueTypeSnapshot, Value = value.Value }).ToList()
        };
    }

    public Task<Resource> AddAsync(ResourceAddDto input) =>
        _dbContext.TransactionAsync(_ => AddAsync(_dbContext, input, saveChanges: false));

    /// <summary>在指定上下文中创建资源；调用方可将它放入同一事务。</summary>
    internal async Task<Resource> AddAsync(DefaultDbContext dbContext, ResourceAddDto input, bool saveChanges)
    {
        EnsureAdmin();
        Resource resource = new() { EnvironmentId = input.EnvironmentId, CategoryId = input.CategoryId,
            GroupId = input.GroupId, DefinitionId = input.DefinitionId,
            TagNames = input.TagNames.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(), TenantId = _userContext.TenantId };
        List<ResValue> values = await BuildAndValidateValuesAsync(dbContext, resource, input.Values);
        await dbContext.Resources.InsertAsync(resource);
        if (values.Count != 0) await dbContext.ResValues.BulkInsertAsync(values);
        return resource;
    }

    public async Task<bool> UpdateAsync(Guid id, ResourceUpdateDto input)
    {
        EnsureAdmin();
        Resource resource = await FindOwnedAsync(id) ?? throw new BusinessException("资源不存在", StatusCodes.Status404NotFound);
        resource.EnvironmentId = input.EnvironmentId;
        resource.CategoryId = input.CategoryId;
        resource.GroupId = input.GroupId;
        resource.DefinitionId = input.DefinitionId;
        string[] tagNames = input.TagNames.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return await _dbContext.TransactionAsync(async _ =>
        {
            List<ResValue> values = await BuildAndValidateValuesAsync(_dbContext, resource, input.Values);
            await _dbContext.Resources.Where(item => item.Id == id).ExecuteUpdateAsync(setter => setter
                .Set(item => item.EnvironmentId, input.EnvironmentId).Set(item => item.CategoryId, input.CategoryId)
                .Set(item => item.GroupId, input.GroupId).Set(item => item.DefinitionId, input.DefinitionId)
                .Set(item => item.TagNames, tagNames)
                .Set(item => item.UpdatedTime, DateTimeOffset.UtcNow));
            await _dbContext.ResValues.Where(value => value.ResourceId == id).ExecuteUpdateAsync(s => s.Set(value => value.IsDeleted, true));
            if (values.Count != 0) await _dbContext.ResValues.BulkInsertAsync(values);
            return true;
        });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        EnsureAdmin();
        _ = await FindOwnedAsync(id) ?? throw new BusinessException("资源不存在", StatusCodes.Status404NotFound);
        await _dbContext.Resources.Where(item => item.Id == id).ExecuteUpdateAsync(setter => setter.Set(item => item.IsDeleted, true));
        return true;
    }

    public override Task<bool> HasPermissionAsync(Guid id) => GetVisibleQuery(_dbContext).AnyAsync(r => r.Id == id);

    internal IQueryable<Resource> GetVisibleQuery(DefaultDbContext dbContext)
    {
        IQueryable<Resource> query = dbContext.Resources.Where(r => r.TenantId == _userContext.TenantId);
        if (_userContext.IsAdmin) return query;
        Guid[] roleIds = _userContext.RoleIds.ToArray();
        return query.Where(resource => dbContext.ResPermissions.Any(permission =>
            permission.TenantId == _userContext.TenantId && roleIds.Contains(permission.RoleId) &&
            permission.EnvironmentId == resource.EnvironmentId && permission.CategoryId == resource.CategoryId));
    }

    private Task<Resource?> FindOwnedAsync(Guid id) => _dbContext.Resources.FirstOrDefaultAsync(item => item.Id == id && item.TenantId == _userContext.TenantId);

    private async Task<List<ResourceItemDto>> ToItemsAsync(List<Resource> resources)
    {
        if (resources.Count == 0) return [];
        Guid[] environmentIds = resources.Select(item => item.EnvironmentId).Distinct().ToArray();
        Guid[] categoryIds = resources.Select(item => item.CategoryId).Distinct().ToArray();
        Guid[] groupIds = resources.Where(item => item.GroupId.HasValue).Select(item => item.GroupId!.Value).Distinct().ToArray();
        Guid[] definitionIds = resources.Select(item => item.DefinitionId).Distinct().ToArray();
        Dictionary<Guid, ResEnvironment> environments = (await _dbContext.ResEnvironments.Where(item => environmentIds.Contains(item.Id)).ToListAsync()).ToDictionary(item => item.Id);
        Dictionary<Guid, ResCategory> categories = (await _dbContext.ResCategories.Where(item => categoryIds.Contains(item.Id)).ToListAsync()).ToDictionary(item => item.Id);
        Dictionary<Guid, ResGroup> groups = (await _dbContext.ResGroups.Where(item => groupIds.Contains(item.Id)).ToListAsync()).ToDictionary(item => item.Id);
        Dictionary<Guid, ResDefinition> definitions = (await _dbContext.ResDefinitions.Where(item => definitionIds.Contains(item.Id)).ToListAsync()).ToDictionary(item => item.Id);
        return resources.Select(resource => new ResourceItemDto
        {
            Id = resource.Id, EnvironmentId = resource.EnvironmentId, EnvironmentName = environments[resource.EnvironmentId].Name,
            CategoryId = resource.CategoryId, CategoryName = categories[resource.CategoryId].Name, GroupId = resource.GroupId,
            GroupName = resource.GroupId.HasValue && groups.TryGetValue(resource.GroupId.Value, out ResGroup? group) ? group.Name : null,
            DefinitionId = resource.DefinitionId, DefinitionName = definitions[resource.DefinitionId].Name,
            TagNames = resource.TagNames.ToList(), UpdatedTime = resource.UpdatedTime
        }).ToList();
    }

    private async Task<List<ResValue>> BuildAndValidateValuesAsync(DefaultDbContext dbContext, Resource resource, List<ResourceValueDto> inputs)
    {
        bool environmentExists = await dbContext.ResEnvironments.AnyAsync(e => e.Id == resource.EnvironmentId);
        bool categoryExists = await dbContext.ResCategories.AnyAsync(c => c.Id == resource.CategoryId);
        if (!environmentExists || !categoryExists) throw new BusinessException("环境或分类不存在", StatusCodes.Status400BadRequest);
        bool groupIsInvalid = resource.GroupId != null && !await dbContext.ResGroups.AnyAsync(g => g.Id == resource.GroupId && g.CategoryId == resource.CategoryId);
        if (groupIsInvalid) throw new BusinessException("资源分组不属于所选分类", StatusCodes.Status400BadRequest);
        List<ResourceValueDto> normalizedValues = await ValidateAndNormalizeValuesAsync(dbContext, resource.DefinitionId, inputs);
        List<ResDefinitionPropertyMap> maps = await dbContext.ResDefinitionPropertyMaps.Where(map => map.DefinitionId == resource.DefinitionId)
            .OrderBy(map => map.Sort).ToListAsync();
        Guid[] propertyIds = maps.Select(map => map.PropertyId).ToArray();
        Dictionary<Guid, ResDefinitionProperty> propertyLookup = (await dbContext.ResDefinitionProperties.Where(property => propertyIds.Contains(property.Id)).ToListAsync()).ToDictionary(property => property.Id);
        List<ResDefinitionProperty> properties = maps.Select(map => propertyLookup[map.PropertyId]).ToList();
        Dictionary<Guid, ResDefinitionProperty> propertiesById = properties.ToDictionary(item => item.Id);
        return normalizedValues.Select(input => { ResDefinitionProperty property = propertiesById[input.DefinitionPropertyId]; return new ResValue
        {
            ResourceId = resource.Id, DefinitionPropertyId = property.Id, Value = input.Value,
            PropertyNameSnapshot = property.Name, ValueTypeSnapshotValue = property.ValueTypeValue, TenantId = _userContext.TenantId
        }; }).ToList();
    }

    public Task<List<ResourceValueDto>> ValidateAndNormalizeValuesAsync(Guid definitionId, List<ResourceValueDto> inputs) => ValidateAndNormalizeValuesAsync(_dbContext, definitionId, inputs);

    internal async Task<List<ResourceValueDto>> ValidateAndNormalizeValuesAsync(DefaultDbContext dbContext, Guid definitionId, List<ResourceValueDto> inputs)
    {
        List<ResDefinitionPropertyMap> maps = await dbContext.ResDefinitionPropertyMaps.Where(map => map.DefinitionId == definitionId)
            .OrderBy(map => map.Sort).ToListAsync();
        Guid[] propertyIds = maps.Select(map => map.PropertyId).ToArray();
        Dictionary<Guid, ResDefinitionProperty> propertyLookup = (await dbContext.ResDefinitionProperties.Where(property => propertyIds.Contains(property.Id)).ToListAsync()).ToDictionary(property => property.Id);
        List<ResDefinitionProperty> properties = maps.Select(map => propertyLookup[map.PropertyId]).ToList();
        bool definitionDoesNotExist = properties.Count == 0 && !await dbContext.ResDefinitions.AnyAsync(d => d.Id == definitionId);
        if (definitionDoesNotExist) throw new BusinessException("资源定义不存在", StatusCodes.Status400BadRequest);
        Dictionary<Guid, ResDefinitionProperty> propertiesById = properties.ToDictionary(property => property.Id);
        if (inputs.Select(v => v.DefinitionPropertyId).Distinct().Count() != inputs.Count || inputs.Any(v => !propertiesById.ContainsKey(v.DefinitionPropertyId)))
            throw new BusinessException("资源属性包含重复或未知字段", StatusCodes.Status400BadRequest);
        ResDefinitionProperty? missing = properties.FirstOrDefault(p => p.IsRequired && inputs.All(v => v.DefinitionPropertyId != p.Id || string.IsNullOrWhiteSpace(v.Value)));
        if (missing != null) throw new BusinessException($"属性 {missing.Name} 为必填项", StatusCodes.Status400BadRequest);
        List<ResourceValueDto> normalized = [];
        foreach (ResourceValueDto input in inputs)
        {
            ResDefinitionProperty property = propertiesById[input.DefinitionPropertyId];
            if (string.IsNullOrWhiteSpace(input.Value)) continue;
            if (input.Value.Length > Math.Min(property.MaxLength, 1000)) throw new BusinessException($"属性 {property.Name} 超过最大长度", StatusCodes.Status400BadRequest);
            normalized.Add(new ResourceValueDto { DefinitionPropertyId = property.Id, Value = NormalizeValue(input.Value, property.ValueType, property.Name) });
        }
        return normalized;
    }

    private static string NormalizeValue(string value, ResValueType type, string propertyName)
    {
        string trimmed = value.Trim();
        return type switch
        {
            ResValueType.String => value,
            ResValueType.Number when NumberRegex.IsMatch(trimmed) && decimal.TryParse(trimmed, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal number) => number.ToString(CultureInfo.InvariantCulture),
            ResValueType.Boolean when bool.TryParse(trimmed, out bool boolean) => boolean.ToString().ToLowerInvariant(),
            ResValueType.Date when DateOnly.TryParseExact(trimmed, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ResValueType.Uri when Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri) => uri.AbsoluteUri,
            ResValueType.IPAddress when IPAddress.TryParse(trimmed, out IPAddress? address) => address.ToString(),
            _ => throw new BusinessException($"属性 {propertyName} 的值格式无效", StatusCodes.Status400BadRequest)
        };
    }

    private void EnsureAdmin()
    {
        if (!_userContext.IsAdmin) throw new BusinessException("无管理资源权限", StatusCodes.Status403Forbidden);
    }
}
