using Entity.SystemMod;
using Perigon.AspNetCore.Models;
using SystemMod.Managers;
using SystemMod.Models.SysRoleDtos;

namespace ApiService.Endpoints.SystemMod;

public sealed class SysRoleEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/SysRole", "SysRole", WebConst.User)
            .RequireAuthorization(WebConst.SuperAdmin);
        group.MapGet("", ListAsync);
        group.MapPost("", AddAsync);
        group.MapPatch("/{id}", UpdateAsync);
        group.MapPut("/menus", UpdateMenusAsync);
        group.MapGet("/{id}", DetailAsync);
        group.MapDelete("/{id}", DeleteAsync);
    }

    private static Task<PageList<SysRoleItemDto>> ListAsync(
        [AsParameters] SysRoleFilterDto filter,
        SysRoleManager manager,
        CancellationToken cancellationToken) => manager.FilterAsync(filter);

    private static async Task<IResult> AddAsync(
        SysRoleAddDto dto,
        SysRoleManager manager,
        CancellationToken cancellationToken)
    {
        var entity = await manager.AddAsync(dto);
        return TypedResults.Created($"/api/SysRole/{entity.Id}", entity);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SysRoleUpdateDto dto,
        SysRoleManager manager,
        CancellationToken cancellationToken) => TypedResults.Ok(await manager.UpdateAsync(id, dto));

    private static async Task<IResult> UpdateMenusAsync(
        SysRoleSetMenusDto dto,
        SysRoleManager manager,
        CancellationToken cancellationToken) => TypedResults.Ok(await manager.SetMenusAsync(dto));

    private static async Task<IResult> DetailAsync(
        Guid id,
        SysRoleManager manager,
        CancellationToken cancellationToken)
    {
        var result = await manager.GetAsync(id);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        SysRoleManager manager,
        CancellationToken cancellationToken)
    {
        await manager.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}
