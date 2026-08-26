using Entity.ResourceMod;

namespace AdminService.Controllers.ResourceMod;

/// <summary>
/// 资源基础配置管理，包括环境、分类、分组、标签、属性定义、资源定义和资源权限。
/// </summary>
/// <see cref="ResourceConfigurationManager"/>
public class ResourceConfigurationController(
    Localizer localizer,
    IUserContext user,
    ILogger<ResourceConfigurationManager> logger,
    ResourceConfigurationManager manager
) : RestControllerBase<ResourceConfigurationManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 获取当前租户的资源环境列表。
    /// </summary>
    /// <returns>按名称排序的资源环境列表。</returns>
    [HttpGet("environments")]
    public Task<List<ResEnvironment>> EnvironmentsAsync()
    {
        return _manager.EnvironmentsAsync();
    }

    /// <summary>
    /// 新增资源环境。
    /// </summary>
    /// <param name="input">资源环境新增信息。</param>
    /// <returns>新创建的资源环境。</returns>
    [HttpPost("environments")]
    public Task<ResEnvironment> AddEnvironmentAsync(ResEnvironmentAddDto input)
    {
        return _manager.AddEnvironmentAsync(input);
    }

    /// <summary>
    /// 更新资源环境。
    /// </summary>
    /// <param name="id">资源环境唯一标识。</param>
    /// <param name="input">资源环境更新信息。</param>
    /// <returns>更新后的资源环境。</returns>
    [HttpPut("environments/{id:guid}")]
    public Task<ResEnvironment> UpdateEnvironmentAsync(
        [FromRoute] Guid id,
        ResEnvironmentUpdateDto input)
    {
        return _manager.UpdateEnvironmentAsync(id, input);
    }

    /// <summary>
    /// 删除资源环境。
    /// </summary>
    /// <param name="id">资源环境唯一标识。</param>
    /// <returns>删除操作完成。</returns>
    [HttpDelete("environments/{id:guid}")]
    public Task DeleteEnvironmentAsync([FromRoute] Guid id)
    {
        return _manager.DeleteEnvironmentAsync(id);
    }

    /// <summary>
    /// 获取当前租户的资源分类列表。
    /// </summary>
    /// <returns>按名称排序的资源分类列表。</returns>
    [HttpGet("categories")]
    public Task<List<ResCategory>> CategoriesAsync()
    {
        return _manager.CategoriesAsync();
    }

    /// <summary>
    /// 新增资源分类。
    /// </summary>
    /// <param name="input">资源分类新增信息。</param>
    /// <returns>新创建的资源分类。</returns>
    [HttpPost("categories")]
    public Task<ResCategory> AddCategoryAsync(ResCategoryAddDto input)
    {
        return _manager.AddCategoryAsync(input);
    }

    /// <summary>
    /// 更新资源分类。
    /// </summary>
    /// <param name="id">资源分类唯一标识。</param>
    /// <param name="input">资源分类更新信息。</param>
    /// <returns>更新后的资源分类。</returns>
    [HttpPut("categories/{id:guid}")]
    public Task<ResCategory> UpdateCategoryAsync(
        [FromRoute] Guid id,
        ResCategoryUpdateDto input)
    {
        return _manager.UpdateCategoryAsync(id, input);
    }

    /// <summary>
    /// 删除资源分类。
    /// </summary>
    /// <param name="id">资源分类唯一标识。</param>
    /// <returns>删除操作完成。</returns>
    [HttpDelete("categories/{id:guid}")]
    public Task DeleteCategoryAsync([FromRoute] Guid id)
    {
        return _manager.DeleteCategoryAsync(id);
    }

    /// <summary>
    /// 获取资源分组列表。
    /// </summary>
    /// <param name="categoryId">可选的资源分类标识；指定后仅返回该分类下的分组。</param>
    /// <returns>按名称排序的资源分组列表。</returns>
    [HttpGet("groups")]
    public Task<List<ResGroup>> GroupsAsync([FromQuery] Guid? categoryId)
    {
        return _manager.GroupsAsync(categoryId);
    }

    /// <summary>
    /// 新增资源分组。
    /// </summary>
    /// <param name="input">资源分组新增信息。</param>
    /// <returns>新创建的资源分组。</returns>
    [HttpPost("groups")]
    public Task<ResGroup> AddGroupAsync(ResGroupAddDto input)
    {
        return _manager.AddGroupAsync(input);
    }

    /// <summary>
    /// 更新资源分组。
    /// </summary>
    /// <param name="id">资源分组唯一标识。</param>
    /// <param name="input">资源分组更新信息。</param>
    /// <returns>更新后的资源分组。</returns>
    [HttpPut("groups/{id:guid}")]
    public Task<ResGroup> UpdateGroupAsync(
        [FromRoute] Guid id,
        ResGroupUpdateDto input)
    {
        return _manager.UpdateGroupAsync(id, input);
    }

    /// <summary>
    /// 删除资源分组。
    /// </summary>
    /// <param name="id">资源分组唯一标识。</param>
    /// <returns>删除操作完成。</returns>
    [HttpDelete("groups/{id:guid}")]
    public Task DeleteGroupAsync([FromRoute] Guid id)
    {
        return _manager.DeleteGroupAsync(id);
    }

    /// <summary>
    /// 获取当前租户的资源标签列表。
    /// </summary>
    /// <returns>按名称排序的资源标签列表。</returns>
    [HttpGet("tags")]
    public Task<List<ResTag>> TagsAsync()
    {
        return _manager.TagsAsync();
    }

    /// <summary>
    /// 新增资源标签。
    /// </summary>
    /// <param name="input">资源标签新增信息。</param>
    /// <returns>新创建的资源标签。</returns>
    [HttpPost("tags")]
    public Task<ResTag> AddTagAsync(ResTagAddDto input)
    {
        return _manager.AddTagAsync(input);
    }

    /// <summary>
    /// 更新资源标签。
    /// </summary>
    /// <param name="id">资源标签唯一标识。</param>
    /// <param name="input">资源标签更新信息。</param>
    /// <returns>更新后的资源标签。</returns>
    [HttpPut("tags/{id:guid}")]
    public Task<ResTag> UpdateTagAsync(
        [FromRoute] Guid id,
        ResTagUpdateDto input)
    {
        return _manager.UpdateTagAsync(id, input);
    }

    /// <summary>
    /// 删除资源标签。
    /// </summary>
    /// <param name="id">资源标签唯一标识。</param>
    /// <returns>删除操作完成。</returns>
    [HttpDelete("tags/{id:guid}")]
    public Task DeleteTagAsync([FromRoute] Guid id)
    {
        return _manager.DeleteTagAsync(id);
    }

    /// <summary>
    /// 获取资源属性定义列表。
    /// </summary>
    /// <param name="name">可选的属性名称关键字。</param>
    /// <returns>按名称排序的资源属性定义列表。</returns>
    [HttpGet("properties")]
    public Task<List<ResDefinitionProperty>> PropertiesAsync([FromQuery] string? name)
    {
        return _manager.PropertiesAsync(name);
    }

    /// <summary>
    /// 新增资源属性定义。
    /// </summary>
    /// <param name="input">资源属性定义新增信息。</param>
    /// <returns>新创建的资源属性定义。</returns>
    [HttpPost("properties")]
    public Task<ResDefinitionProperty> AddPropertyAsync(ResDefinitionPropertyAddDto input)
    {
        return _manager.AddPropertyAsync(input);
    }

    /// <summary>
    /// 更新资源属性定义。
    /// </summary>
    /// <param name="id">资源属性定义唯一标识。</param>
    /// <param name="input">资源属性定义更新信息。</param>
    /// <returns>更新后的资源属性定义。</returns>
    [HttpPut("properties/{id:guid}")]
    public Task<ResDefinitionProperty> UpdatePropertyAsync(
        [FromRoute] Guid id,
        ResDefinitionPropertyUpdateDto input)
    {
        return _manager.UpdatePropertyAsync(id, input);
    }

    /// <summary>
    /// 删除资源属性定义。
    /// </summary>
    /// <param name="id">资源属性定义唯一标识。</param>
    /// <returns>删除操作完成。</returns>
    [HttpDelete("properties/{id:guid}")]
    public Task DeletePropertyAsync([FromRoute] Guid id)
    {
        return _manager.DeletePropertyAsync(id);
    }

    /// <summary>
    /// 获取资源定义列表。
    /// </summary>
    /// <param name="name">可选的资源定义名称关键字。</param>
    /// <returns>按名称排序且包含属性配置的资源定义列表。</returns>
    [HttpGet("definitions")]
    public Task<List<ResDefinition>> DefinitionsAsync([FromQuery] string? name)
    {
        return _manager.DefinitionsAsync(name);
    }

    /// <summary>
    /// 新增资源定义。
    /// </summary>
    /// <param name="input">资源定义新增信息。</param>
    /// <returns>新创建的资源定义。</returns>
    [HttpPost("definitions")]
    public Task<ResDefinition> AddDefinitionAsync(ResDefinitionAddDto input)
    {
        return _manager.AddDefinitionAsync(input);
    }

    /// <summary>
    /// 更新资源定义。
    /// </summary>
    /// <param name="id">资源定义唯一标识。</param>
    /// <param name="input">资源定义更新信息。</param>
    /// <returns>更新后的资源定义。</returns>
    [HttpPut("definitions/{id:guid}")]
    public Task<ResDefinition> UpdateDefinitionAsync(
        [FromRoute] Guid id,
        ResDefinitionUpdateDto input)
    {
        return _manager.UpdateDefinitionAsync(id, input);
    }

    /// <summary>
    /// 删除资源定义。
    /// </summary>
    /// <param name="id">资源定义唯一标识。</param>
    /// <returns>删除操作完成。</returns>
    [HttpDelete("definitions/{id:guid}")]
    public Task DeleteDefinitionAsync([FromRoute] Guid id)
    {
        return _manager.DeleteDefinitionAsync(id);
    }

    /// <summary>
    /// 获取指定环境和分类的资源权限。
    /// </summary>
    /// <param name="environmentId">资源环境唯一标识。</param>
    /// <param name="categoryId">资源分类唯一标识。</param>
    /// <returns>指定环境和分类下的角色授权列表。</returns>
    [HttpGet("permissions")]
    public Task<List<ResPermission>> PermissionsAsync(
        [FromQuery] Guid environmentId,
        [FromQuery] Guid categoryId)
    {
        return _manager.GetPermissionsAsync(environmentId, categoryId);
    }

    /// <summary>
    /// 替换指定环境和分类的资源权限。
    /// </summary>
    /// <param name="input">资源权限更新信息，包含环境、分类和完整角色标识列表。</param>
    /// <returns>权限替换操作完成。</returns>
    [HttpPut("permissions")]
    public Task SetPermissionsAsync(ResPermissionUpdateDto input)
    {
        return _manager.SetPermissionsAsync(input);
    }
}
