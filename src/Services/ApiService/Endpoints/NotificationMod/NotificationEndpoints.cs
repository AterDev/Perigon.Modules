using Microsoft.AspNetCore.Http;
using NotificationMod.Managers;
using NotificationMod.Models.InAppNotificationDtos;
using NotificationMod.Models.NotificationDeliveryDtos;
using NotificationMod.Models.NotificationEventDtos;
using NotificationMod.Models.NotificationReceiverDtos;
using NotificationMod.Models.NotificationRecoveryDtos;
using NotificationMod.Models.NotificationRuleDtos;
using NotificationMod.Models.NotificationSourceDtos;
using NotificationMod.Services;
using NotificationMod.Demands;
using Perigon.AspNetCore.Models;
using Share;
using Share.Implement;

namespace ApiService.Endpoints;

/// <summary>站内消息和业务通知管理端点。</summary>
public sealed class NotificationEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/Notification", "Notification", WebConst.User);
        var admin = group.MapGroup("").RequireAuthorization(WebConst.AdminUser);

        admin.MapPost("/filter", FilterAsync);
        admin.MapGet("/{id:guid}", DetailAsync);
        admin.MapPost("", AddAsync);
        admin.MapPatch("/{id:guid}", UpdateAsync);
        admin.MapPost("/{id:guid}/publish", PublishAsync);
        admin.MapDelete("/{id:guid}", DeleteAsync);

        group.MapPost("/mine", MineAsync);
        group.MapGet("/mine/{id:guid}", MineDetailAsync);
        group.MapGet("/mine/unread-count", UnreadCountAsync);
        group.MapPost("/{id:guid}/read", ReadAsync);

        admin.MapPost("/sources/filter", SourcesAsync);
        admin.MapPost("/sources", AddSourceAsync);
        admin.MapPatch("/sources/{id:guid}", UpdateSourceAsync);
        admin.MapDelete("/sources/{id:guid}", DeleteSourceAsync);
        admin.MapPost("/receivers/filter", ReceiversAsync);
        admin.MapPost("/receivers", AddReceiverAsync);
        admin.MapPatch("/receivers/{id:guid}", UpdateReceiverAsync);
        admin.MapDelete("/receivers/{id:guid}", DeleteReceiverAsync);
        admin.MapPost("/rules/filter", RulesAsync);
        admin.MapPost("/rules", AddRuleAsync);
        admin.MapPatch("/rules/{id:guid}", UpdateRuleAsync);
        admin.MapDelete("/rules/{id:guid}", DeleteRuleAsync);
        admin.MapPost("/events/filter", EventsAsync);
        admin.MapPost("/deliveries/filter", DeliveriesAsync);
        admin.MapPost("/recovery", RecoverAsync);
    }

    public static Task<PageList<InAppNotificationItemDto>> FilterAsync(
        InAppNotificationFilterDto filter,
        NotificationManager manager) => manager.FilterAsync(filter);

    public static Task<InAppNotificationDetailDto?> DetailAsync(
        Guid id,
        NotificationManager manager) => manager.GetAsync(id);

    public static async Task<RestEndpointBase.ApiResult<InAppNotificationDetailDto>> AddAsync(
        InAppNotificationAddDto dto,
        NotificationManager manager,
        INotificationUserDemand users,
        CancellationToken cancellationToken)
    {
        var recipientIds = dto.Publish
            ? await users.GetExistingUserIdsAsync(dto.TargetUserIds, cancellationToken)
            : [];
        var result = await manager.AddAsync(dto, recipientIds);
        return TypedResults.Created($"/api/Notification/{result.Id}", result);
    }

    public static Task<bool> UpdateAsync(Guid id, InAppNotificationUpdateDto dto, NotificationManager manager) =>
        manager.UpdateAsync(id, dto);

    public static async Task<RestEndpointBase.ApiResult<bool>> PublishAsync(
        Guid id,
        NotificationManager manager,
        INotificationUserDemand users,
        CancellationToken cancellationToken)
    {
        var requestedUserIds = await manager.GetUnpublishedTargetIdsAsync(id);
        if (requestedUserIds is not null)
        {
            var recipientIds = await users.GetExistingUserIdsAsync(requestedUserIds, cancellationToken);
            await manager.PublishAsync(id, requestedUserIds, recipientIds);
        }

        return TypedResults.NoContent();
    }

    public static Task<bool> DeleteAsync(Guid id, NotificationManager manager) => manager.DeleteAsync(id);

    public static Task<PageList<InAppNotificationItemDto>> MineAsync(
        InAppNotificationFilterDto filter,
        NotificationManager manager) => manager.MineAsync(filter);

    public static Task<InAppNotificationDetailDto> MineDetailAsync(Guid id, NotificationManager manager) =>
        manager.MineDetailAsync(id);

    public static Task<int> UnreadCountAsync(NotificationManager manager) => manager.UnreadCountAsync();

    public static Task<InAppNotificationReadDto> ReadAsync(Guid id, NotificationManager manager) =>
        manager.ReadAsync(id);

    public static Task<PageList<NotificationSourceItemDto>> SourcesAsync(
        FilterBase filter,
        NotificationManager manager) => manager.SourcesAsync(filter);

    public static Task<NotificationSourceItemDto> AddSourceAsync(
        NotificationSourceAddDto dto,
        NotificationManager manager) => manager.SaveSourceAsync(null, dto);

    public static Task<NotificationSourceItemDto> UpdateSourceAsync(
        Guid id,
        NotificationSourceUpdateDto dto,
        NotificationManager manager) => manager.SaveSourceAsync(id, dto);

    public static Task<bool> DeleteSourceAsync(Guid id, NotificationManager manager) => manager.DeleteSourceAsync(id);

    public static Task<PageList<NotificationReceiverItemDto>> ReceiversAsync(
        FilterBase filter,
        NotificationManager manager) => manager.ReceiversAsync(filter);

    public static Task<NotificationReceiverItemDto> AddReceiverAsync(
        NotificationReceiverAddDto dto,
        NotificationManager manager) => manager.SaveReceiverAsync(null, dto);

    public static Task<NotificationReceiverItemDto> UpdateReceiverAsync(
        Guid id,
        NotificationReceiverUpdateDto dto,
        NotificationManager manager) => manager.SaveReceiverAsync(id, dto);

    public static Task<bool> DeleteReceiverAsync(Guid id, NotificationManager manager) => manager.DeleteReceiverAsync(id);

    public static Task<PageList<NotificationRuleItemDto>> RulesAsync(
        FilterBase filter,
        NotificationManager manager) => manager.RulesAsync(filter);

    public static Task<NotificationRuleItemDto> AddRuleAsync(
        NotificationRuleAddDto dto,
        NotificationManager manager) => manager.SaveRuleAsync(null, dto);

    public static Task<NotificationRuleItemDto> UpdateRuleAsync(
        Guid id,
        NotificationRuleUpdateDto dto,
        NotificationManager manager) => manager.SaveRuleAsync(id, dto);

    public static Task<bool> DeleteRuleAsync(Guid id, NotificationManager manager) => manager.DeleteRuleAsync(id);

    public static Task<PageList<NotificationEventItemDto>> EventsAsync(
        FilterBase filter,
        NotificationManager manager) => manager.EventsAsync(filter);

    public static Task<PageList<NotificationDeliveryItemDto>> DeliveriesAsync(
        FilterBase filter,
        NotificationManager manager) => manager.DeliveriesAsync(filter);

    public static async Task<NotificationRecoveryResultDto> RecoverAsync(
        NotificationRecoveryDto dto,
        NotificationRecoveryService recovery,
        CancellationToken cancellationToken)
    {
        var result = await recovery.RecoverAsync(
            TimeSpan.FromSeconds(dto.LeaseSeconds),
            cancellationToken);
        return new NotificationRecoveryResultDto { Deliveries = result.Deliveries, Events = result.Events };
    }
}
