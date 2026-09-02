using Perigon.AspNetCore.Models;
using ResourceMod.Managers;
using ResourceMod.Models.UserResourceDtos;

namespace AdminService.Controllers.ResourceMod;

/// <summary>
/// 用户资源提交和公开申请审核。
/// </summary>
/// <see cref="UserResourceManager"/>
public class UserResourceController(
    Localizer localizer,
    IUserContext user,
    ILogger<UserResourceManager> logger,
    UserResourceManager manager
) : RestControllerBase<UserResourceManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 查询当前登录用户的用户资源。
    /// </summary>
    [HttpGet("mine")]
    public Task<PageList<UserResourceItemDto>> MineAsync(
        [FromQuery] UserResourceFilterDto filter)
    {
        return _manager.MineAsync(filter);
    }

    /// <summary>
    /// 查询待审核的公开申请。
    /// </summary>
    [Authorize(Policy = WebConst.AdminUser)]
    [HttpGet("review")]
    public Task<PageList<UserResourceItemDto>> ReviewAsync(
        [FromQuery] UserResourceFilterDto filter)
    {
        return _manager.ReviewListAsync(filter);
    }

    /// <summary>
    /// 获取用户资源详情。
    /// </summary>
    [HttpGet("{id:guid}")]
    public Task<UserResourceDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return _manager.GetAsync(id);
    }

    /// <summary>
    /// 新增用户资源或提交公开申请。
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserResourceCreatedDto>> AddAsync(UserResourceAddDto input)
    {
        UserResource resource = await _manager.AddAsync(input);
        return Created(
            $"/api/UserResource/{resource.Id}",
            new UserResourceCreatedDto { Id = resource.Id });
    }

    /// <summary>
    /// 更新用户资源或重新提交公开申请。
    /// </summary>
    [HttpPatch("{id:guid}")]
    public Task<bool> UpdateAsync(
        [FromRoute] Guid id,
        UserResourceUpdateDto input)
    {
        return _manager.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除用户资源。
    /// </summary>
    [HttpDelete("{id:guid}")]
    public Task<bool> DeleteAsync([FromRoute] Guid id)
    {
        return _manager.DeleteAsync(id);
    }

    /// <summary>
    /// 审核通过公开申请并创建常规资源。
    /// </summary>
    [Authorize(Policy = WebConst.AdminUser)]
    [HttpPost("{id:guid}/approve")]
    public Task<bool> ApproveAsync(
        [FromRoute] Guid id,
        UserResourceReviewDto input)
    {
        return _manager.ApproveAsync(id, input);
    }

    /// <summary>
    /// 驳回公开申请。
    /// </summary>
    [Authorize(Policy = WebConst.AdminUser)]
    [HttpPost("{id:guid}/reject")]
    public Task<bool> RejectAsync(
        [FromRoute] Guid id,
        UserResourceRejectDto input)
    {
        return _manager.RejectAsync(id, input);
    }
}
