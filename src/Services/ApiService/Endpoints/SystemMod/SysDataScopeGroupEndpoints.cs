using Perigon.AspNetCore.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Entity.SystemMod;
using SystemMod.Managers;
using SystemMod.Models.SysDataScopeGroupDtos;
using SystemMod.Models.SysUserDataScopeGroupDtos;

namespace ApiService.Endpoints.SystemMod;

public sealed class SysDataScopeGroupEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/SysDataScopeGroup", "SysDataScopeGroup", WebConst.User)
            .RequireAuthorization(WebConst.SuperAdmin);
        group.MapPost("/filter", FilterAsync);
        group.MapPost("", AddAsync);
        group.MapPatch("/{id}", UpdateAsync);
        group.MapGet("/{id}", GetDetailAsync);
        group.MapGet("/{id}/users", GetUserIdsAsync);
        group.MapPut("/{id}/users", SetUsersAsync);
        group.MapDelete("/{id}", DeleteAsync);
    }

    private static Task<PageList<SysDataScopeGroupItemDto>> FilterAsync(
        SysDataScopeGroupFilterDto filter,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken) => manager.FilterAsync(filter);

    private static async Task<Created<SysDataScopeGroup>> AddAsync(
        SysDataScopeGroupAddDto dto,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken)
    {
        var entity = await manager.AddAsync(dto);
        return TypedResults.Created($"/api/SysDataScopeGroup/{entity.Id}", entity);
    }

    private static async Task<Ok<SysDataScopeGroup>> UpdateAsync(
        Guid id,
        SysDataScopeGroupUpdateDto dto,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken) => TypedResults.Ok(await manager.UpdateAsync(id, dto));

    private static async Task<Results<NotFound, Ok<SysDataScopeGroupDetailDto>>> GetDetailAsync(
        Guid id,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken)
    {
        var result = await manager.GetAsync(id);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<Ok<List<Guid>>> GetUserIdsAsync(
        Guid id,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken) => TypedResults.Ok(await manager.GetUserIdsAsync(id));

    private static async Task<NoContent> SetUsersAsync(
        Guid id,
        SysUserDataScopeGroupSetUsersDto dto,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken)
    {
        await manager.SetUsersAsync(id, dto);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteAsync(
        Guid id,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken)
    {
        await manager.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}
