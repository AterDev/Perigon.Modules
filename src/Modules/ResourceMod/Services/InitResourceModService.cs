using Microsoft.Extensions.Hosting;
using EntityFramework.AppDbContext;
using Perigon.PostgreSQL;

namespace ResourceMod.Services;

public sealed class InitResourceModService(
    IServiceProvider serviceProvider,
    ILogger<InitResourceModService> logger
) : BackgroundService
{
    private static readonly (string Name, string Color, string Icon)[] DefaultEnvironments =
    [
        ("Development", "#4caf50", "code"),
        ("Test", "#2196f3", "science"),
        ("Production", "#f44336", "public"),
    ];

    private static readonly (string Name, string Color, string Icon)[] DefaultTags =
    [
        ("Mac", "#9e9e9e", "desktop_mac"),
        ("Linux", "#ff9800", "terminal"),
        ("Windows", "#673ab7", "desktop_windows"),
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
        ("IconUrl", ResValueType.Uri),
    ];

    private static readonly (string Name, string Icon, string[] Properties)[] DefaultDefinitions =
    [
        ("网站", "web", ["名称", "Url", "IconUrl", "描述", "用户名", "密码"]),
        ("服务器", "dns", ["名称", "IP", "Port", "用户名", "密码"]),
        ("数据库", "database", ["名称", "IP", "Url", "Port", "用户名", "密码"]),
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            DefaultDbContext context = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            await context.TransactionAsync(async cancellationToken =>
            {
                await InitializeEnvironmentsAsync(context, cancellationToken);
                await InitializeCategoryAsync(context, cancellationToken);
                await InitializeTagsAsync(context, cancellationToken);
                await InitializeDefinitionsAsync(context, cancellationToken);
            }, stoppingToken);
            logger.LogInformation("ResourceMod initialization completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceMod initialization failed");
        }
    }

    private static async Task InitializeEnvironmentsAsync(
        DefaultDbContext context,
        CancellationToken cancellationToken)
    {
        List<ResEnvironment> existingEnvironments = await context.ResEnvironments
            .ToListAsync(cancellationToken);

        foreach ((string name, string color, string icon) in DefaultEnvironments)
        {
            ResEnvironment? existing = existingEnvironments.FirstOrDefault(environment =>
                string.Equals(environment.Name, name, StringComparison.Ordinal));
            if (existing is not null)
            {
                if (string.IsNullOrWhiteSpace(existing.Icon))
                {
                    await context.ResEnvironments
                        .Where(item => item.Id == existing.Id)
                        .ExecuteUpdateAsync(
                            update => update.Set(item => item.Icon, icon),
                            cancellationToken: cancellationToken);
                }

                continue;
            }

            await context.ResEnvironments.InsertAsync(
                new ResEnvironment
                {
                    Name = name,
                    Icon = icon,
                    Color = color,
                },
                cancellationToken);
        }
    }

    private static async Task InitializeCategoryAsync(
        DefaultDbContext context,
        CancellationToken cancellationToken)
    {
        ResCategory? existing = await context.ResCategories
            .FirstOrDefaultAsync(category => category.CatalogCode == "Default", cancellationToken);
        if (existing is not null)
        {
            if (string.IsNullOrWhiteSpace(existing.Icon))
            {
                await context.ResCategories
                    .Where(item => item.Id == existing.Id)
                    .ExecuteUpdateAsync(
                        update => update.Set(item => item.Icon, "category"),
                        cancellationToken: cancellationToken);
            }

            return;
        }

        await context.ResCategories.InsertAsync(
            new ResCategory
            {
                Name = "Default",
                CatalogCode = "Default",
                Icon = "category",
                Color = "#9e9e9e",
            },
            cancellationToken);
    }

    private static async Task InitializeTagsAsync(
        DefaultDbContext context,
        CancellationToken cancellationToken)
    {
        List<ResTag> existingTags = await context.ResTags.ToListAsync(cancellationToken);
        foreach ((string name, string color, string icon) in DefaultTags)
        {
            ResTag? existing = existingTags.FirstOrDefault(tag =>
                string.Equals(tag.Name, name, StringComparison.Ordinal));
            if (existing is not null)
            {
                if (string.IsNullOrWhiteSpace(existing.Icon))
                {
                    await context.ResTags
                        .Where(item => item.Id == existing.Id)
                        .ExecuteUpdateAsync(
                            update => update.Set(item => item.Icon, icon),
                            cancellationToken: cancellationToken);
                }

                continue;
            }

            await context.ResTags.InsertAsync(
                new ResTag
                {
                    Name = name,
                    Icon = icon,
                    Color = color,
                },
                cancellationToken);
        }
    }

    private static async Task InitializeDefinitionsAsync(
        DefaultDbContext context,
        CancellationToken cancellationToken)
    {
        List<ResDefinition> existingDefinitions = await context.ResDefinitions
            .ToListAsync(cancellationToken);
        foreach ((string name, string icon, _) in DefaultDefinitions)
        {
            ResDefinition? existing = existingDefinitions.FirstOrDefault(definition =>
                string.Equals(definition.Name, name, StringComparison.Ordinal));
            if (existing is not null && string.IsNullOrWhiteSpace(existing.Icon))
            {
                await context.ResDefinitions
                    .Where(item => item.Id == existing.Id)
                    .ExecuteUpdateAsync(
                        update => update.Set(item => item.Icon, icon),
                        cancellationToken: cancellationToken);
            }
        }

        bool hasProperties = await context.ResDefinitionProperties.AnyAsync(cancellationToken);
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
                },
                StringComparer.Ordinal);
        await context.ResDefinitionProperties
            .BulkInsertAsync(properties.Values, cancellationToken: cancellationToken);

        List<ResDefinition> definitions = [];
        List<ResDefinitionPropertyMap> maps = [];
        foreach ((string name, string icon, string[] propertyNames) in DefaultDefinitions)
        {
            ResDefinition definition = new()
            {
                Name = name,
                Icon = icon,
            };
            definitions.Add(definition);

            for (int sort = 0; sort < propertyNames.Length; sort++)
            {
                ResDefinitionProperty property = properties[propertyNames[sort]];
                maps.Add(
                    new ResDefinitionPropertyMap
                    {
                        DefinitionId = definition.Id,
                        PropertyId = property.Id,
                        Sort = sort,
                    });
            }
        }

        await context.ResDefinitions.BulkInsertAsync(
            definitions,
            cancellationToken: cancellationToken);
        await context.ResDefinitionPropertyMaps.BulkInsertAsync(
            maps,
            cancellationToken: cancellationToken);
    }
}
