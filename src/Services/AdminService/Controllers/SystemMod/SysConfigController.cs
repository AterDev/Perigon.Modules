using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 系统配置
/// </summary>
/// <see cref="SysConfigManager"/>
public class SysConfigController(
    Localizer localizer,
    IUserContext user,
    ILogger<SysConfigController> logger,
    SysConfigManager manager
) : RestControllerBase<SysConfigManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 获取配置列表 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SysConfigItemDto>>> FilterAsync(
        SysConfigFilterDto filter
    )
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 获取枚举信息 ✅
    /// </summary>
    /// <returns></returns>
    [HttpGet("enum")]
    public async Task<ActionResult<Dictionary<string, List<EnumDictionary>>>> GetEnumConfigsAsync()
    {
        return await _manager.GetEnumConfigsAsync();
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<SysConfig>> AddAsync(SysConfigAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return Created($"/api/SysConfig/{entity.Id}", entity);
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool>> UpdateAsync(
        [FromRoute] Guid id,
        SysConfigUpdateDto dto
    )
    {
        // Use manager to perform edit which includes permission check
        await _manager.EditAsync(id, dto);
        return true;
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SysConfigDetailDto?>> GetDetailAsync([FromRoute] Guid id)
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
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        var entity = await _manager.GetOwnedAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        if (entity.IsSystem)
        {
            return Problem("系统配置，无法删除!");
        }

        var deleted = await _manager.DeleteAsync(id);
        return deleted > 0;
    }
}
