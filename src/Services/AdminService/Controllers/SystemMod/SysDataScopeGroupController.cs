using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 数据权限组管理接口。
/// </summary>
[Authorize(WebConst.SuperAdmin)]
public class SysDataScopeGroupController(
    Localizer localizer,
    IUserContext user,
    ILogger<SysDataScopeGroupController> logger,
    SysDataScopeGroupManager manager
) : RestControllerBase<SysDataScopeGroupManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SysDataScopeGroupItemDto>>> FilterAsync(
        SysDataScopeGroupFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<SysDataScopeGroup>> AddAsync(SysDataScopeGroupAddDto dto)
    {
        SysDataScopeGroup entity = await _manager.AddAsync(dto);
        return Created($"/api/SysDataScopeGroup/{entity.Id}", entity);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<SysDataScopeGroup>> UpdateAsync(
        [FromRoute] Guid id,
        SysDataScopeGroupUpdateDto dto)
    {
        return Ok(await _manager.UpdateAsync(id, dto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SysDataScopeGroupDetailDto>> GetDetailAsync([FromRoute] Guid id)
    {
        SysDataScopeGroupDetailDto? result = await _manager.GetAsync(id);
        return result is null ? NotFound() : result;
    }

    [HttpGet("{id}/users")]
    public async Task<ActionResult<List<Guid>>> GetUserIdsAsync([FromRoute] Guid id)
    {
        return await _manager.GetUserIdsAsync(id);
    }

    [HttpPut("{id}/users")]
    public async Task<ActionResult> SetUsersAsync(
        [FromRoute] Guid id,
        SysUserDataScopeGroupSetUsersDto dto)
    {
        await _manager.SetUsersAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _manager.DeleteAsync(id);
        return NoContent();
    }
}
