using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Entity;
using NotificationMod.Models.WebhookDtos;
using Perigon.PostgreSQL;

namespace NotificationMod.Services;

/// <summary>Authenticates, deduplicates, matches and dispatches an anonymous tenant webhook.</summary>
public sealed class NotificationWebhookDispatcher(
    AppDbFactory dbContextFactory,
    ITenantResolver tenantResolver,
    SourcePayloadParser parser,
    NotificationDeliverySender sender,
    IDataProtectionProvider protectionProvider)
{
    private readonly IDataProtector _protector = protectionProvider.CreateProtector("Perigon.NotificationMod.Secrets.v1");

    public async Task<WebhookResultDto> HandleAsync(
        Guid tenantId,
        Guid sourceId,
        JsonElement payload,
        string? headerEventName,
        string? headerEventKey,
        IHeaderDictionary headers,
        IQueryCollection query,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty || sourceId == Guid.Empty) throw new BusinessException("Webhook路由无效", StatusCodes.Status404NotFound);
        Tenant? tenant = tenantResolver.GetById(tenantId);
        if (tenant is null || tenant.Disabled) throw new BusinessException("租户不存在或已禁用", StatusCodes.Status404NotFound);

        await using DefaultDbContext db = dbContextFactory.CreateDbContext(tenantId);
        NotificationSource source = await db.Set<NotificationSource>().Where(x => x.Id == sourceId && x.Enabled).FirstOrDefaultAsync(cancellationToken)
            ?? throw new BusinessException("来源不存在或已禁用", StatusCodes.Status404NotFound);
        string? supplied = source.AuthType.Equals("header", StringComparison.OrdinalIgnoreCase)
            ? headers[source.HeaderKey].FirstOrDefault()
            : query["secretToken"].FirstOrDefault() ?? query["token"].FirstOrDefault() ?? query["secret"].FirstOrDefault() ?? query["key"].FirstOrDefault() ?? headers[source.HeaderKey].FirstOrDefault();
        if (!VerifySecret(source.ProtectedSecret, supplied)) throw new BusinessException("Webhook鉴权失败", StatusCodes.Status401Unauthorized);

        NormalizedEventDto normalized = parser.Parse(payload, source.Type, source.Id, headerEventName, headerEventKey);
        DispatchBatch batch;
        try
        {
            batch = await db.TransactionAsync(_ => CreateBatchAsync(db, source, normalized, tenantId, cancellationToken), cancellationToken);
        }
        catch (Exception ex) when (IsDuplicateEventConflict(ex))
        {
            NotificationEvent duplicate = await db.Set<NotificationEvent>().Where(x => x.SourceId == source.Id && x.EventKey == normalized.EventKey).FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Webhook event conflict could not be resolved.", ex);
            batch = new DispatchBatch(duplicate, [], [], true);
        }
        if (batch.Duplicate || batch.Deliveries.Count == 0)
            return new WebhookResultDto { EventId = batch.Event.Id, Duplicate = batch.Duplicate, DeliveryCount = batch.Deliveries.Count };

        var outcomes = await SendBatchAsync(batch, cancellationToken);
        foreach (var outcome in outcomes)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string status = outcome.Success ? "succeeded" : outcome.Error?.Contains("unknown", StringComparison.OrdinalIgnoreCase) == true ? "unknown" : "failed";
            await db.Set<NotificationDelivery>().Where(x => x.Id == outcome.Delivery.Id && x.Status == "pending").ExecuteUpdateAsync(set => set
                .Set(x => x.Status, status)
                .Set(x => x.Error, outcome.Error)
                .Set(x => x.SentAt, outcome.Success ? now : (DateTimeOffset?)null), cancellationToken: CancellationToken.None);
        }
        bool hasUnknown = outcomes.Any(x => !x.Success && x.Error?.Contains("unknown", StringComparison.OrdinalIgnoreCase) == true);
        string finalStatus = hasUnknown ? "unknown" : outcomes.All(x => x.Success) ? "completed" : outcomes.Any(x => x.Success) ? "partial" : "failed";
        await db.Set<NotificationEvent>().Where(x => x.Id == batch.Event.Id && x.ProcessingStatus == "queued")
            .ExecuteUpdateAsync(set => set.Set(x => x.ProcessingStatus, finalStatus).Set(x => x.FinishedAt, DateTimeOffset.UtcNow), cancellationToken: CancellationToken.None);
        return new WebhookResultDto { EventId = batch.Event.Id, DeliveryCount = batch.Deliveries.Count, FailedCount = outcomes.Count(x => !x.Success) };
    }

    private static async Task<DispatchBatch> CreateBatchAsync(DefaultDbContext db, NotificationSource source, NormalizedEventDto normalized, Guid tenantId, CancellationToken cancellationToken)
    {
        NotificationEvent? existing = await db.Set<NotificationEvent>().Where(x => x.SourceId == source.Id && x.EventKey == normalized.EventKey).FirstOrDefaultAsync(cancellationToken);
        if (existing is not null) return new DispatchBatch(existing, [], [], true);

        NotificationEvent notificationEvent = new()
        {
            TenantId = tenantId, SourceId = source.Id, EventKey = normalized.EventKey, EventType = normalized.EventType,
            ProjectId = normalized.ProjectId, ObjectId = normalized.ObjectId, ProjectName = normalized.ProjectName,
            Namespace = normalized.Namespace, ActorName = normalized.ActorName, Title = normalized.Title, Content = normalized.Content,
            Status = normalized.Status, Action = normalized.Action, Ref = normalized.Ref, Tag = normalized.Tag, Sha = normalized.Sha,
            OccurredAt = normalized.OccurredAt, FinishedAt = normalized.FinishedAt, Duration = normalized.Duration, Url = normalized.Url, ProcessingStatus = "processing",
        };
        await db.Set<NotificationEvent>().InsertAsync(notificationEvent, returning: false, cancellationToken: cancellationToken);
        List<NotificationRule> rules = await db.Set<NotificationRule>().Where(x => x.SourceId == source.Id && x.Enabled).ToListAsync(cancellationToken);
        rules = rules.Where(x => (x.EventTypes.Length == 0 || x.EventTypes.Contains(normalized.EventType)) &&
            (normalized.EventType is not ("pipeline" or "merge_request") || x.Statuses.Length == 0 || x.Statuses.Contains(normalized.Status)) &&
            (x.ProjectIds.Length == 0 || x.ProjectIds.Contains(normalized.ProjectId))).ToList();
        Guid[] ruleIds = rules.Select(x => x.Id).ToArray();
        List<NotificationRuleReceiver> links = ruleIds.Length == 0 ? [] : await db.Set<NotificationRuleReceiver>().Where(x => ruleIds.Contains(x.RuleId)).ToListAsync(cancellationToken);
        Guid[] receiverIds = links.Select(x => x.ReceiverId).Distinct().ToArray();
        List<NotificationReceiver> receivers = receiverIds.Length == 0 ? [] : await db.Set<NotificationReceiver>().Where(x => receiverIds.Contains(x.Id) && x.Enabled).ToListAsync(cancellationToken);
        List<NotificationDelivery> deliveries = receivers.Select(receiver => new NotificationDelivery
        {
            TenantId = tenantId, EventId = notificationEvent.Id, ReceiverId = receiver.Id, ReceiverName = receiver.Name, Status = "pending",
        }).ToList();
        notificationEvent.ProcessingStatus = deliveries.Count == 0 ? "completed" : "queued";
        if (deliveries.Count > 0) await db.Set<NotificationDelivery>().BulkInsertAsync(deliveries, cancellationToken: cancellationToken);
        await db.Set<NotificationEvent>().Where(x => x.Id == notificationEvent.Id)
            .ExecuteUpdateAsync(set => set.Set(x => x.ProcessingStatus, notificationEvent.ProcessingStatus), cancellationToken: cancellationToken);
        return new DispatchBatch(notificationEvent, deliveries, receivers, false);
    }

    private async Task<List<DeliveryOutcome>> SendBatchAsync(DispatchBatch batch, CancellationToken cancellationToken)
    {
        using SemaphoreSlim gate = new(4, 4);
        var tasks = batch.Deliveries.Select(async delivery =>
        {
            await gate.WaitAsync(CancellationToken.None);
            try
            {
                NotificationReceiver receiver = batch.Receivers.First(x => x.Id == delivery.ReceiverId);
                var result = await sender.SendAsync(receiver, batch.Event, cancellationToken);
                return new DeliveryOutcome(delivery, result.Success, result.Error);
            }
            finally { gate.Release(); }
        });
        return (await Task.WhenAll(tasks)).ToList();
    }

    private bool VerifySecret(string protectedSecret, string? supplied)
    {
        if (string.IsNullOrWhiteSpace(supplied)) return false;
        try
        {
            string expected = _protector.Unprotect(protectedSecret);
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(supplied));
        }
        catch { return false; }
    }

    private static bool IsDuplicateEventConflict(Exception exception)
    {
        string message = exception.ToString();
        return message.Contains("NotificationEvent", StringComparison.OrdinalIgnoreCase) &&
            (message.Contains("unique", StringComparison.OrdinalIgnoreCase) || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record DispatchBatch(NotificationEvent Event, List<NotificationDelivery> Deliveries, List<NotificationReceiver> Receivers, bool Duplicate);
    private sealed record DeliveryOutcome(NotificationDelivery Delivery, bool Success, string? Error);
}
