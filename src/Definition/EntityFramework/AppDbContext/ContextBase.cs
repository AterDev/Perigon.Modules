using System.Diagnostics.CodeAnalysis;
using Perigon.AspNetCore.Abstraction;
using Perigon.PostgreSQL;

namespace EntityFramework.AppDbContext;

/// <summary>PostgreSQL context bound to one tenant for its lifetime.</summary>
public abstract class ContextBase(string connectionString, Guid? tenantId) : DbContext(options => options.UsePostgres(connectionString))
{
    public const string SoftDeletionFilterName = "SoftDeletionFilter";
    public const string TenantFilterName = "TenantFilter";
    public Guid? CurrentTenantId { get; } = tenantId is { } id && id != Guid.Empty ? id : null;
    public string ConnectionString { get; } = connectionString;

    public Guid RequiredTenantId => CurrentTenantId
        ?? throw new InvalidOperationException("A tenant id is required for tenant-scoped database access.");

    public new DbSet<TEntity> Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>() where TEntity : class
        => base.Set<TEntity>();

    protected void AddTenantFilters<TEntity>() where TEntity : class, ITenantEntityBase
    {
        HasQueryFilter<TEntity>(SoftDeletionFilterName, entity => !entity.IsDeleted);
        HasQueryFilter<TEntity>(TenantFilterName, entity => entity.TenantId == RequiredTenantId);
    }
}
