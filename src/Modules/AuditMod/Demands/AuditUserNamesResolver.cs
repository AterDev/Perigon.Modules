namespace AuditMod.Demands;

public delegate Task<Dictionary<Guid, string>> AuditUserNamesResolver(
    Guid[] userIds,
    CancellationToken cancellationToken);
