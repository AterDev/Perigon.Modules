using ApiService.Workers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<Share.Serialization.IAppJsonSerializer, ApiService.Serialization.AppJsonSerializer>();
builder.AddFrameworkServices();
builder.AddMiniApiMiddlewareServices();
builder.Services.AddValidation();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiService.ApiJsonSerializerContext.Default));
builder.Services.AddOpenApi(options => options.AddSchemaTransformer<ApiService.OpenApi.EnumSchemaTransformer>());
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(WebConst.AdminUser, policy => policy.RequireRole(WebConst.AdminUser, WebConst.SuperAdmin))
    .AddPolicy(WebConst.SuperAdmin, policy => policy.RequireRole(WebConst.SuperAdmin));
builder.Services.AddManagers();
builder.Services.AddHostedService<DatabaseInitializationWorker>();
builder.AddModules();

WebApplication app = builder.Build();
app.MapDefaultEndpoints();
app.MapEndpointGroups();
app.UseMiniApiMiddlewareServices();
await app.RunAsync();
