using Perigon.AspNetCore.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Entity.SystemMod;
using SystemMod.Managers;
using SystemMod.Models.SysConfigDtos;

namespace ApiService.Endpoints.SystemMod;

public sealed class SysConfigEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/SysConfig", "SysConfig", WebConst.User);
        group.MapPost("/filter", FilterAsync);
        group.MapPost("", AddAsync);
        group.MapPatch("/{id}", UpdateAsync);
        group.MapGet("/{id}", GetDetailAsync);
        group.MapDelete("/{id}", DeleteAsync);
    }

    private static Task<PageList<SysConfigItemDto>> FilterAsync(
        SysConfigFilterDto filter,
        SysConfigManager manager,
        CancellationToken cancellationToken) => manager.FilterAsync(filter);

    private static async Task<Created<SysConfig>> AddAsync(
        SysConfigAddDto dto,
        SysConfigManager manager,
        CancellationToken cancellationToken)
    {
        var entity = await manager.AddAsync(dto);
        return TypedResults.Created($"/api/SysConfig/{entity.Id}", entity);
    }

    private static async Task<Ok<bool>> UpdateAsync(
        Guid id,
        SysConfigUpdateDto dto,
        SysConfigManager manager,
        CancellationToken cancellationToken)
    {
        await manager.EditAsync(id, dto);
        return TypedResults.Ok(true);
    }

    private static async Task<Results<NotFound, Ok<SysConfigDetailDto>>> GetDetailAsync(
        Guid id,
        SysConfigManager manager,
        CancellationToken cancellationToken)
    {
        var result = await manager.GetAsync(id);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<Results<NotFound, ProblemHttpResult, Ok<bool>>> DeleteAsync(
        Guid id,
        SysConfigManager manager,
        CancellationToken cancellationToken)
    {
        var entity = await manager.GetOwnedAsync(id);
        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        if (entity.IsSystem)
        {
            return TypedResults.Problem("系统配置，无法删除!");
        }

        return TypedResults.Ok((await manager.DeleteAsync(id)) > 0);
    }
}
