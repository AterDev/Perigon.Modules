using NotificationMod.Demands;

namespace ApiService.ModuleDemand.NotificationMod;

public sealed class NotificationDemand(NotificationUserIdsResolver? userIdsResolver = null)
    : INotificationUserDemand
{
    public async Task<IReadOnlyCollection<Guid>> GetExistingUserIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        // TODO: 接入宿主用户目录。空列表用于解析广播受众。
        return await (userIdsResolver ?? throw new InvalidOperationException("Configure NotificationDemand user IDs."))
            (userIds.ToArray(), cancellationToken);
    }
}
