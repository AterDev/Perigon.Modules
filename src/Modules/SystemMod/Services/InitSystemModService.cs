using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SystemMod.Services;

/// <summary>
/// Initializes the tenant administrator and baseline system configuration.
/// </summary>
public class InitSystemModService(
    IServiceProvider serviceProvider,
    ILogger<InitSystemModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            await InitModule.InitializeAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SystemMod initialization failed");
        }
    }
}
