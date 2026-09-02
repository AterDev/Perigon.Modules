using Perigon.AspNetCore.Models;
using ResourceMod.Models.UserFavoriteResourceDtos;

namespace AdminService.Controllers.ResourceMod;

/// <summary>
/// 用户收藏资源接口。
/// </summary>
public class UserFavoriteResourceController(
    Localizer localizer,
    IUserContext user,
    ILogger<UserFavoriteResourceController> logger,
    UserFavoriteResourceManager manager
) : RestControllerBase<UserFavoriteResourceManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 查询当前用户的收藏资源。
    /// </summary>
    [HttpGet("mine")]
    public Task<PageList<UserFavoriteResourceItemDto>> MineAsync(
        [FromQuery] UserFavoriteResourceFilterDto filter)
    {
        return _manager.MineAsync(filter);
    }

    /// <summary>
    /// 查询当前用户对指定资源的收藏详情。
    /// </summary>
    [HttpGet("{resourceId:guid}")]
    public Task<UserFavoriteResourceDetailDto> DetailAsync([FromRoute] Guid resourceId)
    {
        return _manager.GetAsync(resourceId);
    }

    /// <summary>
    /// 收藏一个当前用户可见的常规资源。
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserFavoriteResourceCreatedDto>> AddAsync(
        UserFavoriteResourceAddDto input)
    {
        UserFavoriteResource favorite = await _manager.AddAsync(input);
        return Created(
            $"/api/UserFavoriteResource/{favorite.ResourceId}",
            new UserFavoriteResourceCreatedDto
            {
                Id = favorite.Id,
                ResourceId = favorite.ResourceId
            });
    }

    /// <summary>
    /// 取消当前用户对指定资源的收藏。
    /// </summary>
    [HttpDelete("{resourceId:guid}")]
    public Task<bool> RemoveAsync([FromRoute] Guid resourceId)
    {
        return _manager.RemoveAsync(resourceId);
    }
}
