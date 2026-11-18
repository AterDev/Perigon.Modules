using Perigon.AspNetCore.Models;
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

    private static async Task<IResult> AddAsync(
        SysDataScopeGroupAddDto dto,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken)
    {
        var entity = await manager.AddAsync(dto);
        return TypedResults.Created($"/api/SysDataScopeGroup/{entity.Id}", entity);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SysDataScopeGroupUpdateDto dto,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken) => TypedResults.Ok(await manager.UpdateAsync(id, dto));

    private static async Task<IResult> GetDetailAsync(
        Guid id,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken)
    {
        var result = await manager.GetAsync(id);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> GetUserIdsAsync(
        Guid id,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken) => TypedResults.Ok(await manager.GetUserIdsAsync(id));

    private static async Task<IResult> SetUsersAsync(
        Guid id,
        SysUserDataScopeGroupSetUsersDto dto,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken)
    {
        await manager.SetUsersAsync(id, dto);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        SysDataScopeGroupManager manager,
        CancellationToken cancellationToken)
    {
        await manager.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}
