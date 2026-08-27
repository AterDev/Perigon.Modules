using Entity;

namespace EntityFramework.AppDbFactory;

/// <summary>
/// Resolves tenant metadata for components that must create a context synchronously.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Gets a tenant from cache, falling back to the catalog database when necessary.
    /// </summary>
    Tenant? GetById(Guid tenantId);
}
