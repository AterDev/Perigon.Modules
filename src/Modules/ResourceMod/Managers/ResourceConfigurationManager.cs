using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using Perigon.PostgreSQL;
using ResourceMod.Models;
using Share.Exceptions;
using System.Diagnostics.CodeAnalysis;
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
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<List<ResCategory>> CategoriesAsync()
    {
        return await _dbContext.ResCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<ResGroup>> GroupsAsync(Guid? categoryId)
    {
        IQueryable<ResGroup> query = _dbContext.ResGroups;
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
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<ResDefinitionItemDto>> DefinitionsAsync(string? name = null)
    {
        IQueryable<ResDefinition> query = _dbContext.ResDefinitions;
        if (!string.IsNullOrWhiteSpace(name))
        {
            string searchName = name.Trim();
            query = query.Where(d => d.Name.Contains(searchName));
        }

        List<ResDefinition> definitions = await query.OrderBy(d => d.Name).ToListAsync();
        foreach (ResDefinition definition in definitions)
        {
            definition.PropertyMaps = await _dbContext.ResDefinitionPropertyMaps
                .Where(map => map.DefinitionId == definition.Id).OrderBy(map => map.Sort).ToListAsync();
            Guid[] propertyIds = definition.PropertyMaps.Select(map => map.PropertyId).ToArray();
            Dictionary<Guid, ResDefinitionProperty> properties = (await _dbContext.ResDefinitionProperties
                .Where(property => propertyIds.Contains(property.Id)).ToListAsync()).ToDictionary(property => property.Id);
            foreach (ResDefinitionPropertyMap map in definition.PropertyMaps)
            {
                if (properties.TryGetValue(map.PropertyId, out ResDefinitionProperty? property))
                {
                    map.Property = property;
                }
            }
        }

        return definitions.Select(ToDefinitionItemDto).ToList();
    }

    public async Task<List<ResDefinitionPropertyItemDto>> PropertiesAsync(string? name = null)
    {
        IQueryable<ResDefinitionProperty> query = _dbContext.ResDefinitionProperties
            .OrderBy(p => p.Name);
        if (!string.IsNullOrWhiteSpace(name))
        {
            string searchName = name.Trim();
            query = query.Where(p => p.Name.Contains(searchName));
        }

        List<ResDefinitionProperty> properties = await query.ToListAsync();
        return properties.Select(property => ResDefinitionPropertyItemDto.FromEntity(property)).ToList();
    }

    public async Task<ResDefinitionPropertyItemDto> AddPropertyAsync(ResDefinitionPropertyAddDto input)
    {
        EnsureAdmin();
        ValidateProperty(input.Name, input.MaxLength);
        string name = input.Name.Trim();
        await EnsurePropertyNameAvailableAsync(name, null);

        ResDefinitionProperty entity = new()
        {
            Name = name,
            ValueType = input.ValueType,
            IsRequired = input.IsRequired,
            MaxLength = input.MaxLength,
        };

        await _dbContext.ResDefinitionProperties.InsertAsync(entity);
        return ResDefinitionPropertyItemDto.FromEntity(entity);
    }

    public async Task<ResDefinitionPropertyItemDto> UpdatePropertyAsync(
        Guid id,
        ResDefinitionPropertyUpdateDto input)
    {
        EnsureAdmin();
        ValidateProperty(input.Name, input.MaxLength);

        ResDefinitionProperty entity = await GetEntityAsync(
            _dbContext.ResDefinitionProperties,
            id,
            "资源属性不存在");
        string name = input.Name.Trim();
        ResValueType valueType = input.ValueType;
        await EnsurePropertyNameAvailableAsync(name, id);
        await _dbContext.ResDefinitionProperties.Where(item => item.Id == id).ExecuteUpdateAsync(s => s
            .Set(item => item.Name, name).Set(item => item.ValueType, valueType)
            .Set(item => item.IsRequired, input.IsRequired).Set(item => item.MaxLength, input.MaxLength));
        entity.Name = name;
        entity.ValueType = input.ValueType;
        entity.IsRequired = input.IsRequired;
        entity.MaxLength = input.MaxLength;
        return ResDefinitionPropertyItemDto.FromEntity(entity);
    }

    public async Task DeletePropertyAsync(Guid id)
    {
        EnsureAdmin();

        ResDefinitionProperty entity = await GetEntityAsync(
            _dbContext.ResDefinitionProperties,
            id,
            "资源属性不存在");
        bool isReferenced = await _dbContext.ResDefinitionPropertyMaps.AnyAsync(map =>
            map.PropertyId == id);
        bool hasValues = await _dbContext.ResValues.AnyAsync(value =>
            value.DefinitionPropertyId == id);
        bool hasUserValues = await _dbContext.UserResValues.AnyAsync(value =>
            value.DefinitionPropertyId == id);
        if (isReferenced || hasValues || hasUserValues)
        {
            throw new BusinessException("资源属性正在被使用，不能删除", StatusCodes.Status409Conflict);
        }

        await _dbContext.ResDefinitionProperties.Where(item => item.Id == id).ExecuteDeleteAsync();
    }

    public async Task<ResEnvironment> AddEnvironmentAsync(ResEnvironmentAddDto input)
    {
        EnsureAdmin();

        ResEnvironment entity = new()
        {
            Name = input.Name,
            Icon = input.Icon,
            Color = input.Color,
        };

        await _dbContext.ResEnvironments.InsertAsync(entity);

        return entity;
    }

    public async Task<ResEnvironment> UpdateEnvironmentAsync(Guid id, ResEnvironmentUpdateDto input)
    {
        EnsureAdmin();

        ResEnvironment entity = await GetEntityAsync(_dbContext.ResEnvironments, id, "环境不存在");
        await _dbContext.ResEnvironments.Where(item => item.Id == id).ExecuteUpdateAsync(s => s
            .Set(item => item.Name, input.Name).Set(item => item.Icon, input.Icon).Set(item => item.Color, input.Color));
        entity.Name = input.Name;
        entity.Icon = input.Icon;
        entity.Color = input.Color;

        return entity;
    }

    public async Task DeleteEnvironmentAsync(Guid id)
    {
        EnsureAdmin();

        ResEnvironment entity = await GetEntityAsync(_dbContext.ResEnvironments, id, "环境不存在");
        bool isReferenced = await _dbContext.Resources.AnyAsync(r =>
            r.EnvironmentId == id);
        bool hasPermissions = await _dbContext.ResPermissions.AnyAsync(p =>
            p.EnvironmentId == id);
        if (isReferenced || hasPermissions)
        {
            throw new BusinessException("环境已被资源引用，不能删除", StatusCodes.Status409Conflict);
        }

        await _dbContext.ResEnvironments.Where(item => item.Id == id).ExecuteDeleteAsync();
    }

    public async Task<ResCategory> AddCategoryAsync(ResCategoryAddDto input)
    {
        EnsureAdmin();

        bool codeExists = await _dbContext.ResCategories.AnyAsync(c =>
            c.CatalogCode == input.CatalogCode);
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
        };

        await _dbContext.ResCategories.InsertAsync(entity);

        return entity;
    }

    public async Task<ResCategory> UpdateCategoryAsync(Guid id, ResCategoryUpdateDto input)
    {
        EnsureAdmin();

        ResCategory entity = await GetEntityAsync(_dbContext.ResCategories, id, "分类不存在");
        bool codeExists = await _dbContext.ResCategories.AnyAsync(c =>
            c.Id != id &&
            c.CatalogCode == input.CatalogCode);
        if (codeExists)
        {
            throw new BusinessException("分类编码已存在", StatusCodes.Status409Conflict);
        }

        await _dbContext.ResCategories.Where(item => item.Id == id).ExecuteUpdateAsync(s => s
            .Set(item => item.Name, input.Name).Set(item => item.CatalogCode, input.CatalogCode)
            .Set(item => item.Icon, input.Icon).Set(item => item.Color, input.Color));
        entity.Name = input.Name;
        entity.CatalogCode = input.CatalogCode;
        entity.Icon = input.Icon;
        entity.Color = input.Color;

        return entity;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        EnsureAdmin();

        ResCategory entity = await GetEntityAsync(_dbContext.ResCategories, id, "分类不存在");
        bool isReferenced = await _dbContext.Resources.AnyAsync(r =>
            r.CategoryId == id);
        bool hasGroups = await _dbContext.ResGroups.AnyAsync(g =>
            g.CategoryId == id);
        bool hasPermissions = await _dbContext.ResPermissions.AnyAsync(p =>
            p.CategoryId == id);
        if (isReferenced || hasGroups || hasPermissions)
        {
            throw new BusinessException("分类已被资源引用，不能删除", StatusCodes.Status409Conflict);
        }

        await _dbContext.ResCategories.Where(item => item.Id == id).ExecuteDeleteAsync();
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
        };

        await _dbContext.ResGroups.InsertAsync(entity);

        return entity;
    }

    public async Task<ResGroup> UpdateGroupAsync(Guid id, ResGroupUpdateDto input)
    {
        EnsureAdmin();
        await EnsureCategoryAsync(input.CategoryId);

        ResGroup entity = await GetEntityAsync(_dbContext.ResGroups, id, "分组不存在");
        await _dbContext.ResGroups.Where(item => item.Id == id).ExecuteUpdateAsync(s => s
            .Set(item => item.Name, input.Name).Set(item => item.Description, input.Description)
            .Set(item => item.Icon, input.Icon).Set(item => item.Color, input.Color)
            .Set(item => item.CategoryId, input.CategoryId));
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Icon = input.Icon;
        entity.Color = input.Color;
        entity.CategoryId = input.CategoryId;

        return entity;
    }

    public async Task DeleteGroupAsync(Guid id)
    {
        EnsureAdmin();

        ResGroup entity = await GetEntityAsync(_dbContext.ResGroups, id, "分组不存在");
        bool isReferenced = await _dbContext.Resources.AnyAsync(r =>
            r.GroupId == id);
        if (isReferenced)
        {
            throw new BusinessException("分组已被资源引用，不能删除", StatusCodes.Status409Conflict);
        }

        await _dbContext.ResGroups.Where(item => item.Id == id).ExecuteDeleteAsync();
    }

    public async Task<ResTag> AddTagAsync(ResTagAddDto input)
    {
        EnsureAdmin();

        ResTag entity = new()
        {
            Name = input.Name,
            Color = input.Color,
            Icon = input.Icon,
        };

        await _dbContext.ResTags.InsertAsync(entity);

        return entity;
    }

    public async Task<ResTag> UpdateTagAsync(Guid id, ResTagUpdateDto input)
    {
        EnsureAdmin();

        ResTag entity = await GetEntityAsync(_dbContext.ResTags, id, "标签不存在");
        await _dbContext.ResTags.Where(item => item.Id == id).ExecuteUpdateAsync(s => s
            .Set(item => item.Name, input.Name).Set(item => item.Color, input.Color).Set(item => item.Icon, input.Icon));
        entity.Name = input.Name;
        entity.Color = input.Color;
        entity.Icon = input.Icon;

        return entity;
    }

    public async Task DeleteTagAsync(Guid id)
    {
        EnsureAdmin();

        ResTag entity = await GetEntityAsync(_dbContext.ResTags, id, "标签不存在");
        await _dbContext.ResTags.Where(item => item.Id == id).ExecuteDeleteAsync();
    }

    public Task<ResDefinitionItemDto> AddDefinitionAsync(ResDefinitionAddDto input)
        => _dbContext.TransactionAsync(_ => AddDefinitionCoreAsync(input));

    private async Task<ResDefinitionItemDto> AddDefinitionCoreAsync(ResDefinitionAddDto input)
    {
        EnsureAdmin();
        ValidateResourceName(input.Name);
        List<DefinitionPropertySelection> selections =
            await ResolveDefinitionPropertiesAsync(input.Properties, null);

        ResDefinition entity = new()
        {
            Name = input.Name.Trim(),
            Icon = input.Icon,
        };

        foreach (DefinitionPropertySelection selection in selections)
        {
            entity.PropertyMaps.Add(new ResDefinitionPropertyMap
            {
                DefinitionId = entity.Id,
                PropertyId = selection.Property.Id,
                Sort = selection.Sort,
            });
        }

        await _dbContext.ResDefinitions.InsertAsync(entity);
        if (entity.PropertyMaps.Count != 0)
        {
            await _dbContext.ResDefinitionPropertyMaps.BulkInsertAsync(entity.PropertyMaps);
        }

        return ToDefinitionItemDto(await GetDefinitionAsync(entity.Id));
    }

    public Task<ResDefinitionItemDto> UpdateDefinitionAsync(Guid id, ResDefinitionUpdateDto input)
        => _dbContext.TransactionAsync(_ => UpdateDefinitionCoreAsync(id, input));

    private async Task<ResDefinitionItemDto> UpdateDefinitionCoreAsync(Guid id, ResDefinitionUpdateDto input)
    {
        EnsureAdmin();
        ValidateResourceName(input.Name);
        ResDefinition entity = await _dbContext.ResDefinitions
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new BusinessException("资源定义不存在", StatusCodes.Status404NotFound);
        entity.PropertyMaps = await _dbContext.ResDefinitionPropertyMaps
            .Where(map => map.DefinitionId == id).OrderBy(map => map.Sort).ToListAsync();
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
        Guid[] removedPropertyIds = removed.Select(map => map.PropertyId).ToArray();
        bool hasRemovedValues = removed.Count != 0 && await _dbContext.ResValues.AnyAsync(value =>
            removedPropertyIds.Contains(value.DefinitionPropertyId));
        bool hasRemovedUserValues = removed.Count != 0 && await _dbContext.UserResValues.AnyAsync(value =>
            removedPropertyIds.Contains(value.DefinitionPropertyId));
        if (hasRemovedValues || hasRemovedUserValues)
        {
            throw new BusinessException("定义属性已被资源值引用，不能移除", StatusCodes.Status409Conflict);
        }

        if (removed.Count != 0)
        {
            await _dbContext.ResDefinitionPropertyMaps
                .Where(map => map.DefinitionId == entity.Id && removedPropertyIds.Contains(map.PropertyId))
                .ExecuteDeleteAsync();
        }

        List<ResDefinitionPropertyMap> addedMaps = [];
        foreach (DefinitionPropertySelection selection in selections)
        {
            ResDefinitionPropertyMap? target = entity.PropertyMaps.FirstOrDefault(map =>
                map.PropertyId == selection.Property.Id);
            if (target is null)
            {
                addedMaps.Add(new ResDefinitionPropertyMap
                {
                    DefinitionId = entity.Id,
                    PropertyId = selection.Property.Id,
                    Sort = selection.Sort,
                });
            }
            else
            {
                await _dbContext.ResDefinitionPropertyMaps.Where(map => map.Id == target.Id)
                    .ExecuteUpdateAsync(s => s.Set(map => map.Sort, selection.Sort));
                target.Sort = selection.Sort;
            }
        }

        if (addedMaps.Count != 0)
        {
            await _dbContext.ResDefinitionPropertyMaps.BulkInsertAsync(addedMaps);
        }

        string name = input.Name.Trim();
        await _dbContext.ResDefinitions.Where(item => item.Id == id).ExecuteUpdateAsync(s => s
            .Set(item => item.Name, name).Set(item => item.Icon, input.Icon));

        return ToDefinitionItemDto(await GetDefinitionAsync(entity.Id));
    }

    public async Task DeleteDefinitionAsync(Guid id)
    {
        EnsureAdmin();

        ResDefinition entity = await GetEntityAsync(_dbContext.ResDefinitions, id, "资源定义不存在");
        bool isReferenced = await _dbContext.Resources.AnyAsync(r =>
            r.DefinitionId == id);
        bool hasUserResources = await _dbContext.UserResources.AnyAsync(resource =>
            resource.DefinitionId == id);
        if (isReferenced || hasUserResources)
        {
            throw new BusinessException("资源定义已被资源引用，不能删除", StatusCodes.Status409Conflict);
        }

        await _dbContext.ResDefinitions.Where(item => item.Id == id).ExecuteDeleteAsync();
    }

    private async Task<ResDefinition> GetDefinitionAsync(Guid id)
    {
        ResDefinition entity = await _dbContext.ResDefinitions
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new BusinessException("资源定义不存在", StatusCodes.Status404NotFound);
        entity.PropertyMaps = await _dbContext.ResDefinitionPropertyMaps
            .Where(map => map.DefinitionId == id).OrderBy(map => map.Sort).ToListAsync();
        Guid[] propertyIds = entity.PropertyMaps.Select(map => map.PropertyId).ToArray();
        Dictionary<Guid, ResDefinitionProperty> properties = (await _dbContext.ResDefinitionProperties
            .Where(property => propertyIds.Contains(property.Id)).ToListAsync()).ToDictionary(property => property.Id);
        foreach (ResDefinitionPropertyMap map in entity.PropertyMaps)
        {
            if (properties.TryGetValue(map.PropertyId, out ResDefinitionProperty? property))
            {
                map.Property = property;
            }
        }
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
        Guid[] requestedIdValues = requestedIds.ToArray();
        if (requestedIds.Count != inputs.Count(input => input.Id.HasValue))
        {
            throw new BusinessException("资源定义属性不能重复", StatusCodes.Status400BadRequest);
        }

        List<ResDefinitionProperty> referencedProperties = await _dbContext.ResDefinitionProperties
            .Where(property =>
                requestedIdValues.Contains(property.Id))
            .ToListAsync();
        Dictionary<Guid, ResDefinitionProperty> propertiesById = referencedProperties
            .ToDictionary(property => property.Id);

        HashSet<string> requestedNames = inputs
            .Where(input => !input.Id.HasValue)
            .Select(input => NormalizeName(input.Name))
            .Where(name => name.Length != 0)
            .ToHashSet(StringComparer.Ordinal);
        string[] requestedNameValues = requestedNames.ToArray();
        List<ResDefinitionProperty> namedProperties = await _dbContext.ResDefinitionProperties
            .Where(property =>
                requestedNameValues.Contains(property.Name))
            .ToListAsync();
        Dictionary<string, ResDefinitionProperty> propertiesByName = namedProperties
            .ToDictionary(property => NormalizeName(property.Name), StringComparer.Ordinal);
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
                        map.PropertyId == property.Id &&
                        map.DefinitionId != definition!.Id);
                    if (usedByAnotherDefinition)
                    {
                        throw new BusinessException(
                            "已复用的资源属性不能在资源定义中修改",
                            StatusCodes.Status409Conflict);
                    }

                    property.Name = input.Name.Trim();
                    property.ValueType = input.ValueType;
                    property.IsRequired = input.IsRequired;
                    property.MaxLength = input.MaxLength;
                    await _dbContext.ResDefinitionProperties.Where(item => item.Id == property.Id)
                        .ExecuteUpdateAsync(s => s.Set(item => item.Name, property.Name)
            .Set(item => item.ValueType, property.ValueType)
                            .Set(item => item.IsRequired, property.IsRequired)
                            .Set(item => item.MaxLength, property.MaxLength));
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
                    ValueType = input.ValueType,
                    IsRequired = input.IsRequired,
                    MaxLength = input.MaxLength,
                };
                await _dbContext.ResDefinitionProperties.InsertAsync(property);
                propertiesByName[NormalizeName(property.Name)] = property;
            }

            selections.Add(new DefinitionPropertySelection(property, input.Sort));
        }

        bool hasDuplicateName = selections
            .GroupBy(selection => selection.Property.Name, StringComparer.Ordinal)
            .Any(group => group.Count() > 1);
        if (hasDuplicateName)
        {
            throw new BusinessException("资源定义属性不能重复", StatusCodes.Status400BadRequest);
        }

        return selections;
    }

    private async Task EnsurePropertyNameAvailableAsync(string name, Guid? exceptId)
    {
        string normalizedName = NormalizeName(name);
        bool exists = exceptId is Guid excludedId
            ? await _dbContext.ResDefinitionProperties.AnyAsync(property =>
                property.Id != excludedId && property.Name == normalizedName)
            : await _dbContext.ResDefinitionProperties.AnyAsync(property =>
                property.Name == normalizedName);
        if (exists)
        {
            throw new BusinessException("资源属性名称已存在", StatusCodes.Status409Conflict);
        }
    }

    private static ResDefinitionItemDto ToDefinitionItemDto(ResDefinition definition)
    {
        List<ResDefinitionPropertyItemDto> properties = definition.PropertyMaps
            .OrderBy(map => map.Sort)
            .Where(map => map.Property is not null)
            .Select(map => ResDefinitionPropertyItemDto.FromEntity(map.Property, map.Sort))
            .ToList();
        return ResDefinitionItemDto.FromEntity(definition, properties);
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
        return string.Equals(property.Name, input.Name.Trim(), StringComparison.Ordinal) &&
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
        return name?.Trim() ?? string.Empty;
    }

    private sealed record DefinitionPropertySelection(ResDefinitionProperty Property, int Sort);

    public async Task<List<ResPermission>> GetPermissionsAsync(Guid environmentId, Guid categoryId)
    {
        return await _dbContext.ResPermissions
            .Where(p =>
                p.EnvironmentId == environmentId &&
                p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task SetPermissionsAsync(ResPermissionUpdateDto input)
    {
        EnsureAdmin();

        bool environmentExists = await _dbContext.ResEnvironments.AnyAsync(e =>
            e.Id == input.EnvironmentId);
        bool categoryExists = await _dbContext.ResCategories.AnyAsync(c =>
            c.Id == input.CategoryId);
        if (!environmentExists || !categoryExists)
        {
            throw new BusinessException("环境或分类不存在", StatusCodes.Status400BadRequest);
        }

        List<Guid> roleIds = input.RoleIds.Distinct().ToList();
        // RoleId is an opaque identifier owned by the role provider module.
        // ResourceMod stores and compares it but must not query that module's entities.

        List<ResPermission> existing = await GetPermissionsAsync(input.EnvironmentId, input.CategoryId);
        List<ResPermission> permissions = roleIds.Select(roleId => new ResPermission
        {
            RoleId = roleId,
            EnvironmentId = input.EnvironmentId,
            CategoryId = input.CategoryId,
        }).ToList();
        await _dbContext.TransactionAsync(async _ =>
        {
            if (existing.Count != 0)
            {
                await _dbContext.ResPermissions.Where(item => item.EnvironmentId == input.EnvironmentId && item.CategoryId == input.CategoryId)
                    .ExecuteDeleteAsync();
            }
            if (permissions.Count != 0)
            {
                await _dbContext.ResPermissions.BulkInsertAsync(permissions);
            }
        });
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        return Task.FromResult(_userContext.IsAdmin);
    }

    private async Task EnsureCategoryAsync(Guid id)
    {
        bool categoryExists = await _dbContext.ResCategories.AnyAsync(c =>
            c.Id == id);
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

    private async Task<T> GetEntityAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        DbSet<T> set, Guid id, string message)
        where T : EntityBase, new()
    {
        return await set.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new BusinessException(message, StatusCodes.Status404NotFound);
    }
}
