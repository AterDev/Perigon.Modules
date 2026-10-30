using Entity;
using EntityFramework.AppDbContext;
using Perigon.PostgreSQL;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
if (string.Equals(builder.Configuration["Perigon:MigrationMode"], "Apply", StringComparison.OrdinalIgnoreCase))
{
    var connection = builder.Configuration.GetConnectionString(AppConst.Default)
        ?? throw new InvalidOperationException("The default database connection is required for migrations.");
    var analysis = builder.Configuration.GetConnectionString(AppConst.Analysis) ?? connection;
    await using var context = new DefaultDbContext(connection);
    // MigrateAsync does not baseline an existing EF database: migration must be
    // explicitly reviewed and baselined before the new history is applied.
    await context.MigrateAsync();
    var tenants = context.SetWithoutFilters<Tenant>("SoftDelete");
    var existing = await tenants.Where(tenant => tenant.Domain == "default.com").FirstOrDefaultAsync();
    if (existing is null)
    {
        await context.Tenants.InsertAsync(new Tenant
        {
            Name = AppConst.Default,
            Domain = "default.com",
            Description = "This is default tenant, created by system.",
            DbConnectionString = connection,
            AnalysisConnectionString = analysis,
        });
    }
    else
    {
        var databaseConnection = string.IsNullOrWhiteSpace(existing.DbConnectionString) ? connection : existing.DbConnectionString;
        var analysisConnection = string.IsNullOrWhiteSpace(existing.AnalysisConnectionString) ? analysis : existing.AnalysisConnectionString;
        await tenants.Where(tenant => tenant.Id == existing.Id).ExecuteUpdateAsync(update => update
            .Set(tenant => tenant.IsDeleted, false)
            .Set(tenant => tenant.DbConnectionString, databaseConnection)
            .Set(tenant => tenant.AnalysisConnectionString, analysisConnection));
    }
    return;
}
// 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
builder.AddServiceDefaults();
// 框架依赖服务:options, cache, dbContext
builder.AddFrameworkServices();
// Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
builder.AddMiddlewareServices();
builder.Services.AddAuthorizationBuilder().AddPolicy(WebConst.AdminUser, policy =>
{
    policy.RequireRole(WebConst.AdminUser, WebConst.SuperAdmin);
}).AddPolicy(WebConst.SuperAdmin, policy =>
{
    policy.RequireRole(WebConst.SuperAdmin);
});
// 业务Managers
builder.Services.AddManagers();
// 模块服务
builder.AddModules();
WebApplication app = builder.Build();
app.MapDefaultEndpoints();
// 使用中间件
app.UseMiddlewareServices();
app.Run();
