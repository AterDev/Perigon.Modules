using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.SystemMod;

/// <summary>
/// 系统日志
/// </summary>
/// <see cref="SysLogsManager"/>
public class SysLogsController(
    Localizer localizer,
    IUserContext user,
    ILogger<SysLogsController> logger,
    SysLogsManager manager
) : RestControllerBase<SysLogsManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SysLogsItemDto>>> FilterAsync(
        SysLogsFilterDto filter
    )
    {
        return await _manager.ToPageAsync(filter);
    }
}
