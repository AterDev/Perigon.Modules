using ApiService.Composition;
using AuditMod.Managers;
using AuditMod.Models.AuditTaskDtos;
using NotificationMod.Managers;
using Perigon.AspNetCore.Models;
using Share;
using Share.Implement;

namespace ApiService.Endpoints;

public sealed class AuditTaskEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/audit-tasks", "AuditTask", WebConst.User);
        group.MapGet("/mine", MineAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPost("/{id:guid}/approve", ApproveAsync);
        group.MapPost("/{id:guid}/reject", RejectAsync);
    }

    public static Task<PageList<AuditTaskItemDto>> MineAsync(
        [AsParameters] AuditTaskFilterDto filter,
        IUserContext user,
        AuditTaskManager manager,
        CancellationToken cancellationToken) => manager.FilterMineAsync(user.UserId, filter, cancellationToken);

    public static async Task<RestEndpointBase.ApiResult<AuditTaskDetailDto>> GetAsync(
        Guid id,
        IUserContext user,
        AuditTaskManager manager,
        CancellationToken cancellationToken)
    {
        var result = await manager.GetAsync(id, user.UserId, cancellationToken);
        return result is null ? NotFound(Localizer.NotFoundResource) : result;
    }

    public static Task<AuditTaskDetailDto> ApproveAsync(
        Guid id,
        AuditDecisionDto dto,
        IUserContext user,
        AuditTaskManager manager,
        NotificationManager notifications,
        CancellationToken cancellationToken) => manager.ApproveAsync(
            id, user.UserId, dto,
            (context, message, ct) => notifications.PublishInTransactionAsync(
                context,
                message.ToNotificationDto(),
                ct),
            cancellationToken);

    public static Task<AuditTaskDetailDto> RejectAsync(
        Guid id,
        AuditDecisionDto dto,
        IUserContext user,
        AuditTaskManager manager,
        NotificationManager notifications,
        CancellationToken cancellationToken) => manager.RejectAsync(
            id, user.UserId, dto,
            (context, message, ct) => notifications.PublishInTransactionAsync(
                context,
                message.ToNotificationDto(),
                ct),
            cancellationToken);
}
