using EntityFramework.AppDbContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Perigon.AspNetCore.Constants;

namespace EntityFramework.AppDbFactory;

/// <summary>Creates a context using the catalog or the selected tenant's configured connection.</summary>
public sealed class AppDbFactory(IConfiguration configuration, IServiceScopeFactory tenantScopeFactory)
{
    public DefaultDbContext CreateDbContext(Guid? tenantId) => new(GetConnectionStrings(tenantId).Default, tenantId);
    public Task<DefaultDbContext> CreateDbContextAsync(Guid? tenantId = null) => Task.FromResult(CreateDbContext(tenantId));
    public AnalysisDbContext CreateAnalysisDbContext(Guid? tenantId) => new(GetConnectionStrings(tenantId).Analysis, tenantId);
    public Task<AnalysisDbContext> CreateAnalysisDbContextAsync(Guid? tenantId = null) => Task.FromResult(CreateAnalysisDbContext(tenantId));

    private (string Default, string Analysis) GetConnectionStrings(Guid? tenantId)
    {
        var connection = configuration.GetConnectionString(AppConst.Default)
            ?? throw new InvalidOperationException("No default connection string configured");
        var analysis = configuration.GetConnectionString(AppConst.Analysis) ?? connection;
        if (!tenantId.HasValue) return (connection, analysis);
        if (tenantId.Value == Guid.Empty) throw new InvalidOperationException("A non-empty TenantId is required for tenant-scoped database access.");
        using var scope = tenantScopeFactory.CreateScope();
        var tenant = scope.ServiceProvider.GetRequiredService<ITenantResolver>().GetById(tenantId.Value)
            ?? throw new InvalidOperationException($"Tenant '{tenantId}' was not found in the tenant catalog.");
        return (tenant.DbConnectionString ?? connection, tenant.AnalysisConnectionString ?? analysis);
    }
}
