using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 数据权限范围管理接口。
/// </summary>
[Authorize(WebConst.SuperAdmin)]
public class SysDataScopeController(
    Localizer localizer,
    IUserContext user,
    ILogger<SysDataScopeController> logger,
    SysDataScopeManager manager
) : RestControllerBase<SysDataScopeManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SysDataScopeItemDto>>> FilterAsync(
        SysDataScopeFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<SysDataScope>> AddAsync(SysDataScopeAddDto dto)
    {
        SysDataScope entity = await _manager.AddAsync(dto);
        return Created($"/api/SysDataScope/{entity.Id}", entity);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<SysDataScope>> UpdateAsync(
        [FromRoute] Guid id,
        SysDataScopeUpdateDto dto)
    {
        return Ok(await _manager.UpdateAsync(id, dto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SysDataScopeDetailDto>> GetDetailAsync([FromRoute] Guid id)
    {
        SysDataScopeDetailDto? result = await _manager.GetAsync(id);
        return result is null ? NotFound() : result;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _manager.DeleteAsync(id);
        return NoContent();
    }
}
