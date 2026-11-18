using Perigon.AspNetCore.Models;
using SystemMod.Managers;
using SystemMod.Models.SysLogsDtos;

namespace ApiService.Endpoints.SystemMod;

public sealed class SysLogsEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/SysLogs", "SysLogs", WebConst.User);
        group.MapPost("/filter", FilterAsync);
    }

    private static Task<PageList<SysLogsItemDto>> FilterAsync(
        SysLogsFilterDto filter,
        SysLogsManager manager,
        CancellationToken cancellationToken) => manager.ToPageAsync(filter);
}
