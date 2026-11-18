using Entity;
using EntityFramework.AppDbContext;
using Perigon.PostgreSQL;

namespace ApiService.Workers;

internal sealed class DatabaseInitializationWorker(IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = configuration.GetConnectionString(AppConst.Default)
            ?? throw new InvalidOperationException("The default database connection is required for migrations.");
        var analysis = configuration.GetConnectionString(AppConst.Analysis) ?? connection;
        await using var context = new DefaultDbContext(connection);
        await context.MigrateAsync();
        var tenants = context.SetWithoutFilters<Tenant>("SoftDelete");
        var existing = await tenants.Where(tenant => tenant.Domain == "default.com").FirstOrDefaultAsync();
        if (existing is null)
            await context.Tenants.InsertAsync(new Tenant { Name = AppConst.Default, Domain = "default.com", Description = "This is default tenant, created by system.", DbConnectionString = connection, AnalysisConnectionString = analysis });
        else if (existing.IsDeleted || string.IsNullOrWhiteSpace(existing.DbConnectionString)
            || string.IsNullOrWhiteSpace(existing.AnalysisConnectionString))
        {
            var databaseConnection = string.IsNullOrWhiteSpace(existing.DbConnectionString) ? connection : existing.DbConnectionString;
            var analysisConnection = string.IsNullOrWhiteSpace(existing.AnalysisConnectionString) ? analysis : existing.AnalysisConnectionString;
            await tenants.Where(tenant => tenant.Id == existing.Id).ExecuteUpdateAsync(update => update
                .Set(tenant => tenant.IsDeleted, false).Set(tenant => tenant.DbConnectionString, databaseConnection).Set(tenant => tenant.AnalysisConnectionString, analysisConnection));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
