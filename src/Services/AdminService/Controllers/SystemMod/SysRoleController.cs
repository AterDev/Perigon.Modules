using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 系统角色
/// <see cref="SysRoleManager"/>
/// </summary>
[Authorize(WebConst.SuperAdmin)]
public class SysRoleController(
        Localizer localizer,
        IUserContext user,
        ILogger<SysRoleController> logger,
        SysRoleManager manager

) : RestControllerBase<SysRoleManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<PageList<SysRoleItemDto>>> ListAsync(
        [FromQuery] SysRoleFilterDto filter
    )
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<SysRole>> AddAsync(SysRoleAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return Created($"/api/SysRole/{entity.Id}", entity);
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<SysRole>> UpdateAsync(
        [FromRoute] Guid id,
        SysRoleUpdateDto dto
    )
    {
        var entity = await _manager.UpdateAsync(id, dto);
        return Ok(entity);
    }

    /// <summary>
    /// 角色菜单 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("menus")]
    public async Task<ActionResult<SysRole>> UpdateMenusAsync(
        [FromBody] SysRoleSetMenusDto dto
    )
    {
        var result = await _manager.SetMenusAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SysRoleDetailDto?>> DetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.GetAsync(id);
        return res == null ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _manager.DeleteAsync(id);
        return NoContent();
    }
}
