using Microsoft.Extensions.Configuration;

namespace EntityFramework.AppDbFactory;

/// <summary>Creates an independently configured PostgreSQL context.</summary>
public sealed class UniversalDbFactory(IConfiguration configuration)
{
    public TContext CreateDbContext<TContext>() where TContext : Perigon.PostgreSQL.DbContext
    {
        var name = typeof(TContext).Name;
        if (name.EndsWith("DbContext", StringComparison.Ordinal)) name = name[..^"DbContext".Length];
        var connection = configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string for {name} not found.");
        return (TContext)(Activator.CreateInstance(typeof(TContext), connection)
            ?? throw new InvalidOperationException($"Could not create {typeof(TContext).Name}."));
    }
}
