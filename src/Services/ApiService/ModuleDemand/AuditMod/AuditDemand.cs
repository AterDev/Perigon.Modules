using AuditMod;
using AuditMod.Demands;
using EntityFramework.AppDbContext;

namespace ApiService.ModuleDemand.AuditMod;

public sealed class AuditDemand(
    AuditUserNamesResolver? userNames = null,
    AuditUserIdsResolver? existingUserIds = null,
    AuditNotificationPublisher? notificationPublisher = null)
    : IAuditNotificationDemand, IAuditUserDemand
{
    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        // TODO: 接入宿主用户目录。
        return await (userNames ?? throw new InvalidOperationException("Configure AuditDemand user names."))
            (userIds.ToArray(), cancellationToken);
    }

    public async Task<IReadOnlySet<Guid>> GetExistingUserIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        // TODO: 接入宿主用户目录。
        var existing = await (existingUserIds ?? throw new InvalidOperationException("Configure AuditDemand user IDs."))
            (userIds.ToArray(), cancellationToken);
        return existing.ToHashSet();
    }

    public Task PublishAsync(
        DefaultDbContext context,
        AuditNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        // TODO: 接入宿主通知模块。
        return notificationPublisher is null
            ? Task.CompletedTask
            : notificationPublisher(context, message, cancellationToken);
    }
}
