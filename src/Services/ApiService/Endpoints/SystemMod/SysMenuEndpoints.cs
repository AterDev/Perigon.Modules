using Entity.SystemMod;
using Perigon.AspNetCore.Models;
using Share;
using SystemMod.Managers;
using SystemMod.Models.SysMenuDtos;

namespace ApiService.Endpoints.SystemMod;

public sealed class SysMenuEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/SysMenu", "SysMenu", WebConst.User)
            .RequireAuthorization(WebConst.SuperAdmin);
        group.MapPost("/filter", FilterAsync);
        group.MapPost("/sync/{token}", SyncSysMenusAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();
        group.MapPost("", AddAsync);
        group.MapPatch("/{id}", UpdateAsync);
        group.MapGet("/{id}", GetDetailAsync);
    }

    private static Task<PageList<SysMenu>> FilterAsync(
        SysMenuFilterDto filter,
        SysMenuManager manager,
        CancellationToken cancellationToken) => manager.FilterAsync(filter);

    private static async Task<IResult> SyncSysMenusAsync(
        string token,
        List<SysMenuSyncDto> menus,
        SysMenuManager manager,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        if (environment.IsProduction() || token != "MyProjectNameDefaultKey")
        {
            return TypedResults.Forbid();
        }

        return menus.Count == 0
            ? TypedResults.Ok(false)
            : TypedResults.Ok(await manager.SyncSysMenusAsync(menus));
    }

    private static async Task<IResult> AddAsync(
        SysMenuAddDto dto,
        SysMenuManager manager,
        Localizer localizer,
        CancellationToken cancellationToken)
    {
        if (dto.ParentId is not null && !await manager.ExistAsync(dto.ParentId.Value))
        {
            return TypedResults.NotFound(localizer.Get(Localizer.NotFoundResource));
        }

        var entity = await manager.AddAsync(dto);
        return TypedResults.Created($"/api/SysMenu/{entity.Id}", entity);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SysMenuUpdateDto dto,
        SysMenuManager manager,
        Localizer localizer,
        CancellationToken cancellationToken)
    {
        if (await manager.FindAsync(id) is null)
        {
            return TypedResults.NotFound(localizer.Get(Localizer.NotFoundResource));
        }

        await manager.EditAsync(id, dto);
        return TypedResults.Ok(true);
    }

    private static async Task<IResult> GetDetailAsync(
        Guid id,
        SysMenuManager manager,
        CancellationToken cancellationToken)
    {
        var entity = await manager.FindAsync(id);
        return entity is null ? TypedResults.NotFound() : TypedResults.Ok(entity);
    }
}
