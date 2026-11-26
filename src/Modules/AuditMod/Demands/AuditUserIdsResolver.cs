namespace AuditMod.Demands;

public delegate Task<List<Guid>> AuditUserIdsResolver(
    Guid[] userIds,
    CancellationToken cancellationToken);
