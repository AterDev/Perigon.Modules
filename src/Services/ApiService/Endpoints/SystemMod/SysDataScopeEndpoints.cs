using Perigon.AspNetCore.Models;
using SystemMod.Managers;
using SystemMod.Models.SysDataScopeDtos;

namespace ApiService.Endpoints.SystemMod;

public sealed class SysDataScopeEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/SysDataScope", "SysDataScope", WebConst.User)
            .RequireAuthorization(WebConst.SuperAdmin);
        group.MapPost("/filter", FilterAsync);
        group.MapPost("", AddAsync);
        group.MapPatch("/{id}", UpdateAsync);
        group.MapGet("/{id}", GetDetailAsync);
        group.MapDelete("/{id}", DeleteAsync);
    }

    private static Task<PageList<SysDataScopeItemDto>> FilterAsync(
        SysDataScopeFilterDto filter,
        SysDataScopeManager manager,
        CancellationToken cancellationToken) => manager.FilterAsync(filter);

    private static async Task<IResult> AddAsync(
        SysDataScopeAddDto dto,
        SysDataScopeManager manager,
        CancellationToken cancellationToken)
    {
        var entity = await manager.AddAsync(dto);
        return TypedResults.Created($"/api/SysDataScope/{entity.Id}", entity);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SysDataScopeUpdateDto dto,
        SysDataScopeManager manager,
        CancellationToken cancellationToken) => TypedResults.Ok(await manager.UpdateAsync(id, dto));

    private static async Task<IResult> GetDetailAsync(
        Guid id,
        SysDataScopeManager manager,
        CancellationToken cancellationToken)
    {
        var result = await manager.GetAsync(id);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        SysDataScopeManager manager,
        CancellationToken cancellationToken)
    {
        await manager.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}
