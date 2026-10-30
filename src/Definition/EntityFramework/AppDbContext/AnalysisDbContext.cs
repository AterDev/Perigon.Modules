namespace EntityFramework.AppDbContext;

/// <summary>PostgreSQL analysis context bound to one tenant.</summary>
public sealed class AnalysisDbContext(string connectionString, Guid? tenantId = null)
    : DefaultDbContext(connectionString, tenantId);
