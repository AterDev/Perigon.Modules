using Entity.ResourceMod;

namespace AdminService.Controllers.ResourceMod;

public class ResourceConfigurationController(
    Localizer localizer,
    IUserContext user,
    ILogger<ResourceConfigurationManager> logger,
    ResourceConfigurationManager manager
) : RestControllerBase<ResourceConfigurationManager>(localizer, manager, user, logger)
{
    [HttpGet("environments")]
    public Task<List<ResEnvironment>> EnvironmentsAsync()
    {
        return _manager.EnvironmentsAsync();
    }

    [HttpPost("environments")]
    public Task<ResEnvironment> AddEnvironmentAsync(ResEnvironmentInput input)
    {
        return _manager.AddEnvironmentAsync(input);
    }

    [HttpPut("environments/{id:guid}")]
    public Task<ResEnvironment> UpdateEnvironmentAsync(Guid id, ResEnvironmentInput input)
    {
        return _manager.UpdateEnvironmentAsync(id, input);
    }

    [HttpDelete("environments/{id:guid}")]
    public Task DeleteEnvironmentAsync(Guid id)
    {
        return _manager.DeleteEnvironmentAsync(id);
    }

    [HttpGet("categories")]
    public Task<List<ResCategory>> CategoriesAsync()
    {
        return _manager.CategoriesAsync();
    }

    [HttpPost("categories")]
    public Task<ResCategory> AddCategoryAsync(ResCategoryInput input)
    {
        return _manager.AddCategoryAsync(input);
    }

    [HttpPut("categories/{id:guid}")]
    public Task<ResCategory> UpdateCategoryAsync(Guid id, ResCategoryInput input)
    {
        return _manager.UpdateCategoryAsync(id, input);
    }

    [HttpDelete("categories/{id:guid}")]
    public Task DeleteCategoryAsync(Guid id)
    {
        return _manager.DeleteCategoryAsync(id);
    }

    [HttpGet("groups")]
    public Task<List<ResGroup>> GroupsAsync(Guid categoryId)
    {
        return _manager.GroupsAsync(categoryId);
    }

    [HttpPost("groups")]
    public Task<ResGroup> AddGroupAsync(ResGroupInput input)
    {
        return _manager.AddGroupAsync(input);
    }

    [HttpPut("groups/{id:guid}")]
    public Task<ResGroup> UpdateGroupAsync(Guid id, ResGroupInput input)
    {
        return _manager.UpdateGroupAsync(id, input);
    }

    [HttpDelete("groups/{id:guid}")]
    public Task DeleteGroupAsync(Guid id)
    {
        return _manager.DeleteGroupAsync(id);
    }

    [HttpGet("tags")]
    public Task<List<ResTag>> TagsAsync()
    {
        return _manager.TagsAsync();
    }

    [HttpPost("tags")]
    public Task<ResTag> AddTagAsync(ResTagInput input)
    {
        return _manager.AddTagAsync(input);
    }

    [HttpPut("tags/{id:guid}")]
    public Task<ResTag> UpdateTagAsync(Guid id, ResTagInput input)
    {
        return _manager.UpdateTagAsync(id, input);
    }

    [HttpDelete("tags/{id:guid}")]
    public Task DeleteTagAsync(Guid id)
    {
        return _manager.DeleteTagAsync(id);
    }

    [HttpGet("definitions")]
    public Task<List<ResDefinition>> DefinitionsAsync()
    {
        return _manager.DefinitionsAsync();
    }

    [HttpPost("definitions")]
    public Task<ResDefinition> AddDefinitionAsync(ResDefinitionInput input)
    {
        return _manager.AddDefinitionAsync(input);
    }

    [HttpPut("definitions/{id:guid}")]
    public Task<ResDefinition> UpdateDefinitionAsync(Guid id, ResDefinitionInput input)
    {
        return _manager.UpdateDefinitionAsync(id, input);
    }

    [HttpDelete("definitions/{id:guid}")]
    public Task DeleteDefinitionAsync(Guid id)
    {
        return _manager.DeleteDefinitionAsync(id);
    }

    [HttpGet("permissions")]
    public Task<List<ResPermission>> PermissionsAsync(Guid environmentId, Guid categoryId)
    {
        return _manager.GetPermissionsAsync(environmentId, categoryId);
    }

    [HttpPut("permissions")]
    public Task SetPermissionsAsync(ResPermissionInput input)
    {
        return _manager.SetPermissionsAsync(input);
    }
}
