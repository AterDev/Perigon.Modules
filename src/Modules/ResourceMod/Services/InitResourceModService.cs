using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ResourceMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitResourceModService(
    IServiceProvider serviceProvider,
    ILogger<InitResourceModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            DefaultDbContext context = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            List<Guid> tenantIds = await context.Tenants.Select(t => t.Id).ToListAsync(stoppingToken);
            foreach (Guid tenantId in tenantIds)
            {
                if (!await context.ResEnvironments.AnyAsync(e => e.TenantId == tenantId && e.Name == "Dev", stoppingToken))
                {
                    context.ResEnvironments.Add(new ResEnvironment { Name = "Dev", Color = "#2196f3", TenantId = tenantId });
                }
                if (!await context.ResCategories.AnyAsync(c => c.TenantId == tenantId && c.CatalogCode == "Default", stoppingToken))
                {
                    context.ResCategories.Add(new ResCategory { Name = "Default", CatalogCode = "Default", Color = "#9e9e9e", TenantId = tenantId });
                }
            }
            await context.SaveChangesAsync(stoppingToken);
            logger.LogInformation("ResourceMod initialized for {TenantCount} tenants", tenantIds.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceMod initialization failed");
            return;
        }
    }
}
