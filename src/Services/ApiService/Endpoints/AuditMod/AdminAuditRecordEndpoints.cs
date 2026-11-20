using AuditMod.Managers;
using AuditMod.Models.AuditRecordDtos;
using Perigon.AspNetCore.Models;
using Share.Implement;

namespace ApiService.Endpoints;

public sealed class AdminAuditRecordEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/admin/audit-records", "AuditRecord", WebConst.AdminUser)
            .RequireAuthorization(WebConst.User);
        group.MapGet("", AllAsync);
    }

    public static Task<PageList<AuditRecordItemDto>> AllAsync(
        [AsParameters] AuditRecordFilterDto filter,
        AuditRecordManager manager,
        CancellationToken cancellationToken) => manager.FilterAllAsync(filter, cancellationToken);
}
