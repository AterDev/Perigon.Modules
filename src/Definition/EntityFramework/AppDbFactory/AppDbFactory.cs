using EntityFramework.AppDbContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Perigon.AspNetCore.Constants;
using Perigon.AspNetCore.Services;

namespace EntityFramework.AppDbFactory;

/// <summary>
/// Factory for creating tenant-aware database contexts.
/// </summary>
/// <param name="cache"></param>
/// <param name="configuration"></param>
public class AppDbFactory(IOptions<ComponentOption> options, CacheService cache, IConfiguration configuration)
{
    public DefaultDbContext CreateDbContext(Guid? tenantId)
    {
        var (connectionString, _) = GetConnectionStrings(tenantId);

        var builder = new DbContextOptionsBuilder<DefaultDbContext>();
        switch (options?.Value.Database)
        {
            case DatabaseType.PostgreSql:
                builder.UseNpgsql(connectionString);
                break;
            case DatabaseType.SqlServer:
                builder.UseSqlServer(connectionString);
                break;
        }
        var context = new DefaultDbContext(builder.Options);
        context.SetTenantId(tenantId);
        return context;
    }

    public Task<DefaultDbContext> CreateDbContextAsync(Guid? tenantId = null)
    {
        return Task.FromResult(CreateDbContext(tenantId));
    }

    public AnalysisDbContext CreateAnalysisDbContext(Guid? tenantId)
    {
        var (_, analysisConnectionString) = GetConnectionStrings(tenantId);
        var builder = new DbContextOptionsBuilder<AnalysisDbContext>();
        switch (options?.Value.Database)
        {
            case DatabaseType.PostgreSql:
                builder.UseNpgsql(analysisConnectionString);
                break;
            case DatabaseType.SqlServer:
                builder.UseSqlServer(analysisConnectionString);
                break;
        }
        var context = new AnalysisDbContext(builder.Options);
        context.SetTenantId(tenantId);
        return context;
    }

    public Task<AnalysisDbContext> CreateAnalysisDbContextAsync(Guid? tenantId = null)
    {
        return Task.FromResult(CreateAnalysisDbContext(tenantId));
    }

    private (string DbConnectionString, string AnalysisConnectionString) GetConnectionStrings(Guid? tenantId)
    {
        var defaultConnectionString = configuration.GetConnectionString(AppConst.Default)
            ?? throw new InvalidOperationException("No default connection string configured");
        var defaultAnalysisConnectionString = configuration.GetConnectionString(AppConst.Analysis)
            ?? defaultConnectionString;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
        {
            return (defaultConnectionString, defaultAnalysisConnectionString);
        }

        var cacheKey = $"{WebConst.TenantId}__{tenantId.Value}";
        var tenant = cache.GetMemory<Tenant>(cacheKey);
        if (tenant is null)
        {
            return (defaultConnectionString, defaultAnalysisConnectionString);
        }

        var tenantDbConnectionString = tenant.DbConnectionString ?? defaultConnectionString;
        var tenantAnalysisConnectionString = tenant.AnalysisConnectionString ?? defaultAnalysisConnectionString;
        return (tenantDbConnectionString, tenantAnalysisConnectionString);
    }
}
