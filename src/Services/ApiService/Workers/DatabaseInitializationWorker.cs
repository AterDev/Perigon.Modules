using EntityFramework.AppDbContext;
using Perigon.PostgreSQL;
using SystemMod;

namespace ApiService.Workers;

internal sealed class DatabaseInitializationWorker(IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = configuration.GetConnectionString(AppConst.Default)
            ?? throw new InvalidOperationException("The default database connection is required for migrations.");
        await using var context = new DefaultDbContext(connection);
        await context.MigrateAsync(cancellationToken);
        await InitModule.InitializeAdminAccountAsync(context, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
