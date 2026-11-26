using ApiService.Workers;
using AuditMod.Demands;
using NotificationMod.Demands;
using NotificationMod.Managers;
using NotificationMod.Models.InAppNotificationDtos;
using SystemMod.Managers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddFrameworkServices();
builder.AddMiddlewareServices();
builder.Services.AddValidation();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiService.ApiJsonSerializerContext.Default));
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
    options.AddSchemaTransformer<ApiService.OpenApi.EnumSchemaTransformer>();
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(WebConst.AdminUser, policy => policy.RequireRole(WebConst.AdminUser, WebConst.SuperAdmin))
    .AddPolicy(WebConst.SuperAdmin, policy => policy.RequireRole(WebConst.SuperAdmin));
builder.Services.AddManagers();
builder.Services.AddHostedService<DatabaseInitializationWorker>();
builder.Services.AddHostedService<SystemLogTaskHostedService>();
builder.AddModules();
builder.Services.AddScoped<AuditUserNamesResolver>(services =>
    services.GetRequiredService<SysUserManager>().GetDisplayNamesAsync);
builder.Services.AddScoped<AuditUserIdsResolver>(services =>
    services.GetRequiredService<SysUserManager>().GetUserIdsAsync);
builder.Services.AddScoped<NotificationUserIdsResolver>(services =>
    services.GetRequiredService<SysUserManager>().GetUserIdsAsync);
builder.Services.AddScoped<AuditNotificationPublisher>(serviceProvider =>
{
    var notifications = serviceProvider.GetRequiredService<NotificationManager>();
    return (context, message, cancellationToken) => notifications.PublishInTransactionAsync(
        context,
        new InAppNotificationPublishDto(
            message.Type,
            message.Title,
            message.Content,
            message.Link,
            message.CreatedBy,
            message.TargetUserIds.ToArray()),
        cancellationToken);
});

WebApplication app = builder.Build();
app.MapDefaultEndpoints();
app.MapEndpointGroups();
app.UseMiddlewareServices();
await app.RunAsync();
