namespace NotificationMod.Services;

/// <summary>Manual timeout recovery. The caller must choose a threshold longer than the maximum delivery duration; no automatic lease ownership is claimed.</summary>
public sealed class NotificationRecoveryService(AppDbFactory dbContextFactory)
{
    public async Task<(int Deliveries, int Events)> RecoverAsync(TimeSpan lease, CancellationToken cancellationToken = default)
    {
        if (lease <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lease));
        }
        await using DefaultDbContext db = dbContextFactory.CreateDbContext();
        DateTimeOffset threshold = DateTimeOffset.UtcNow.Subtract(lease);
        int deliveries = await db.Set<NotificationDelivery>().Where(x => x.Status == "pending" && x.CreatedTime < threshold)
            .ExecuteUpdateAsync(set => set.Set(x => x.Status, "unknown").Set(x => x.Error, "处理租约已过期，投递结果未知"), cancellationToken: cancellationToken);
        int events = await db.Set<NotificationEvent>().Where(x => (x.ProcessingStatus == "processing" || x.ProcessingStatus == "queued") && x.CreatedTime < threshold)
            .ExecuteUpdateAsync(set => set.Set(x => x.ProcessingStatus, "failed").Set(x => x.FinishedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);
        return (deliveries, events);
    }
}
