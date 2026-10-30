using System.Security.Claims;
using Entity;
using Entity.SystemMod;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Perigon.AspNetCore.Abstraction;
using Perigon.AspNetCore.Constants;
using Perigon.AspNetCore.Options;
using Perigon.AspNetCore.Services;
using Perigon.PostgreSQL;
using Perigon.PostgreSQL.Migrations;
using Share.Implement;
using Share.Services;
using ServiceDefaults;

namespace ApiTest;

/// <summary>
/// PostgreSQL tenant and framework contract tests.
/// Tests use the AppHost database and only create uniquely named rows. Cleanup
/// is soft-delete/update based; no database or user data is dropped.
/// </summary>
public sealed class TenantFrameworkTests
{
    private static string ConnectionString =>
        GlobalHooks.App?.GetConnectionStringAsync(AppConst.Default).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("The AppHost PostgreSQL connection is unavailable.");

    [Before(Class)]
    public static async Task WaitForPostgreSqlAsync()
    {
        if (GlobalHooks.NotificationService is not null)
        {
            await GlobalHooks.NotificationService
                .WaitForResourceAsync("AdminService", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(60));
        }
    }

    [Test]
    public async Task TenantScopedQuery_ShouldIncludeTenantAndSoftDeleteFilters()
    {
        await using var context = new DefaultDbContext(ConnectionString, Guid.CreateVersion7());
        string sql = context.SysUsers.ToQuerySql().CommandText;
        await Assert.That(sql).Contains("TenantId");
        await Assert.That(sql).Contains("IsDeleted");
    }

