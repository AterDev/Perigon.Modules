using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using ResourceMod.Models;
using Share.Exceptions;
using System.Text.RegularExpressions;

namespace ResourceMod.Managers;

public class ResourceConfigurationManager(
    AppDbFactory dbContextFactory,
    ILogger<ResourceConfigurationManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ResEnvironment>(dbContextFactory, userContext, logger)
{
    private static readonly Regex ResourceNameRegex = new(
        "^[\\p{L}\\p{N}_-]+(?: [\\p{L}\\p{N}_-]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<List<ResEnvironment>> EnvironmentsAsync()
    {
        return await _dbContext.ResEnvironments
            .Where(e => e.TenantId == _userContext.TenantId)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<List<ResCategory>> CategoriesAsync()
    {
        return await _dbContext.ResCategories
            .Where(c => c.TenantId == _userContext.TenantId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<ResGroup>> GroupsAsync(Guid? categoryId)
    {
        IQueryable<ResGroup> query = _dbContext.ResGroups
            .Where(g => g.TenantId == _userContext.TenantId);
        if (categoryId.HasValue)
        {
            query = query.Where(g => g.CategoryId == categoryId.Value);
        }

        return await query
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<List<ResTag>> TagsAsync()
    {
        return await _dbContext.ResTags
            .Where(t => t.TenantId == _userContext.TenantId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<ResDefinition>> DefinitionsAsync(string? name = null)
    {
        IQueryable<ResDefinition> query = _dbContext.ResDefinitions
            .Where(d => d.TenantId == _userContext.TenantId)
            .Include(d => d.PropertyMaps)
            .ThenInclude(map => map.Property)
            .AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(d => d.Name.Contains(name.Trim()));
        }

        List<ResDefinition> definitions = await query.OrderBy(d => d.Name).ToListAsync();
        foreach (ResDefinition definition in definitions)
        {
            PopulateProperties(definition);
        }

        return definitions;
    }

    public async Task<List<ResDefinitionProperty>> PropertiesAsync(string? name = null)
    {
        IQueryable<ResDefinitionProperty> query = _dbContext.ResDefinitionProperties
            .Where(p => p.TenantId == _userContext.TenantId)
            .OrderBy(p => p.Name);
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(p => p.Name.Contains(name.Trim()));
        }

        return await query.ToListAsync();
    }

    public async Task<ResDefinitionProperty> AddPropertyAsync(ResDefinitionPropertyAddDto input)
    {
        EnsureAdmin();
        ValidateProperty(input.Name, input.MaxLength);
        string name = input.Name.Trim();
        await EnsurePropertyNameAvailableAsync(name, null);

        ResDefinitionProperty entity = new()
        {
            Name = name,
            NameKey = NormalizeName(name),
            ValueType = input.ValueType,
            IsRequired = input.IsRequired,
            MaxLength = input.MaxLength,
            TenantId = _userContext.TenantId
        };

        await _dbContext.ResDefinitionProperties.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<ResDefinitionProperty> UpdatePropertyAsync(
        Guid id,
        ResDefinitionPropertyUpdateDto input)
    {
        EnsureAdmin();
        ValidateProperty(input.Name, input.MaxLength);

        ResDefinitionProperty entity = await GetTenantEntityAsync(
            _dbContext.ResDefinitionProperties,
            id,
            "资源属性不存在");
        string name = input.Name.Trim();
        await EnsurePropertyNameAvailableAsync(name, id);
        entity.Name = name;
        entity.NameKey = NormalizeName(name);
        entity.ValueType = input.ValueType;
        entity.IsRequired = input.IsRequired;
        entity.MaxLength = input.MaxLength;

        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeletePropertyAsync(Guid id)
    {
        EnsureAdmin();

        ResDefinitionProperty entity = await GetTenantEntityAsync(
            _dbContext.ResDefinitionProperties,
            id,
            "资源属性不存在");
        bool isReferenced = await _dbContext.ResDefinitionPropertyMaps.AnyAsync(map =>
            map.TenantId == _userContext.TenantId && map.PropertyId == id);
        bool hasValues = await _dbContext.ResValues.AnyAsync(value =>
            value.TenantId == _userContext.TenantId && value.DefinitionPropertyId == id);
        if (isReferenced || hasValues)
        {
            throw new BusinessException("资源属性正在被使用，不能删除", StatusCodes.Status409Conflict);
        }

        _dbContext.ResDefinitionProperties.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ResEnvironment> AddEnvironmentAsync(ResEnvironmentAddDto input)
    {
        EnsureAdmin();

        ResEnvironment entity = new()
        {
            Name = input.Name,
            Icon = input.Icon,
            Color = input.Color,
            TenantId = _userContext.TenantId
        };

        await _dbContext.ResEnvironments.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task<ResEnvironment> UpdateEnvironmentAsync(Guid id, ResEnvironmentUpdateDto input)
    {
        EnsureAdmin();

        ResEnvironment entity = await GetTenantEntityAsync(_dbContext.ResEnvironments, id, "环境不存在");
        entity.Name = input.Name;
        entity.Icon = input.Icon;
        entity.Color = input.Color;

        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task DeleteEnvironmentAsync(Guid id)
    {
        EnsureAdmin();

        ResEnvironment entity = await GetTenantEntityAsync(_dbContext.ResEnvironments, id, "环境不存在");
        bool isReferenced = await _dbContext.Resources.AnyAsync(r =>
            r.TenantId == _userContext.TenantId && r.EnvironmentId == id);
        bool hasPermissions = await _dbContext.ResPermissions.AnyAsync(p =>
            p.TenantId == _userContext.TenantId && p.EnvironmentId == id);
        if (isReferenced || hasPermissions)
        {
            throw new BusinessException("环境已被资源引用，不能删除", StatusCodes.Status409Conflict);
        }

        _dbContext.ResEnvironments.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ResCategory> AddCategoryAsync(ResCategoryAddDto input)
    {
        EnsureAdmin();

        bool codeExists = await _dbContext.ResCategories.AnyAsync(c =>
            c.TenantId == _userContext.TenantId && c.CatalogCode == input.CatalogCode);
        if (codeExists)
        {
            throw new BusinessException("分类编码已存在", StatusCodes.Status409Conflict);
        }

        ResCategory entity = new()
        {
            Name = input.Name,
            CatalogCode = input.CatalogCode,
            Icon = input.Icon,
            Color = input.Color,
            TenantId = _userContext.TenantId
        };

        await _dbContext.ResCategories.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task<ResCategory> UpdateCategoryAsync(Guid id, ResCategoryUpdateDto input)
    {
        EnsureAdmin();

        ResCategory entity = await GetTenantEntityAsync(_dbContext.ResCategories, id, "分类不存在");
        bool codeExists = await _dbContext.ResCategories.AnyAsync(c =>
            c.TenantId == _userContext.TenantId &&
            c.Id != id &&
            c.CatalogCode == input.CatalogCode);
        if (codeExists)
        {
            throw new BusinessException("分类编码已存在", StatusCodes.Status409Conflict);
        }

        entity.Name = input.Name;
        entity.CatalogCode = input.CatalogCode;
        entity.Icon = input.Icon;
        entity.Color = input.Color;

        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        EnsureAdmin();

        ResCategory entity = await GetTenantEntityAsync(_dbContext.ResCategories, id, "分类不存在");
        bool isReferenced = await _dbContext.Resources.AnyAsync(r =>
            r.TenantId == _userContext.TenantId && r.CategoryId == id);
        bool hasGroups = await _dbContext.ResGroups.AnyAsync(g =>
            g.TenantId == _userContext.TenantId && g.CategoryId == id);
        bool hasPermissions = await _dbContext.ResPermissions.AnyAsync(p =>
            p.TenantId == _userContext.TenantId && p.CategoryId == id);
        if (isReferenced || hasGroups || hasPermissions)
        {
            throw new BusinessException("分类已被资源引用，不能删除", StatusCodes.Status409Conflict);
        }

        _dbContext.ResCategories.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ResGroup> AddGroupAsync(ResGroupAddDto input)
    {
        EnsureAdmin();
        await EnsureCategoryAsync(input.CategoryId);

        ResGroup entity = new()
        {
            Name = input.Name,
            Description = input.Description,
            Icon = input.Icon,
            Color = input.Color,
            CategoryId = input.CategoryId,
            TenantId = _userContext.TenantId
        };

        await _dbContext.ResGroups.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task<ResGroup> UpdateGroupAsync(Guid id, ResGroupUpdateDto input)
    {
        EnsureAdmin();
        await EnsureCategoryAsync(input.CategoryId);

        ResGroup entity = await GetTenantEntityAsync(_dbContext.ResGroups, id, "分组不存在");
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Icon = input.Icon;
        entity.Color = input.Color;
        entity.CategoryId = input.CategoryId;

        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task DeleteGroupAsync(Guid id)
    {
        EnsureAdmin();

        ResGroup entity = await GetTenantEntityAsync(_dbContext.ResGroups, id, "分组不存在");
        bool isReferenced = await _dbContext.Resources.AnyAsync(r =>
            r.TenantId == _userContext.TenantId && r.GroupId == id);
        if (isReferenced)
        {
            throw new BusinessException("分组已被资源引用，不能删除", StatusCodes.Status409Conflict);
        }

        _dbContext.ResGroups.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ResTag> AddTagAsync(ResTagAddDto input)
    {
        EnsureAdmin();

        ResTag entity = new()
        {
            Name = input.Name,
            Color = input.Color,
            Icon = input.Icon,
            TenantId = _userContext.TenantId
        };

        await _dbContext.ResTags.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task<ResTag> UpdateTagAsync(Guid id, ResTagUpdateDto input)
    {
        EnsureAdmin();

        ResTag entity = await GetTenantEntityAsync(_dbContext.ResTags, id, "标签不存在");
        entity.Name = input.Name;
        entity.Color = input.Color;
        entity.Icon = input.Icon;

        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task DeleteTagAsync(Guid id)
    {
        EnsureAdmin();

        ResTag entity = await GetTenantEntityAsync(_dbContext.ResTags, id, "标签不存在");
        _dbContext.ResTags.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ResDefinition> AddDefinitionAsync(ResDefinitionAddDto input)
    {
        EnsureAdmin();
        ValidateResourceName(input.Name);
        List<DefinitionPropertySelection> selections =
            await ResolveDefinitionPropertiesAsync(input.Properties, null);

        ResDefinition entity = new()
        {
            Name = input.Name.Trim(),
            Icon = input.Icon,
            TenantId = _userContext.TenantId
        };

        foreach (DefinitionPropertySelection selection in selections)
        {
            entity.PropertyMaps.Add(new ResDefinitionPropertyMap
            {
                PropertyId = selection.Property.Id,
                Sort = selection.Sort,
                TenantId = _userContext.TenantId
            });
        }

        await _dbContext.ResDefinitions.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return await GetDefinitionAsync(entity.Id);
    }

    public async Task<ResDefinition> UpdateDefinitionAsync(Guid id, ResDefinitionUpdateDto input)
    {
        EnsureAdmin();
        ValidateResourceName(input.Name);
        ResDefinition entity = await _dbContext.ResDefinitions
            .Include(d => d.PropertyMaps)
            .ThenInclude(map => map.Property)
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == _userContext.TenantId)
            ?? throw new BusinessException("资源定义不存在", StatusCodes.Status404NotFound);
        List<DefinitionPropertySelection> selections =
            await ResolveDefinitionPropertiesAsync(input.Properties, entity);

        entity.Name = input.Name.Trim();
        entity.Icon = input.Icon;

        HashSet<Guid> retainedIds = selections
            .Select(selection => selection.Property.Id)
            .ToHashSet();
        List<ResDefinitionPropertyMap> removed = entity.PropertyMaps
            .Where(map => !retainedIds.Contains(map.PropertyId))
            .ToList();
        bool hasRemovedValues = removed.Count != 0 && await _dbContext.ResValues.AnyAsync(value =>
            value.TenantId == _userContext.TenantId &&
            removed.Select(map => map.PropertyId).Contains(value.DefinitionPropertyId));
        if (hasRemovedValues)
        {
            throw new BusinessException("定义属性已被资源值引用，不能移除", StatusCodes.Status409Conflict);
        }

        _dbContext.ResDefinitionPropertyMaps.RemoveRange(removed);

        foreach (DefinitionPropertySelection selection in selections)
        {
            ResDefinitionPropertyMap? target = entity.PropertyMaps.FirstOrDefault(map =>
                map.PropertyId == selection.Property.Id);
            if (target is null)
            {
                _dbContext.ResDefinitionPropertyMaps.Add(new ResDefinitionPropertyMap
                {
                    DefinitionId = entity.Id,
                    PropertyId = selection.Property.Id,
                    Sort = selection.Sort,
                    TenantId = _userContext.TenantId
                });
            }
            else
            {
                target.Sort = selection.Sort;
            }
        }

        await _dbContext.SaveChangesAsync();

        return await GetDefinitionAsync(entity.Id);
    }

    public async Task DeleteDefinitionAsync(Guid id)
    {
        EnsureAdmin();

        ResDefinition entity = await GetTenantEntityAsync(_dbContext.ResDefinitions, id, "资源定义不存在");
        bool isReferenced = await _dbContext.Resources.AnyAsync(r =>
            r.TenantId == _userContext.TenantId && r.DefinitionId == id);
        if (isReferenced)
        {
            throw new BusinessException("资源定义已被资源引用，不能删除", StatusCodes.Status409Conflict);
        }

        _dbContext.ResDefinitions.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    private async Task<ResDefinition> GetDefinitionAsync(Guid id)
    {
        ResDefinition entity = await _dbContext.ResDefinitions
            .Include(d => d.PropertyMaps)
            .ThenInclude(map => map.Property)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == _userContext.TenantId)
            ?? throw new BusinessException("资源定义不存在", StatusCodes.Status404NotFound);
        PopulateProperties(entity);
        return entity;
    }

    private async Task<List<DefinitionPropertySelection>> ResolveDefinitionPropertiesAsync(
        List<ResDefinitionPropertyDto> inputs,
        ResDefinition? definition)
    {
        HashSet<Guid> requestedIds = inputs
            .Where(input => input.Id.HasValue)
            .Select(input => input.Id!.Value)
            .ToHashSet();
        if (requestedIds.Count != inputs.Count(input => input.Id.HasValue))
        {
            throw new BusinessException("资源定义属性不能重复", StatusCodes.Status400BadRequest);
        }

        List<ResDefinitionProperty> referencedProperties = await _dbContext.ResDefinitionProperties
            .Where(property =>
                property.TenantId == _userContext.TenantId && requestedIds.Contains(property.Id))
            .ToListAsync();
        Dictionary<Guid, ResDefinitionProperty> propertiesById = referencedProperties
            .ToDictionary(property => property.Id);

        HashSet<string> requestedNames = inputs
            .Where(input => !input.Id.HasValue)
            .Select(input => NormalizeName(input.Name))
            .Where(name => name.Length != 0)
            .ToHashSet(StringComparer.Ordinal);
        List<ResDefinitionProperty> namedProperties = await _dbContext.ResDefinitionProperties
            .Where(property =>
                property.TenantId == _userContext.TenantId &&
                requestedNames.Contains(property.NameKey))
            .ToListAsync();
        Dictionary<string, ResDefinitionProperty> propertiesByName = namedProperties
            .ToDictionary(property => property.NameKey, StringComparer.Ordinal);
        HashSet<Guid> currentPropertyIds = definition?.PropertyMaps
            .Select(map => map.PropertyId)
            .ToHashSet() ?? [];

        List<DefinitionPropertySelection> selections = [];
        foreach (ResDefinitionPropertyDto input in inputs)
        {
            ValidateProperty(input.Name, input.MaxLength);
            ResDefinitionProperty property;
            if (input.Id.HasValue)
            {
                if (!propertiesById.TryGetValue(input.Id.Value, out property!))
                {
                    throw new BusinessException("资源属性不存在或不属于当前租户", StatusCodes.Status400BadRequest);
                }

                if (currentPropertyIds.Contains(property.Id) && HasPropertyChanges(property, input))
                {
                    await EnsurePropertyNameAvailableAsync(input.Name.Trim(), property.Id);
                    bool usedByAnotherDefinition = await _dbContext.ResDefinitionPropertyMaps.AnyAsync(map =>
                        map.TenantId == _userContext.TenantId &&
                        map.PropertyId == property.Id &&
                        map.DefinitionId != definition!.Id);
                    if (usedByAnotherDefinition)
                    {
                        throw new BusinessException(
                            "已复用的资源属性不能在资源定义中修改",
                            StatusCodes.Status409Conflict);
                    }

                    property.Name = input.Name.Trim();
                    property.NameKey = NormalizeName(input.Name);
                    property.ValueType = input.ValueType;
                    property.IsRequired = input.IsRequired;
                    property.MaxLength = input.MaxLength;
                }
            }
            else if (propertiesByName.TryGetValue(NormalizeName(input.Name), out ResDefinitionProperty? existing))
            {
                if (!MatchesProperty(existing, input))
                {
                    throw new BusinessException(
                        "同名资源属性已存在，请选择已有属性或先修改属性定义",
                        StatusCodes.Status409Conflict);
                }

                property = existing;
            }
            else
            {
                property = new ResDefinitionProperty
                {
                    Name = input.Name.Trim(),
                    NameKey = NormalizeName(input.Name),
                    ValueType = input.ValueType,
                    IsRequired = input.IsRequired,
                    MaxLength = input.MaxLength,
                    TenantId = _userContext.TenantId
                };
                _dbContext.ResDefinitionProperties.Add(property);
                propertiesByName[property.NameKey] = property;
            }

            selections.Add(new DefinitionPropertySelection(property, input.Sort));
        }

        bool hasDuplicateName = selections
            .GroupBy(selection => selection.Property.Name, StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);
        if (hasDuplicateName)
        {
            throw new BusinessException("资源定义属性不能重复", StatusCodes.Status400BadRequest);
        }

        return selections;
    }

    private async Task EnsurePropertyNameAvailableAsync(string name, Guid? exceptId)
    {
        bool exists = await _dbContext.ResDefinitionProperties.AnyAsync(property =>
            property.TenantId == _userContext.TenantId &&
            (!exceptId.HasValue || property.Id != exceptId.Value) &&
            property.NameKey == NormalizeName(name));
        if (exists)
        {
            throw new BusinessException("资源属性名称已存在", StatusCodes.Status409Conflict);
        }
    }

    private static void PopulateProperties(ResDefinition definition)
    {
        definition.Properties = definition.PropertyMaps
            .OrderBy(map => map.Sort)
            .Select(map =>
            {
                map.Property.Sort = map.Sort;
                return map.Property;
            })
            .ToList();
    }

    private static bool HasPropertyChanges(ResDefinitionProperty property, ResDefinitionPropertyDto input)
    {
        return !string.Equals(property.Name, input.Name.Trim(), StringComparison.Ordinal) ||
            property.ValueType != input.ValueType ||
            property.IsRequired != input.IsRequired ||
            property.MaxLength != input.MaxLength;
    }

    private static bool MatchesProperty(ResDefinitionProperty property, ResDefinitionPropertyDto input)
    {
        return string.Equals(property.Name, input.Name.Trim(), StringComparison.OrdinalIgnoreCase) &&
            property.ValueType == input.ValueType &&
            property.IsRequired == input.IsRequired &&
            property.MaxLength == input.MaxLength;
    }

    private static void ValidateProperty(string name, int maxLength)
    {
        ValidateResourceName(name);
        if (maxLength is < 1 or > 1000)
        {
            throw new BusinessException("资源属性无效", StatusCodes.Status400BadRequest);
        }
    }

    private static void ValidateResourceName(string? name)
    {
        string value = name?.Trim() ?? string.Empty;
        if (value.Length == 0 || value.Length > 60 || !ResourceNameRegex.IsMatch(value))
        {
            throw new BusinessException("资源名称只能包含中文、字母、数字、空格、下划线或连字符", StatusCodes.Status400BadRequest);
        }
    }

    private static string NormalizeName(string? name)
    {
        return (name?.Trim() ?? string.Empty).ToLowerInvariant();
    }

    private sealed record DefinitionPropertySelection(ResDefinitionProperty Property, int Sort);

    public async Task<List<ResPermission>> GetPermissionsAsync(Guid environmentId, Guid categoryId)
    {
        return await _dbContext.ResPermissions
            .Where(p =>
                p.TenantId == _userContext.TenantId &&
                p.EnvironmentId == environmentId &&
                p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task SetPermissionsAsync(ResPermissionUpdateDto input)
    {
        EnsureAdmin();

        bool environmentExists = await _dbContext.ResEnvironments.AnyAsync(e =>
            e.Id == input.EnvironmentId && e.TenantId == _userContext.TenantId);
        bool categoryExists = await _dbContext.ResCategories.AnyAsync(c =>
            c.Id == input.CategoryId && c.TenantId == _userContext.TenantId);
        if (!environmentExists || !categoryExists)
        {
            throw new BusinessException("环境或分类不存在", StatusCodes.Status400BadRequest);
        }

        List<Guid> roleIds = input.RoleIds.Distinct().ToList();
        int matchingRoleCount = await _dbContext.SystemRoles.CountAsync(r =>
            r.TenantId == _userContext.TenantId && roleIds.Contains(r.Id));
        if (roleIds.Count != matchingRoleCount)
        {
            throw new BusinessException("角色不存在", StatusCodes.Status400BadRequest);
        }

        List<ResPermission> existing = await GetPermissionsAsync(input.EnvironmentId, input.CategoryId);
        _dbContext.ResPermissions.RemoveRange(existing);
        await _dbContext.ResPermissions.AddRangeAsync(roleIds.Select(roleId => new ResPermission
        {
            RoleId = roleId,
            EnvironmentId = input.EnvironmentId,
            CategoryId = input.CategoryId,
            TenantId = _userContext.TenantId
        }));
        await _dbContext.SaveChangesAsync();
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        return Task.FromResult(_userContext.IsAdmin);
    }

    private async Task EnsureCategoryAsync(Guid id)
    {
        bool categoryExists = await _dbContext.ResCategories.AnyAsync(c =>
            c.Id == id && c.TenantId == _userContext.TenantId);
        if (!categoryExists)
        {
            throw new BusinessException("分类不存在", StatusCodes.Status400BadRequest);
        }
    }

    private void EnsureAdmin()
    {
        if (!_userContext.IsAdmin)
        {
            throw new BusinessException("无管理资源权限", StatusCodes.Status403Forbidden);
        }
    }

    private async Task<T> GetTenantEntityAsync<T>(DbSet<T> set, Guid id, string message)
        where T : EntityBase
    {
        return await set.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == _userContext.TenantId)
            ?? throw new BusinessException(message, StatusCodes.Status404NotFound);
    }
}
