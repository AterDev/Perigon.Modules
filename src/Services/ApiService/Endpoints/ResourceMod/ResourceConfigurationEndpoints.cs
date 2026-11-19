using Entity.ResourceMod;
using ResourceMod.Managers;
using ResourceMod.Models.ResCategoryDtos;
using ResourceMod.Models.ResDefinitionDtos;
using ResourceMod.Models.ResDefinitionPropertyDtos;
using ResourceMod.Models.ResEnvironmentDtos;
using ResourceMod.Models.ResGroupDtos;
using ResourceMod.Models.ResPermissionDtos;
using ResourceMod.Models.ResTagDtos;
using Share.Implement;

namespace ApiService.Endpoints.ResourceMod;

/// <summary>资源基础配置 Minimal API endpoints.</summary>
public sealed class ResourceConfigurationEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/ResourceConfiguration", "ResourceConfiguration", WebConst.User);
        group.MapGet("environments", EnvironmentsAsync);
        group.MapPost("environments", AddEnvironmentAsync);
        group.MapPut("environments/{id:guid}", UpdateEnvironmentAsync);
        group.MapDelete("environments/{id:guid}", DeleteEnvironmentAsync);
        group.MapGet("categories", CategoriesAsync);
        group.MapPost("categories", AddCategoryAsync);
        group.MapPut("categories/{id:guid}", UpdateCategoryAsync);
        group.MapDelete("categories/{id:guid}", DeleteCategoryAsync);
        group.MapGet("groups", GroupsAsync);
        group.MapPost("groups", AddGroupAsync);
        group.MapPut("groups/{id:guid}", UpdateGroupAsync);
        group.MapDelete("groups/{id:guid}", DeleteGroupAsync);
        group.MapGet("tags", TagsAsync);
        group.MapPost("tags", AddTagAsync);
        group.MapPut("tags/{id:guid}", UpdateTagAsync);
        group.MapDelete("tags/{id:guid}", DeleteTagAsync);
        group.MapGet("properties", PropertiesAsync);
        group.MapPost("properties", AddPropertyAsync);
        group.MapPut("properties/{id:guid}", UpdatePropertyAsync);
        group.MapDelete("properties/{id:guid}", DeletePropertyAsync);
        group.MapGet("definitions", DefinitionsAsync);
        group.MapPost("definitions", AddDefinitionAsync);
        group.MapPut("definitions/{id:guid}", UpdateDefinitionAsync);
        group.MapDelete("definitions/{id:guid}", DeleteDefinitionAsync);
        group.MapGet("permissions", PermissionsAsync);
        group.MapPut("permissions", SetPermissionsAsync);
    }

    public static Task<List<ResEnvironment>> EnvironmentsAsync(ResourceConfigurationManager manager) => manager.EnvironmentsAsync();
    public static Task<ResEnvironment> AddEnvironmentAsync(ResEnvironmentAddDto input, ResourceConfigurationManager manager) => manager.AddEnvironmentAsync(input);
    public static Task<ResEnvironment> UpdateEnvironmentAsync(Guid id, ResEnvironmentUpdateDto input, ResourceConfigurationManager manager) => manager.UpdateEnvironmentAsync(id, input);
    public static Task DeleteEnvironmentAsync(Guid id, ResourceConfigurationManager manager) => manager.DeleteEnvironmentAsync(id);

    public static Task<List<ResCategory>> CategoriesAsync(ResourceConfigurationManager manager) => manager.CategoriesAsync();
    public static Task<ResCategory> AddCategoryAsync(ResCategoryAddDto input, ResourceConfigurationManager manager) => manager.AddCategoryAsync(input);
    public static Task<ResCategory> UpdateCategoryAsync(Guid id, ResCategoryUpdateDto input, ResourceConfigurationManager manager) => manager.UpdateCategoryAsync(id, input);
    public static Task DeleteCategoryAsync(Guid id, ResourceConfigurationManager manager) => manager.DeleteCategoryAsync(id);

    public static Task<List<ResGroup>> GroupsAsync(Guid? categoryId, ResourceConfigurationManager manager) => manager.GroupsAsync(categoryId);
    public static Task<ResGroup> AddGroupAsync(ResGroupAddDto input, ResourceConfigurationManager manager) => manager.AddGroupAsync(input);
    public static Task<ResGroup> UpdateGroupAsync(Guid id, ResGroupUpdateDto input, ResourceConfigurationManager manager) => manager.UpdateGroupAsync(id, input);
    public static Task DeleteGroupAsync(Guid id, ResourceConfigurationManager manager) => manager.DeleteGroupAsync(id);

    public static Task<List<ResTag>> TagsAsync(ResourceConfigurationManager manager) => manager.TagsAsync();
    public static Task<ResTag> AddTagAsync(ResTagAddDto input, ResourceConfigurationManager manager) => manager.AddTagAsync(input);
    public static Task<ResTag> UpdateTagAsync(Guid id, ResTagUpdateDto input, ResourceConfigurationManager manager) => manager.UpdateTagAsync(id, input);
    public static Task DeleteTagAsync(Guid id, ResourceConfigurationManager manager) => manager.DeleteTagAsync(id);

    public static Task<List<ResDefinitionPropertyItemDto>> PropertiesAsync(string? name, ResourceConfigurationManager manager) => manager.PropertiesAsync(name);
    public static Task<ResDefinitionPropertyItemDto> AddPropertyAsync(ResDefinitionPropertyAddDto input, ResourceConfigurationManager manager) => manager.AddPropertyAsync(input);
    public static Task<ResDefinitionPropertyItemDto> UpdatePropertyAsync(Guid id, ResDefinitionPropertyUpdateDto input, ResourceConfigurationManager manager) => manager.UpdatePropertyAsync(id, input);
    public static Task DeletePropertyAsync(Guid id, ResourceConfigurationManager manager) => manager.DeletePropertyAsync(id);

    public static Task<List<ResDefinitionItemDto>> DefinitionsAsync(string? name, ResourceConfigurationManager manager) => manager.DefinitionsAsync(name);
    public static Task<ResDefinitionItemDto> AddDefinitionAsync(ResDefinitionAddDto input, ResourceConfigurationManager manager) => manager.AddDefinitionAsync(input);
    public static Task<ResDefinitionItemDto> UpdateDefinitionAsync(Guid id, ResDefinitionUpdateDto input, ResourceConfigurationManager manager) => manager.UpdateDefinitionAsync(id, input);
    public static Task DeleteDefinitionAsync(Guid id, ResourceConfigurationManager manager) => manager.DeleteDefinitionAsync(id);

    public static Task<List<ResPermission>> PermissionsAsync(Guid environmentId, Guid categoryId, ResourceConfigurationManager manager) => manager.GetPermissionsAsync(environmentId, categoryId);
    public static Task SetPermissionsAsync(ResPermissionUpdateDto input, ResourceConfigurationManager manager) => manager.SetPermissionsAsync(input);
}
