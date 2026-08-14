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
            DefaultDbContext context = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            List<Guid> tenantIds = await context.Tenants
                .Select(t => t.Id)
                .ToListAsync(stoppingToken);
            foreach (Guid tenantId in tenantIds)
            {
                await InitializeEnvironmentsAsync(context, tenantId, stoppingToken);
                await InitializeCategoryAsync(context, tenantId, stoppingToken);
                await InitializeTagsAsync(context, tenantId, stoppingToken);
                await InitializeDefinitionsAsync(context, tenantId, stoppingToken);
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
                    existing.Icon = icon;
                }
                continue;
            }

            context.ResEnvironments.Add(new ResEnvironment
            {
                Name = name,
                Icon = icon,
                Color = color,
                TenantId = tenantId
            });
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
                existing.Icon = "category";
            }
            return;
        }

        context.ResCategories.Add(new ResCategory
        {
            Name = "Default",
            CatalogCode = "Default",
            Icon = "category",
            Color = "#9e9e9e",
            TenantId = tenantId
        });
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
                    existing.Icon = icon;
                }
                continue;
            }

            context.ResTags.Add(new ResTag
            {
                Name = name,
                Icon = icon,
                Color = color,
                TenantId = tenantId
            });
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
                existing.Icon = icon;
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
                    ValueType = item.ValueType,
                    IsRequired = item.Name == "名称",
                    MaxLength = 200,
                    TenantId = tenantId
                },
                StringComparer.Ordinal);
        context.ResDefinitionProperties.AddRange(properties.Values);

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

        context.ResDefinitions.AddRange(definitions);
        context.ResDefinitionPropertyMaps.AddRange(maps);
    }
}
