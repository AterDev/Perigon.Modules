using Microsoft.Extensions.Hosting;
using EntityFramework.AppDbFactory;
using Perigon.PostgreSQL;

namespace ResourceMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitResourceModService(
    IServiceProvider serviceProvider,
    ILogger<InitResourceModService> logger
) : BackgroundService
{
    private static readonly (string Name, string Color, string Icon)[] DefaultEnvironments =
    [
        ("Development", "#4caf50", "code"),
        ("Test", "#2196f3", "science"),
        ("Production", "#f44336", "public")
    ];

    private static readonly (string Name, string Color, string Icon)[] DefaultTags =
    [
        ("Mac", "#9e9e9e", "desktop_mac"),
        ("Linux", "#ff9800", "terminal"),
        ("Windows", "#673ab7", "desktop_windows")
    ];

    private static readonly (string Name, ResValueType ValueType)[] DefaultProperties =
    [
        ("名称", ResValueType.String),
        ("Url", ResValueType.Uri),
        ("描述", ResValueType.String),
        ("IP", ResValueType.IPAddress),
        ("Port", ResValueType.Number),
        ("用户名", ResValueType.String),
        ("密码", ResValueType.String),
        ("密钥", ResValueType.String),
        ("APIKey", ResValueType.String),
        ("Token", ResValueType.String),
        ("AppId", ResValueType.String),
        ("AppSecret", ResValueType.String),
        ("IconUrl", ResValueType.Uri)
    ];

    private static readonly (string Name, string Icon, string[] Properties)[] DefaultDefinitions =
    [
        ("网站", "web", ["名称", "Url", "IconUrl", "描述", "用户名", "密码"]),
        ("服务器", "dns", ["名称", "IP", "Port", "用户名", "密码"]),
        ("数据库", "database", ["名称", "IP", "Url", "Port", "用户名", "密码"])
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            DefaultDbContext catalogContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            AppDbFactory dbContextFactory = scope.ServiceProvider.GetRequiredService<AppDbFactory>();
            List<Tenant> tenants = await catalogContext.Tenants.ToListAsync(stoppingToken);
            foreach (Tenant tenant in tenants)
            {
                Guid tenantId = tenant.Id;
                await using DefaultDbContext context = dbContextFactory.CreateDbContext(tenantId);
                await context.TransactionAsync(async cancellationToken =>
                {
                    await InitializeEnvironmentsAsync(context, tenantId, cancellationToken);
                    await InitializeCategoryAsync(context, tenantId, cancellationToken);
                    await InitializeTagsAsync(context, tenantId, cancellationToken);
                    await InitializeDefinitionsAsync(context, tenantId, cancellationToken);
                }, stoppingToken);
            }
            logger.LogInformation("ResourceMod initialized for {TenantCount} tenants", tenants.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceMod initialization failed");
            return;
        }
    }

    private static async Task InitializeEnvironmentsAsync(
        DefaultDbContext context,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        List<ResEnvironment> existingEnvironments = await context.ResEnvironments
            .Where(environment => environment.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach ((string name, string color, string icon) in DefaultEnvironments)
        {
            ResEnvironment? existing = existingEnvironments.FirstOrDefault(environment =>
                string.Equals(environment.Name, name, StringComparison.Ordinal));
            if (existing != null)
            {
                if (string.IsNullOrWhiteSpace(existing.Icon))
                {
                    await context.ResEnvironments.Where(item => item.Id == existing.Id)
                        .ExecuteUpdateAsync(s => s.Set(item => item.Icon, icon), cancellationToken: cancellationToken);
                }
                continue;
            }

            await context.ResEnvironments.InsertAsync(new ResEnvironment
            {
                Name = name,
                Icon = icon,
                Color = color,
                TenantId = tenantId
            }, cancellationToken);
        }
    }

    private static async Task InitializeCategoryAsync(
        DefaultDbContext context,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        ResCategory? existing = await context.ResCategories.FirstOrDefaultAsync(category =>
            category.TenantId == tenantId && category.CatalogCode == "Default", cancellationToken);
        if (existing != null)
        {
            if (string.IsNullOrWhiteSpace(existing.Icon))
            {
                await context.ResCategories.Where(item => item.Id == existing.Id)
                    .ExecuteUpdateAsync(s => s.Set(item => item.Icon, "category"), cancellationToken: cancellationToken);
            }
            return;
        }

        await context.ResCategories.InsertAsync(new ResCategory
        {
            Name = "Default",
            CatalogCode = "Default",
            Icon = "category",
            Color = "#9e9e9e",
            TenantId = tenantId
        }, cancellationToken);
    }

    private static async Task InitializeTagsAsync(
        DefaultDbContext context,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        List<ResTag> existingTags = await context.ResTags
            .Where(tag => tag.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach ((string name, string color, string icon) in DefaultTags)
        {
            ResTag? existing = existingTags.FirstOrDefault(tag =>
                string.Equals(tag.Name, name, StringComparison.Ordinal));
            if (existing != null)
            {
                if (string.IsNullOrWhiteSpace(existing.Icon))
                {
                    await context.ResTags.Where(item => item.Id == existing.Id)
                        .ExecuteUpdateAsync(s => s.Set(item => item.Icon, icon), cancellationToken: cancellationToken);
                }
                continue;
            }

            await context.ResTags.InsertAsync(new ResTag
            {
                Name = name,
                Icon = icon,
                Color = color,
                TenantId = tenantId
            }, cancellationToken);
        }
    }

    private static async Task InitializeDefinitionsAsync(
        DefaultDbContext context,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        List<ResDefinition> existingDefinitions = await context.ResDefinitions
            .Where(definition => definition.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        foreach ((string name, string icon, _) in DefaultDefinitions)
        {
            ResDefinition? existing = existingDefinitions.FirstOrDefault(definition =>
                string.Equals(definition.Name, name, StringComparison.Ordinal));
            if (existing != null && string.IsNullOrWhiteSpace(existing.Icon))
            {
                await context.ResDefinitions.Where(item => item.Id == existing.Id)
                    .ExecuteUpdateAsync(s => s.Set(item => item.Icon, icon), cancellationToken: cancellationToken);
            }
        }

        bool hasProperties = await context.ResDefinitionProperties.AnyAsync(property =>
            property.TenantId == tenantId, cancellationToken);
        if (existingDefinitions.Count != 0 || hasProperties)
        {
            return;
        }

        Dictionary<string, ResDefinitionProperty> properties = DefaultProperties
            .ToDictionary(
                item => item.Name,
                item => new ResDefinitionProperty
                {
                    Name = item.Name,
                    ValueTypeValue = (int)item.ValueType,
                    IsRequired = item.Name == "名称",
                    MaxLength = 200,
                    TenantId = tenantId
                },
                StringComparer.Ordinal);
        await context.ResDefinitionProperties.BulkInsertAsync(properties.Values, cancellationToken: cancellationToken);

        List<ResDefinition> definitions = [];
        List<ResDefinitionPropertyMap> maps = [];
        foreach ((string name, string icon, string[] propertyNames) in DefaultDefinitions)
        {
            ResDefinition definition = new()
            {
                Name = name,
                Icon = icon,
                TenantId = tenantId
            };
            definitions.Add(definition);

            for (int sort = 0; sort < propertyNames.Length; sort++)
            {
                ResDefinitionProperty property = properties[propertyNames[sort]];
                maps.Add(new ResDefinitionPropertyMap
                {
                    DefinitionId = definition.Id,
                    PropertyId = property.Id,
                    Sort = sort,
                    TenantId = tenantId
                });
            }
        }

        await context.ResDefinitions.BulkInsertAsync(definitions, cancellationToken: cancellationToken);
        await context.ResDefinitionPropertyMaps.BulkInsertAsync(maps, cancellationToken: cancellationToken);
    }
}
