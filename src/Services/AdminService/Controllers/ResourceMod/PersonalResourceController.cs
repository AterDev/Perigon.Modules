using ResourceMod.Managers;
using ResourceMod.Models.PersonalResourceDtos;
using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.ResourceMod;

/// <summary>
/// 个人资源提交和公开申请审核。
/// </summary>
/// <see cref="PersonalResourceManager"/>
public class PersonalResourceController(
    Localizer localizer,
    IUserContext user,
    ILogger<PersonalResourceManager> logger,
    PersonalResourceManager manager
) : RestControllerBase<PersonalResourceManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 查询当前登录用户的个人资源。
    /// </summary>
    [HttpGet("mine")]
    public Task<PageList<PersonalResourceItemDto>> MineAsync(
        [FromQuery] PersonalResourceFilterDto filter)
    {
        return _manager.MineAsync(filter);
    }

    /// <summary>
    /// 查询待审核的公开申请。
    /// </summary>
    [HttpGet("review")]
    public Task<PageList<PersonalResourceItemDto>> ReviewAsync(
        [FromQuery] PersonalResourceFilterDto filter)
    {
        return _manager.ReviewListAsync(filter);
    }

    /// <summary>
    /// 获取个人资源详情。
    /// </summary>
    [HttpGet("{id:guid}")]
    public Task<PersonalResourceDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return _manager.GetAsync(id);
    }

    /// <summary>
    /// 新增个人资源或提交公开申请。
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PersonalResourceCreatedDto>> AddAsync(PersonalResourceAddDto input)
    {
        PersonalResource resource = await _manager.AddAsync(input);
        return Created(
            $"/api/PersonalResource/{resource.Id}",
            new PersonalResourceCreatedDto { Id = resource.Id });
    }

    /// <summary>
    /// 更新个人资源或重新提交公开申请。
    /// </summary>
    [HttpPatch("{id:guid}")]
    public Task<bool> UpdateAsync(
        [FromRoute] Guid id,
        PersonalResourceUpdateDto input)
    {
        return _manager.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除个人资源。
    /// </summary>
    [HttpDelete("{id:guid}")]
    public Task<bool> DeleteAsync([FromRoute] Guid id)
    {
        return _manager.DeleteAsync(id);
    }

    /// <summary>
    /// 审核通过公开申请并创建常规资源。
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public Task<bool> ApproveAsync(
        [FromRoute] Guid id,
        PersonalResourceReviewDto input)
    {
        return _manager.ApproveAsync(id, input);
    }

    /// <summary>
    /// 驳回公开申请。
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    public Task<bool> RejectAsync(
        [FromRoute] Guid id,
        PersonalResourceRejectDto input)
    {
        return _manager.RejectAsync(id, input);
    }
}
