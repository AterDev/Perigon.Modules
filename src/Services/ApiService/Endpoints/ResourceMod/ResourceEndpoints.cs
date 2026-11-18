using Entity.ResourceMod;
using Perigon.AspNetCore.Models;
using ResourceMod.Managers;
using ResourceMod.Models.ResourceDtos;
using Share.Implement;

namespace ApiService.Endpoints.ResourceMod;

/// <summary>资源 Minimal API endpoints.</summary>
public sealed class ResourceEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/Resource", "Resource", WebConst.User);
        group.MapGet("list", ListAsync);
        group.MapGet("{id}", DetailAsync);
        group.MapPost("", AddAsync);
        group.MapPatch("{id}", UpdateAsync);
        group.MapDelete("{id}", DeleteAsync);
    }

    public static Task<PageList<ResourceItemDto>> ListAsync(
        [AsParameters] ResourceFilterDto filter,
        ResourceManager manager) => manager.FilterAsync(filter);

    public static Task<ResourceDetailDto?> DetailAsync(Guid id, ResourceManager manager) =>
        manager.GetAsync(id);

    public static async Task<ApiResult<ResourceCreatedDto>> AddAsync(
        ResourceAddDto input,
        ResourceManager manager)
    {
        Resource resource = await manager.AddAsync(input);
        return TypedResults.Created(
            $"/api/Resource/{resource.Id}",
            new ResourceCreatedDto { Id = resource.Id });
    }

    public static Task<bool> UpdateAsync(
        Guid id,
        ResourceUpdateDto input,
        ResourceManager manager) => manager.UpdateAsync(id, input);

    public static Task<bool> DeleteAsync(Guid id, ResourceManager manager) =>
        manager.DeleteAsync(id);
}
