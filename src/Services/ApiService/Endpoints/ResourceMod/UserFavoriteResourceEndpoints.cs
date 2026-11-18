using ResourceMod.Managers;
using ResourceMod.Models.UserFavoriteResourceDtos;
using Perigon.AspNetCore.Models;
using Share.Implement;

namespace ApiService.Endpoints.ResourceMod;

/// <summary>用户收藏资源 Minimal API endpoints.</summary>
public sealed class UserFavoriteResourceEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/UserFavoriteResource", "UserFavoriteResource", WebConst.User);
        group.MapGet("mine", MineAsync);
        group.MapGet("{resourceId:guid}", DetailAsync);
        group.MapPost("", AddAsync);
        group.MapDelete("{resourceId:guid}", RemoveAsync);
    }

    public static Task<PageList<UserFavoriteResourceItemDto>> MineAsync(
        [AsParameters] UserFavoriteResourceFilterDto filter,
        UserFavoriteResourceManager manager) => manager.MineAsync(filter);

    public static Task<UserFavoriteResourceDetailDto> DetailAsync(
        Guid resourceId,
        UserFavoriteResourceManager manager) => manager.GetAsync(resourceId);

    public static async Task<ApiResult<UserFavoriteResourceCreatedDto>> AddAsync(
        UserFavoriteResourceAddDto input,
        UserFavoriteResourceManager manager)
    {
        var favorite = await manager.AddAsync(input);
        return TypedResults.Created(
            $"/api/UserFavoriteResource/{favorite.ResourceId}",
            new UserFavoriteResourceCreatedDto { Id = favorite.Id, ResourceId = favorite.ResourceId });
    }

    public static Task<bool> RemoveAsync(Guid resourceId, UserFavoriteResourceManager manager) =>
        manager.RemoveAsync(resourceId);
}
