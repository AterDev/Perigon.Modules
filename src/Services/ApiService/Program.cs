using ApiService.Workers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddFrameworkServices();
builder.AddMiddlewareServices();
builder.Services.AddValidation();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiService.ApiJsonSerializerContext.Default));
builder.Services.AddOpenApi(options => options.AddSchemaTransformer<ApiService.OpenApi.EnumSchemaTransformer>());
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(WebConst.AdminUser, policy => policy.RequireRole(WebConst.AdminUser, WebConst.SuperAdmin))
    .AddPolicy(WebConst.SuperAdmin, policy => policy.RequireRole(WebConst.SuperAdmin));
builder.Services.AddManagers();
builder.Services.AddHostedService<DatabaseInitializationWorker>();
builder.Services.AddHostedService<SystemLogTaskHostedService>();
builder.AddModules();

WebApplication app = builder.Build();
app.MapDefaultEndpoints();
app.MapEndpointGroups();
app.UseMiddlewareServices();
await app.RunAsync();
