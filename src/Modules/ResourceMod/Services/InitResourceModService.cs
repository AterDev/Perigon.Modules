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
            List<Guid> tenantIds = await context.Tenants
                .Select(t => t.Id)
                .ToListAsync(stoppingToken);
            foreach (Guid tenantId in tenantIds)
            {
                bool developmentEnvironmentExists = await context.ResEnvironments.AnyAsync(e =>
                    e.TenantId == tenantId && e.Name == "Dev", stoppingToken);
                if (!developmentEnvironmentExists)
                {
                    context.ResEnvironments.Add(new ResEnvironment
                    {
                        Name = "Dev",
                        Color = "#2196f3",
                        TenantId = tenantId
                    });
                }

                bool defaultCategoryExists = await context.ResCategories.AnyAsync(c =>
                    c.TenantId == tenantId && c.CatalogCode == "Default", stoppingToken);
                if (!defaultCategoryExists)
                {
                    context.ResCategories.Add(new ResCategory
                    {
                        Name = "Default",
                        CatalogCode = "Default",
                        Color = "#9e9e9e",
                        TenantId = tenantId
                    });
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
