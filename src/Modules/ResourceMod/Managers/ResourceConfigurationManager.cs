using Entity.SystemMod;
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
    public async Task<List<ResEnvironment>> EnvironmentsAsync() => await _dbContext.ResEnvironments.Where(e => e.TenantId == _userContext.TenantId).OrderBy(e => e.Name).ToListAsync();
    public async Task<List<ResCategory>> CategoriesAsync() => await _dbContext.ResCategories.Where(c => c.TenantId == _userContext.TenantId).OrderBy(c => c.Name).ToListAsync();
    public async Task<List<ResGroup>> GroupsAsync(Guid categoryId) => await _dbContext.ResGroups.Where(g => g.TenantId == _userContext.TenantId && g.CategoryId == categoryId).OrderBy(g => g.Name).ToListAsync();
    public async Task<List<ResTag>> TagsAsync() => await _dbContext.ResTags.Where(t => t.TenantId == _userContext.TenantId).OrderBy(t => t.Name).ToListAsync();
    public async Task<List<ResDefinition>> DefinitionsAsync() => await _dbContext.ResDefinitions.Where(d => d.TenantId == _userContext.TenantId).Include(d => d.Properties).OrderBy(d => d.Name).ToListAsync();
    public async Task<List<SystemRole>> RolesAsync() => await _dbContext.SystemRoles.Where(r => r.TenantId == _userContext.TenantId).OrderBy(r => r.Name).ToListAsync();

    public async Task<ResEnvironment> AddEnvironmentAsync(ResEnvironmentInput input)
    {
        EnsureAdmin();
        ResEnvironment entity = new() { Name = input.Name, Icon = input.Icon, Color = input.Color, TenantId = _userContext.TenantId };
        await _dbContext.ResEnvironments.AddAsync(entity); await _dbContext.SaveChangesAsync(); return entity;
    }
    public async Task<ResCategory> AddCategoryAsync(ResCategoryInput input)
    {
        EnsureAdmin();
        if (await _dbContext.ResCategories.AnyAsync(c => c.TenantId == _userContext.TenantId && c.CatalogCode == input.CatalogCode)) throw new BusinessException("分类编码已存在", StatusCodes.Status409Conflict);
        ResCategory entity = new() { Name = input.Name, CatalogCode = input.CatalogCode, Icon = input.Icon, Color = input.Color, TenantId = _userContext.TenantId };
        await _dbContext.ResCategories.AddAsync(entity); await _dbContext.SaveChangesAsync(); return entity;
    }
    public async Task<ResGroup> AddGroupAsync(ResGroupInput input)
    {
        EnsureAdmin(); await EnsureCategoryAsync(input.CategoryId);
        ResGroup entity = new() { Name = input.Name, Description = input.Description, Icon = input.Icon, Color = input.Color, CategoryId = input.CategoryId, TenantId = _userContext.TenantId };
        await _dbContext.ResGroups.AddAsync(entity); await _dbContext.SaveChangesAsync(); return entity;
    }
    public async Task<ResTag> AddTagAsync(ResTagInput input)
    {
        EnsureAdmin(); ResTag entity = new() { Name = input.Name, Color = input.Color, Icon = input.Icon, TenantId = _userContext.TenantId };
        await _dbContext.ResTags.AddAsync(entity); await _dbContext.SaveChangesAsync(); return entity;
    }
    public async Task<ResDefinition> AddDefinitionAsync(ResDefinitionInput input)
    {
        EnsureAdmin(); ValidateProperties(input.Properties);
        ResDefinition entity = new() { Name = input.Name, Icon = input.Icon, TenantId = _userContext.TenantId };
        foreach (ResDefinitionPropertyInput property in input.Properties) entity.Properties.Add(new ResDefinitionProperty { Name = property.Name, ValueType = property.ValueType, IsRequired = property.IsRequired, MaxLength = property.MaxLength, Sort = property.Sort, TenantId = _userContext.TenantId });
        await _dbContext.ResDefinitions.AddAsync(entity); await _dbContext.SaveChangesAsync(); return entity;
    }
    public async Task<List<ResPermission>> GetPermissionsAsync(Guid environmentId, Guid categoryId) => await _dbContext.ResPermissions.Where(p => p.TenantId == _userContext.TenantId && p.EnvironmentId == environmentId && p.CategoryId == categoryId).ToListAsync();
    public async Task SetPermissionsAsync(ResPermissionInput input)
    {
        EnsureAdmin();
        if (!await _dbContext.ResEnvironments.AnyAsync(e => e.Id == input.EnvironmentId && e.TenantId == _userContext.TenantId) || !await _dbContext.ResCategories.AnyAsync(c => c.Id == input.CategoryId && c.TenantId == _userContext.TenantId)) throw new BusinessException("环境或分类不存在", StatusCodes.Status400BadRequest);
        List<Guid> roleIds = input.RoleIds.Distinct().ToList();
        if (roleIds.Count != await _dbContext.SystemRoles.CountAsync(r => r.TenantId == _userContext.TenantId && roleIds.Contains(r.Id))) throw new BusinessException("角色不存在", StatusCodes.Status400BadRequest);
        List<ResPermission> existing = await GetPermissionsAsync(input.EnvironmentId, input.CategoryId);
        _dbContext.ResPermissions.RemoveRange(existing);
        await _dbContext.ResPermissions.AddRangeAsync(roleIds.Select(roleId => new ResPermission { RoleId = roleId, EnvironmentId = input.EnvironmentId, CategoryId = input.CategoryId, TenantId = _userContext.TenantId }));
        await _dbContext.SaveChangesAsync();
    }
    public override Task<bool> HasPermissionAsync(Guid id) => Task.FromResult(_userContext.IsAdmin);
    private async Task EnsureCategoryAsync(Guid id) { if (!await _dbContext.ResCategories.AnyAsync(c => c.Id == id && c.TenantId == _userContext.TenantId)) throw new BusinessException("分类不存在", StatusCodes.Status400BadRequest); }
    private static void ValidateProperties(List<ResDefinitionPropertyInput> properties) { if (properties.GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1) || properties.Any(p => p.MaxLength is < 1 or > 1000)) throw new BusinessException("资源定义属性无效", StatusCodes.Status400BadRequest); }
    private void EnsureAdmin() { if (!_userContext.IsAdmin) throw new BusinessException("无管理资源权限", StatusCodes.Status403Forbidden); }
}