    [Test]
    public async Task CatalogQuery_ShouldOnlyUseSoftDeleteFilter()
    {
        await using var context = new DefaultDbContext(ConnectionString);
        string sql = context.Tenants.ToQuerySql().CommandText;
        await Assert.That(sql).Contains("IsDeleted");
        await Assert.That(sql.Contains("TenantId", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task IgnoringAllFilters_ShouldRemoveTenantAndSoftDeletePredicates()
    {
        await using var context = new DefaultDbContext(ConnectionString, Guid.CreateVersion7());
        string sql = context.SetIgnoringAllFilters<SysUser>().ToQuerySql().CommandText;
        string whereClause = ExtractWhereClause(sql);
        await Assert.That(whereClause.Contains("TenantId", StringComparison.Ordinal)).IsFalse();
        await Assert.That(whereClause.Contains("IsDeleted", StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task MigrationModel_ShouldContainTenantColumnsAndExplicitJoinTables()
    {
        await using var context = new DefaultDbContext(ConnectionString, Guid.CreateVersion7());
        MigrationModel model = context.GetMigrationModel();
        MigrationTable userTable = model.Tables.Single(table => table.Name == "SysUsers");
        await Assert.That(userTable.Columns.Select(column => column.Name))
            .Contains(nameof(ITenantEntityBase.TenantId));
        MigrationTable roleLinkTable = model.Tables.Single(table => table.Name == "SysUserRoles");
        await Assert.That(roleLinkTable.Columns.Select(column => column.Name)).Contains(nameof(SysUserRole.UserId));
        await Assert.That(roleLinkTable.Columns.Select(column => column.Name)).Contains(nameof(SysUserRole.RoleId));
        await Assert.That(roleLinkTable.Columns.Select(column => column.Name))
            .Contains(nameof(ITenantEntityBase.TenantId));
    }

    [Test]
    public async Task PostgreSqlTenantRows_ShouldBeIsolatedAndSoftDeletable()
    {
        Guid tenantA = Guid.CreateVersion7();
        Guid tenantB = Guid.CreateVersion7();
        string suffix = Guid.CreateVersion7().ToString("N");
        Guid rowAId;
        Guid rowBId;

        await using (var context = new DefaultDbContext(ConnectionString, tenantA))
        {
            var row = SysConfig.NewSysConfig("integration", $"tenant-a-{suffix}", "A");
            row.TenantId = tenantA;
            await context.SysConfigs.InsertAsync(row);
            rowAId = row.Id;
        }

        await using (var context = new DefaultDbContext(ConnectionString, tenantB))
        {
            var row = SysConfig.NewSysConfig("integration", $"tenant-b-{suffix}", "B");
            row.TenantId = tenantB;
            await context.SysConfigs.InsertAsync(row);
            rowBId = row.Id;
            List<SysConfig> rows = await context.SysConfigs.ToListAsync();
            await Assert.That(rows.Select(item => item.Key)).IsEquivalentTo([$"tenant-b-{suffix}"]);
            int crossTenantUpdate = await context.SysConfigs
                .Where(item => item.Id == rowAId)
                .ExecuteUpdateAsync(update => update.Set(item => item.Value, "tampered"));
            await Assert.That(crossTenantUpdate).IsEqualTo(0);
        }

        await using (var context = new DefaultDbContext(ConnectionString, tenantA))
        {
            SysConfig row = await context.SysConfigs.Where(item => item.Id == rowAId).FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Tenant A row was not found.");
            await Assert.That(row.Value).IsEqualTo("A");
            int deleted = await context.SysConfigs.Where(item => item.Id == rowAId)
                .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
            await Assert.That(deleted).IsEqualTo(1);
            await Assert.That(await context.SysConfigs.Where(item => item.Id == rowAId).AnyAsync()).IsFalse();
            bool stillStored = await context.SetWithoutFilters<SysConfig>(ContextBase.SoftDeletionFilterName)
                .Where(item => item.Id == rowAId).AnyAsync();
            await Assert.That(stillStored).IsTrue();
        }

        await using (var cleanup = new DefaultDbContext(ConnectionString, tenantB))
        {
            await cleanup.SysConfigs.Where(item => item.Id == rowBId)
                .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        }
    }

    [Test]
    public async Task ClaimsTransformation_WhenTenantClaimIsMissing_LeavesPrincipalUnbound()
    {
        await using var context = new DefaultDbContext(ConnectionString);
        using ServiceProvider services = CreateCacheServiceProvider();
        var cache = CreateCacheService(services);
        var tenantService = new TenantService(context, cache, NullLogger<TenantService>.Instance);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, Guid.CreateVersion7().ToString())], "test"));
        var transformed = await new UserClaimsTransformation(tenantService, cache).TransformAsync(principal);
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantId)).IsNull();
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantType)).IsNull();
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantName)).IsNull();
    }

    [Test]
    public async Task TenantService_WhenTenantIsCached_DoesNotReadCatalogAgain()
    {
        Tenant tenant = await InsertCatalogTenantAsync("cached");
        try
        {
            await using var context = new DefaultDbContext(ConnectionString);
            using ServiceProvider services = CreateCacheServiceProvider();
            var service = new TenantService(context, CreateCacheService(services), NullLogger<TenantService>.Instance);
            await Assert.That(await service.GetByIdAsync(tenant.Id)).IsNotNull();
            await context.Tenants.Where(item => item.Id == tenant.Id)
                .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
            Tenant? cached = await service.GetByIdAsync(tenant.Id);
            await Assert.That(cached).IsNotNull();
            await Assert.That(cached!.Name).IsEqualTo(tenant.Name);
        }
        finally { await SoftDeleteCatalogTenantAsync(tenant.Id); }
    }

    [Test]
    public async Task TenantService_RefreshCacheAsync_ReplacesCachedTenant()
    {
        Tenant tenant = await InsertCatalogTenantAsync("refresh");
        try
        {
            await using var context = new DefaultDbContext(ConnectionString);
            using ServiceProvider services = CreateCacheServiceProvider();
            var service = new TenantService(context, CreateCacheService(services), NullLogger<TenantService>.Instance);
            await service.GetByIdAsync(tenant.Id);
            string updatedName = $"Updated-{tenant.Name}";
            await context.Tenants.Where(item => item.Id == tenant.Id).ExecuteUpdateAsync(update => update
                .Set(item => item.Name, updatedName)
                .Set(item => item.Disabled, true));
            Tenant refreshed = await service.RefreshCacheAsync(tenant.Id)
                ?? throw new InvalidOperationException("Tenant refresh returned null.");
            await Assert.That(refreshed.Name).IsEqualTo(updatedName);
            await Assert.That(refreshed.Disabled).IsTrue();
        }
        finally { await SoftDeleteCatalogTenantAsync(tenant.Id); }
    }

    [Test]
    public async Task AppDbFactory_WhenTenantCacheIsMissing_SelectsTenantConnectionAndCachesMetadata()
    {
        Tenant tenant = await InsertCatalogTenantAsync("factory");
        try
        {
            using ServiceProvider cacheServices = CreateCacheServiceProvider();
            var cache = CreateCacheService(cacheServices);
            using ServiceProvider tenantServices = CreateTenantResolverProvider(cache);
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{AppConst.Default}"] = ConnectionString,
                [$"ConnectionStrings:{AppConst.Analysis}"] = ConnectionString,
            }).Build();
            var factory = new AppDbFactory(configuration, tenantServices.GetRequiredService<IServiceScopeFactory>());
            await using var context = factory.CreateDbContext(tenant.Id);
            await Assert.That(context.ConnectionString).IsEqualTo(ConnectionString);
            await Assert.That(cache.GetMemory<Tenant>(TenantService.GetCacheKey(tenant.Id))).IsNotNull();
        }
        finally { await SoftDeleteCatalogTenantAsync(tenant.Id); }
    }

    [Test]
    public async Task TenantScopedManager_WhenTenantIdIsMissing_ThrowsImmediately()
    {
        Exception? exception = null;
        try { _ = new SysConfigTestManager(CreateFactory(), new TestUserContext(), NullLogger<SysConfigTestManager>.Instance); }
        catch (Exception caught) { exception = caught; }
        await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        await Assert.That(exception!.Message).Contains("TenantId");
    }

    [Test]
    public async Task TenantManager_WhenTenantIdIsMissing_DoesNotRequireTenantContext()
    {
        var manager = new CatalogTenantManager(CreateFactory(), new TestUserContext(), NullLogger<CatalogTenantManager>.Instance);
        await Assert.That(manager).IsNotNull();
        manager.Dispose();
    }

    private static async Task<Tenant> InsertCatalogTenantAsync(string prefix)
    {
        await using var context = new DefaultDbContext(ConnectionString);
        var tenant = new Tenant
        {
            Name = $"{prefix}-{Guid.CreateVersion7():N}",
            Domain = $"{prefix}-{Guid.CreateVersion7():N}.test",
            DbConnectionString = ConnectionString,
            AnalysisConnectionString = ConnectionString,
        };
        await context.Tenants.InsertAsync(tenant);
        return tenant;
    }

    private static async Task SoftDeleteCatalogTenantAsync(Guid tenantId)
    {
        await using var context = new DefaultDbContext(ConnectionString);
        await context.Tenants.Where(item => item.Id == tenantId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
    }

    private static AppDbFactory CreateFactory()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"ConnectionStrings:{AppConst.Default}"] = ConnectionString,
            [$"ConnectionStrings:{AppConst.Analysis}"] = ConnectionString,
        }).Build();
        return new AppDbFactory(configuration, CreateEmptyTenantScopeFactory());
    }

    private static IServiceScopeFactory CreateEmptyTenantScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITenantResolver, EmptyTenantResolver>();
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private static ServiceProvider CreateTenantResolverProvider(CacheService cache)
    {
        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddLogging();
        services.AddScoped<DefaultDbContext>(_ => new DefaultDbContext(ConnectionString));
        services.AddScoped<TenantService>();
        services.AddScoped<ITenantResolver>(provider => provider.GetRequiredService<TenantService>());
        return services.BuildServiceProvider();
    }

    private static ServiceProvider CreateCacheServiceProvider()
    {
        var services = new ServiceCollection();
        services.Configure<CacheOption>(_ => { });
        services.Configure<ComponentOption>(_ => { });
        services.AddMemoryCache();
        services.AddHybridCache();
        return services.BuildServiceProvider();
    }

    private static CacheService CreateCacheService(IServiceProvider services) => new(
        services.GetRequiredService<HybridCache>(), services.GetRequiredService<IMemoryCache>(),
        services.GetRequiredService<IOptions<CacheOption>>(), services.GetRequiredService<IOptions<ComponentOption>>());

    private static string ExtractWhereClause(string sql)
    {
        int whereIndex = sql.IndexOf(" WHERE ", StringComparison.Ordinal);
        return whereIndex < 0 ? string.Empty : sql[(whereIndex + " WHERE ".Length)..];
    }

    private sealed class TestUserContext : IUserContext
    {
        public Guid UserId => Guid.Empty;
        public Guid? GroupId => null;
        public Guid TenantId { get; set; }
        public string? TenantType { get; set; }
        public string? UserName => null;
        public string? Email => null;
        public bool IsAdmin => false;
        public string? CurrentRole => null;
        public IReadOnlyList<string>? Roles => [];
        public IReadOnlyList<Guid> RoleIds => [];
        public HttpContext? HttpContext { get; set; }
        public bool IsRole(string roleName) => false;
    }

    private sealed class EmptyTenantResolver : ITenantResolver
    {
        public Tenant? GetById(Guid tenantId) => null;
    }

    private sealed class SysConfigTestManager(AppDbFactory factory, IUserContext userContext, Microsoft.Extensions.Logging.ILogger logger)
        : ManagerBase<DefaultDbContext, SysConfig>(factory, userContext, logger)
    {
        public override Task<bool> HasPermissionAsync(Guid id) => Task.FromResult(true);
    }

    private sealed class CatalogTenantManager(AppDbFactory factory, IUserContext userContext, Microsoft.Extensions.Logging.ILogger logger)
        : ManagerBase<DefaultDbContext, Tenant>(factory, userContext, logger)
    {
        public override Task<bool> HasPermissionAsync(Guid id) => Task.FromResult(true);
    }
}
