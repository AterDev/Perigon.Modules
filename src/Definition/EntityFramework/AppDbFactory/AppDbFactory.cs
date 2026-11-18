using EntityFramework.AppDbContext;
using Microsoft.Extensions.Configuration;
using Perigon.AspNetCore.Constants;

namespace EntityFramework.AppDbFactory;

/// <summary>Creates a context for the configured application database.</summary>
public sealed class AppDbFactory(IConfiguration configuration)
{
    public DefaultDbContext CreateDbContext()
    {
        return new DefaultDbContext(GetConnectionString(AppConst.Default));
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
