using Entity.ResourceMod;

namespace AdminService.Controllers.ResourceMod;

public class ResourceConfigurationController(
    Localizer localizer, IUserContext user, ILogger<ResourceConfigurationManager> logger, ResourceConfigurationManager manager
) : RestControllerBase<ResourceConfigurationManager>(localizer, manager, user, logger)
{
    [HttpGet("environments")] public Task<List<ResEnvironment>> EnvironmentsAsync() => _manager.EnvironmentsAsync();
    [HttpPost("environments")] public Task<ResEnvironment> AddEnvironmentAsync(ResEnvironmentInput input) => _manager.AddEnvironmentAsync(input);
    [HttpGet("categories")] public Task<List<ResCategory>> CategoriesAsync() => _manager.CategoriesAsync();
    [HttpPost("categories")] public Task<ResCategory> AddCategoryAsync(ResCategoryInput input) => _manager.AddCategoryAsync(input);
    [HttpGet("groups")] public Task<List<ResGroup>> GroupsAsync(Guid categoryId) => _manager.GroupsAsync(categoryId);
    [HttpPost("groups")] public Task<ResGroup> AddGroupAsync(ResGroupInput input) => _manager.AddGroupAsync(input);
    [HttpGet("tags")] public Task<List<ResTag>> TagsAsync() => _manager.TagsAsync();
    [HttpPost("tags")] public Task<ResTag> AddTagAsync(ResTagInput input) => _manager.AddTagAsync(input);
    [HttpGet("definitions")] public Task<List<ResDefinition>> DefinitionsAsync() => _manager.DefinitionsAsync();
    [HttpPost("definitions")] public Task<ResDefinition> AddDefinitionAsync(ResDefinitionInput input) => _manager.AddDefinitionAsync(input);
    [HttpGet("roles")] public Task<List<Entity.SystemMod.SystemRole>> RolesAsync() => _manager.RolesAsync();
    [HttpGet("permissions")] public Task<List<ResPermission>> PermissionsAsync(Guid environmentId, Guid categoryId) => _manager.GetPermissionsAsync(environmentId, categoryId);
    [HttpPut("permissions")] public Task SetPermissionsAsync(ResPermissionInput input) => _manager.SetPermissionsAsync(input);
}
