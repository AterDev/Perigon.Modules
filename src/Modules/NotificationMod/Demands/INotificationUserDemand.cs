namespace NotificationMod.Demands;

public interface INotificationUserDemand
{
    Task<IReadOnlyCollection<Guid>> GetExistingUserIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default);
}
