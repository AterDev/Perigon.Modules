using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using ResourceMod.Models;
using Share.Exceptions;

namespace ResourceMod.Managers;

public class ResourceConfigurationManager(
    TenantDbFactory dbContextFactory,
    ILogger<ResourceConfigurationManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ResEnvironment>(dbContextFactory, userContext, logger)
{
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

    public async Task<List<ResGroup>> GroupsAsync(Guid categoryId)
    {
        return await _dbContext.ResGroups
            .Where(g => g.TenantId == _userContext.TenantId && g.CategoryId == categoryId)
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
            .Include(d => d.Properties);
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(d => d.Name.Contains(name.Trim()));
        }

        return await query.OrderBy(d => d.Name).ToListAsync();
    }

    public async Task<ResEnvironment> AddEnvironmentAsync(ResEnvironmentInput input)
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

    public async Task<ResEnvironment> UpdateEnvironmentAsync(Guid id, ResEnvironmentInput input)
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

    public async Task<ResCategory> AddCategoryAsync(ResCategoryInput input)
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

    public async Task<ResCategory> UpdateCategoryAsync(Guid id, ResCategoryInput input)
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

    public async Task<ResGroup> AddGroupAsync(ResGroupInput input)
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

    public async Task<ResGroup> UpdateGroupAsync(Guid id, ResGroupInput input)
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

    public async Task<ResTag> AddTagAsync(ResTagInput input)
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

    public async Task<ResTag> UpdateTagAsync(Guid id, ResTagInput input)
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

    public async Task<ResDefinition> AddDefinitionAsync(ResDefinitionInput input)
    {
        EnsureAdmin();
        ValidateProperties(input.Properties);

        ResDefinition entity = new()
        {
            Name = input.Name,
            Icon = input.Icon,
            TenantId = _userContext.TenantId
        };

        foreach (ResDefinitionPropertyInput property in input.Properties)
        {
            entity.Properties.Add(new ResDefinitionProperty
            {
                Name = property.Name,
                ValueType = property.ValueType,
                IsRequired = property.IsRequired,
                MaxLength = property.MaxLength,
                Sort = property.Sort,
                TenantId = _userContext.TenantId
            });
        }

        await _dbContext.ResDefinitions.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task<ResDefinition> UpdateDefinitionAsync(Guid id, ResDefinitionInput input)
    {
        EnsureAdmin();
        ValidateProperties(input.Properties);

        ResDefinition entity = await _dbContext.ResDefinitions
            .Include(d => d.Properties)
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == _userContext.TenantId)
            ?? throw new BusinessException("资源定义不存在", StatusCodes.Status404NotFound);
        entity.Name = input.Name;
        entity.Icon = input.Icon;

        HashSet<Guid> retainedIds = input.Properties
            .Where(p => p.Id.HasValue)
            .Select(p => p.Id!.Value)
            .ToHashSet();
        List<ResDefinitionProperty> removed = entity.Properties
            .Where(p => !retainedIds.Contains(p.Id))
            .ToList();
        bool isReferenced = removed.Count != 0 && await _dbContext.ResValues.AnyAsync(v =>
            removed.Select(p => p.Id).Contains(v.DefinitionPropertyId));
        if (isReferenced)
        {
            throw new BusinessException("定义属性已被资源值引用，不能删除", StatusCodes.Status409Conflict);
        }

        _dbContext.ResDefinitionProperties.RemoveRange(removed);

        foreach (ResDefinitionPropertyInput property in input.Properties)
        {
            ResDefinitionProperty? target = property.Id.HasValue
                ? entity.Properties.FirstOrDefault(p => p.Id == property.Id.Value)
                : null;
            if (property.Id.HasValue && target is null)
            {
                throw new BusinessException("定义属性不属于当前定义", StatusCodes.Status400BadRequest);
            }

            if (target is null)
            {
                ResDefinitionProperty newProperty = new()
                {
                    Name = property.Name,
                    ValueType = property.ValueType,
                    IsRequired = property.IsRequired,
                    MaxLength = property.MaxLength,
                    Sort = property.Sort,
                    DefinitionId = entity.Id,
                    TenantId = _userContext.TenantId
                };
                _dbContext.ResDefinitionProperties.Add(newProperty);
            }
            else
            {
                target.Name = property.Name;
                target.ValueType = property.ValueType;
                target.IsRequired = property.IsRequired;
                target.MaxLength = property.MaxLength;
                target.Sort = property.Sort;
            }
        }

        await _dbContext.SaveChangesAsync();

        return entity;
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

    public async Task<List<ResPermission>> GetPermissionsAsync(Guid environmentId, Guid categoryId)
    {
        return await _dbContext.ResPermissions
            .Where(p =>
                p.TenantId == _userContext.TenantId &&
                p.EnvironmentId == environmentId &&
                p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task SetPermissionsAsync(ResPermissionInput input)
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

    private static void ValidateProperties(List<ResDefinitionPropertyInput> properties)
    {
        bool hasDuplicateName = properties
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Any(g => g.Count() > 1);
        bool hasInvalidMaxLength = properties.Any(p => p.MaxLength is < 1 or > 1000);
        if (hasDuplicateName || hasInvalidMaxLength)
        {
            throw new BusinessException("资源定义属性无效", StatusCodes.Status400BadRequest);
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
