using Microsoft.Extensions.Hosting;

namespace SystemMod.Services;

/// <summary>
/// Initializes the tenant administrator and baseline system configuration.
/// </summary>
public class InitSystemModService(
    IServiceProvider serviceProvider,
    ILogger<InitSystemModService> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            await InitModule.InitializeAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SystemMod initialization failed");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
