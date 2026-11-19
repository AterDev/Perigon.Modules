using EntityFramework.AppDbContext;

namespace ApiService.Workers;

internal sealed class DatabaseInitializationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseInitializationWorker> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            await context.MigrateAsync(cancellationToken);
            await SystemMod.InitModule.InitializeAsync(context, cancellationToken);
            foreach (var initialize in scope.ServiceProvider.GetServices<Func<DefaultDbContext, CancellationToken, Task>>())
            {
                await initialize(context, cancellationToken);
            }
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Database initialization failed. The application will continue startup.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
