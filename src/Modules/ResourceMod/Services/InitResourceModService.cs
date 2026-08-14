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
    private static readonly (string Name, string Color)[] DefaultEnvironments =
    [
        ("Development", "#4caf50"),
        ("Test", "#2196f3"),
        ("Production", "#f44336")
    ];

    private static readonly (string Name, string Color)[] DefaultTags =
    [
        ("Mac", "#9e9e9e"),
        ("Linux", "#ff9800"),
        ("Windows", "#673ab7")
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

    private static readonly (string Name, string[] Properties)[] DefaultDefinitions =
    [
        ("网站", ["名称", "Url", "IconUrl", "描述", "用户名", "密码"]),
        ("服务器", ["名称", "IP", "Port", "用户名", "密码"]),
        ("数据库", ["名称", "IP", "Url", "Port", "用户名", "密码"])
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
        List<string> existingNames = await context.ResEnvironments
            .Where(environment => environment.TenantId == tenantId)
            .Select(environment => environment.Name)
            .ToListAsync(cancellationToken);

        foreach ((string name, string color) in DefaultEnvironments)
        {
            if (existingNames.Contains(name, StringComparer.Ordinal))
            {
                continue;
            }

            context.ResEnvironments.Add(new ResEnvironment
            {
                Name = name,
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
        bool exists = await context.ResCategories.AnyAsync(category =>
            category.TenantId == tenantId && category.CatalogCode == "Default", cancellationToken);
        if (exists)
        {
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
        List<string> existingNames = await context.ResTags
            .Where(tag => tag.TenantId == tenantId)
            .Select(tag => tag.Name)
            .ToListAsync(cancellationToken);

        foreach ((string name, string color) in DefaultTags)
        {
            if (existingNames.Contains(name, StringComparer.Ordinal))
            {
                continue;
            }

            context.ResTags.Add(new ResTag
            {
                Name = name,
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
        bool hasDefinitions = await context.ResDefinitions.AnyAsync(definition =>
            definition.TenantId == tenantId, cancellationToken);
        bool hasProperties = await context.ResDefinitionProperties.AnyAsync(property =>
            property.TenantId == tenantId, cancellationToken);
        if (hasDefinitions || hasProperties)
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
        foreach ((string name, string[] propertyNames) in DefaultDefinitions)
        {
            ResDefinition definition = new()
            {
                Name = name,
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
