namespace NotificationMod.Demands;

public delegate Task<List<Guid>> NotificationUserIdsResolver(
    Guid[] userIds,
    CancellationToken cancellationToken);
