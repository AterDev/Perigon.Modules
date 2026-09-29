using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 系统菜单
/// </summary>
/// <see cref="SysMenuManager"/>
[Authorize(WebConst.SuperAdmin)]
public class SysMenuController(
    Localizer localizer,
    IUserContext user,
    ILogger<SysMenuController> logger,
    IWebHostEnvironment env,
    SysMenuManager manager
) : RestControllerBase<SysMenuManager>(localizer, manager, user, logger)
{
    private readonly IWebHostEnvironment _env = env;

    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SysMenu>>> FilterAsync(SysMenuFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 菜单同步 ✅
    /// </summary>
    /// <param name="token"></param>
    /// <param name="menus"></param>
    /// <returns></returns>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("sync/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> SyncSysMenus(
        string token,
        List<SysMenuSyncDto> menus
    )
    {
        if (_env.IsProduction())
        {
            return Forbid();
        }
        // 不经过jwt验证，定义自己的key用来开发时同步菜单
        if (token != "MyProjectNameDefaultKey")
        {
            return Forbid();
        }
        if (menus != null && menus.Count != 0)
        {
            return await _manager.SyncSysMenusAsync(menus);
        }
        return false;
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<SysMenu>> AddAsync(SysMenuAddDto dto)
    {
        if (dto.ParentId != null)
        {
            if (!await _manager.ExistAsync(dto.ParentId.Value))
            {
                return NotFound(Localizer.NotFoundResource);
            }
        }

        var entity = await _manager.AddAsync(dto);
        return Created($"/api/SysMenu/{entity.Id}", entity);
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool>> UpdateAsync([FromRoute] Guid id, SysMenuUpdateDto dto)
    {
        SysMenu? current = await _manager.FindAsync(id);
        if (current == null)
        {
            return NotFound(Localizer.NotFoundResource);
        }

        await _manager.EditAsync(id, dto);
        return true;
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SysMenu?>> GetDetailAsync([FromRoute] Guid id)
    {
        var entity = await _manager.FindAsync(id);
        return entity == null ? NotFound() : entity;
    }
}
