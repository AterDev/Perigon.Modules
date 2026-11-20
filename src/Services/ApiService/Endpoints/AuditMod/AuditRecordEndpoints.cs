using ApiService.Composition;
using AuditMod.Managers;
using AuditMod.Models.AuditRecordDtos;
using NotificationMod.Managers;
using Perigon.AspNetCore.Models;
using Share;
using Share.Implement;
using SystemMod.Managers;

namespace ApiService.Endpoints;

public sealed class AuditRecordEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/audit-records", "AuditRecord", WebConst.User);
        group.MapPost("", SubmitAsync);
        group.MapGet("/mine", MineAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPost("/{id:guid}/withdraw", WithdrawAsync);
    }

    public static async Task<RestEndpointBase.ApiResult<AuditRecordDetailDto>> SubmitAsync(
        AuditRecordSubmitDto dto,
        AuditSubmissionManager submission,
        AuditRecordManager manager,
        SysUserManager users,
        NotificationManager notifications,
        CancellationToken cancellationToken)
    {
        var auditorIds = await submission.GetAuditorIdsAsync(dto.SceneCode, cancellationToken);
        var auditorNames = await users.GetDisplayNamesAsync(auditorIds.ToArray(), cancellationToken);
        var record = await submission.SubmitAsync(
            dto.SceneCode,
            dto.BizId,
            dto.ApplyReason,
            dto.FormSnapshotJson,
            auditorNames,
            (context, message, ct) => notifications.PublishInTransactionAsync(
                context,
                message.ToNotificationDto(),
                ct),
            cancellationToken);
        var detail = await manager.GetAsync(record.Id, cancellationToken);
        return detail is null
            ? NotFound(Localizer.NotFoundResource)
            : TypedResults.Created($"/api/audit-records/{record.Id}", detail);
    }

    public static Task<PageList<AuditRecordItemDto>> MineAsync(
        [AsParameters] AuditRecordFilterDto filter,
        AuditRecordManager manager,
        CancellationToken cancellationToken) => manager.FilterMineAsync(filter, cancellationToken);

    public static async Task<RestEndpointBase.ApiResult<AuditRecordDetailDto>> GetAsync(
        Guid id,
        AuditRecordManager manager,
        CancellationToken cancellationToken)
    {
        var result = await manager.GetAsync(id, cancellationToken);
        return result is null ? NotFound(Localizer.NotFoundResource) : result;
    }

    public static Task<AuditRecordDetailDto> WithdrawAsync(
        Guid id,
        AuditRecordWithdrawDto dto,
        IUserContext user,
        AuditRecordManager manager,
        CancellationToken cancellationToken) => manager.WithdrawAsync(id, user.UserId, dto, cancellationToken);
}
