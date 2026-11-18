using ResourceMod.Managers;
using ResourceMod.Models.UserResourceDtos;
using Perigon.AspNetCore.Models;
using Share.Implement;

namespace ApiService.Endpoints.ResourceMod;

/// <summary>用户资源提交与审核 Minimal API endpoints.</summary>
public sealed class UserResourceEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/UserResource", "UserResource", WebConst.User);
        group.MapGet("mine", MineAsync);
        group.MapGet("review", ReviewAsync).RequireAuthorization(WebConst.AdminUser);
        group.MapGet("{id:guid}", DetailAsync);
        group.MapPost("", AddAsync);
        group.MapPatch("{id:guid}", UpdateAsync);
        group.MapDelete("{id:guid}", DeleteAsync);
        group.MapPost("{id:guid}/approve", ApproveAsync).RequireAuthorization(WebConst.AdminUser);
        group.MapPost("{id:guid}/reject", RejectAsync).RequireAuthorization(WebConst.AdminUser);
    }

    public static Task<PageList<UserResourceItemDto>> MineAsync(
        [AsParameters] UserResourceFilterDto filter,
        UserResourceManager manager) => manager.MineAsync(filter);

    public static Task<PageList<UserResourceItemDto>> ReviewAsync(
        [AsParameters] UserResourceFilterDto filter,
        UserResourceManager manager) => manager.ReviewListAsync(filter);

    public static Task<UserResourceDetailDto?> DetailAsync(Guid id, UserResourceManager manager) =>
        manager.GetAsync(id);

    public static async Task<ApiResult<UserResourceCreatedDto>> AddAsync(
        UserResourceAddDto input,
        UserResourceManager manager)
    {
        var resource = await manager.AddAsync(input);
        return TypedResults.Created(
            $"/api/UserResource/{resource.Id}",
            new UserResourceCreatedDto { Id = resource.Id });
    }

    public static Task<bool> UpdateAsync(
        Guid id,
        UserResourceUpdateDto input,
        UserResourceManager manager) => manager.UpdateAsync(id, input);

    public static Task<bool> DeleteAsync(Guid id, UserResourceManager manager) =>
        manager.DeleteAsync(id);

    public static Task<bool> ApproveAsync(
        Guid id,
        UserResourceReviewDto input,
        UserResourceManager manager) => manager.ApproveAsync(id, input);

    public static Task<bool> RejectAsync(
        Guid id,
        UserResourceRejectDto input,
        UserResourceManager manager) => manager.RejectAsync(id, input);
}
