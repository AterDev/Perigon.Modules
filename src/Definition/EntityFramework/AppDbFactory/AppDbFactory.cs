using EntityFramework.AppDbContext;
using Microsoft.Extensions.Configuration;
using Perigon.AspNetCore.Constants;
using Perigon.PostgreSQL.Options;

namespace EntityFramework.AppDbFactory;

/// <summary>Creates a context for the configured application database.</summary>
public sealed class AppDbFactory(IConfiguration configuration)
{
    public DefaultDbContext CreateDbContext()
    {
        return CreateDbContext(GetConnectionString(AppConst.Default));
    }

    public static DefaultDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder()
            .UsePostgres(connectionString)
            .Build<DefaultDbContext>();
        return new DefaultDbContext(options);
    }

    public Task<DefaultDbContext> CreateDbContextAsync()
    {
        return Task.FromResult(CreateDbContext());
    }

    private string GetConnectionString(string name)
    {
        return configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"No connection string configured for '{name}'.");
    }
}
