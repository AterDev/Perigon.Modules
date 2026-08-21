using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using Entity;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
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
using ServiceDefaults;
using Share.Implement;

namespace ApiTest;

public sealed class TenantFrameworkTests
{
    [Test]
    public async Task MigrationModel_WhenNewTenantEntityHasOrdinaryIndex_AddsTenantId()
    {
        await using var context = CreateConventionContext();
        var differ = context.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
        var designTimeModel = context.GetService<IDesignTimeModel>().Model;

        var operations = differ.GetDifferences(null!, designTimeModel.GetRelationalModel());
        var index = operations
            .OfType<CreateIndexOperation>()
            .Single(operation => operation.Table == "TenantFrameworkTestEntities");

        await Assert.That(index.Columns).IsEquivalentTo(
            [nameof(ITenantEntityBase.TenantId), nameof(TenantFrameworkTestEntity.Code)]
        );
        await Assert.That(index.IsUnique).IsTrue();
        await Assert.That(index.Filter).IsEqualTo("\"IsDeleted\" = 0");
    }

    [Test]
    public async Task MigrationModel_WhenTenantIndexIsAlreadyConfigured_DoesNotDuplicateTenantId()
    {
        await using var context = CreateConventionContext();
        var differ = context.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
        var designTimeModel = context.GetService<IDesignTimeModel>().Model;

        var operations = differ.GetDifferences(null!, designTimeModel.GetRelationalModel());
        var indexes = operations
            .OfType<CreateIndexOperation>()
            .Where(operation => operation.Table == "TenantFrameworkTestEntitiesWithIndex")
            .ToArray();

        await Assert.That(indexes.Length).IsEqualTo(1);
        await Assert.That(indexes[0].Columns).IsEquivalentTo(
            [nameof(ITenantEntityBase.TenantId), nameof(TenantFrameworkTestEntityWithIndex.Code)]
        );
    }

    [Test]
    public async Task DefaultDbContextSeeding_WhenDatabaseIsCreated_CreatesDefaultTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var optionsBuilder = new DbContextOptionsBuilder<DefaultDbContext>();
        optionsBuilder.UseSqlite(connection).UseDefaultDbContextSeeding();

        await using var context = new DefaultDbContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();

        var tenant = await context.Tenants
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(t => t.Domain == DefaultDbContextSeeding.DefaultTenantDomain);

        await Assert.That(tenant).IsNotNull();
        await Assert.That(tenant!.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(tenant.Name).IsEqualTo(AppConst.Default);
    }

    [Test]
    public async Task AnalysisDbContext_WhenSavingTenantCatalog_AllowsWriteAndQuery()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new AnalysisDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var tenant = new Tenant
        {
            Domain = "analysis-test.example",
            Name = "Analysis Test",
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var loadedTenant = await context.Tenants.SingleAsync(item => item.Id == tenant.Id);

        await Assert.That(loadedTenant.Id).IsEqualTo(tenant.Id);
        await Assert.That(loadedTenant.Name).IsEqualTo("Analysis Test");
    }

    [Test]
    public async Task TenantCatalog_WhenTenantIdIsMissing_CanSaveAndQueryTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<DefaultDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new DefaultDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var tenant = new Tenant
        {
            Domain = "catalog-test.example",
            Name = "Catalog Test",
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var loadedTenant = await context.Tenants.SingleAsync(item => item.Id == tenant.Id);

        await Assert.That(loadedTenant.Id).IsEqualTo(tenant.Id);
        await Assert.That(loadedTenant.TenantId).IsEqualTo(Guid.Empty);
    }

    [Test]
    public async Task GlobalTenantFilter_WhenQueryOmitsTenantId_ReturnsOnlyCurrentTenantRows()
    {
        var tenantA = Guid.CreateVersion7();
        var tenantB = Guid.CreateVersion7();
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var context = CreateConventionContext(connection, tenantA))
        {
            await context.Database.EnsureCreatedAsync();
            context.AddRange(
                new TenantFrameworkTestEntity { Code = "active" },
                new TenantFrameworkTestEntity { Code = "deleted", IsDeleted = true }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = CreateConventionContext(connection, tenantB))
        {
            context.Add(new TenantFrameworkTestEntity { Code = "other-tenant" });
            await context.SaveChangesAsync();

            var rows = await context.Set<TenantFrameworkTestEntity>().ToListAsync();
            await Assert.That(rows.Count).IsEqualTo(1);
            await Assert.That(rows[0].Code).IsEqualTo("other-tenant");
        }

        await using (var context = CreateConventionContext(connection, tenantA))
        {
            var rows = await context.Set<TenantFrameworkTestEntity>().ToListAsync();
            await Assert.That(rows.Count).IsEqualTo(1);
            await Assert.That(rows[0].Code).IsEqualTo("active");

            var rowsIncludingDeleted = await context
                .Set<TenantFrameworkTestEntity>()
                .IgnoreQueryFilters([ContextBase.SoftDeletionFilterName])
                .ToListAsync();

            await Assert.That(rowsIncludingDeleted.Count).IsEqualTo(2);
            var sql = context.Set<TenantFrameworkTestEntity>().ToQueryString();
            await Assert.That(sql).Contains(nameof(ITenantEntityBase.TenantId));
        }
    }

    [Test]
    public async Task TenantModel_ShouldIgnoreTenantIdAndKeepTenantVisibleToCatalogQueries()
    {
        await using var context = CreateContext();
        var tenantType = context.Model.FindEntityType(typeof(Tenant))
            ?? throw new InvalidOperationException("Tenant entity metadata was not found.");

        await Assert.That(tenantType.FindProperty(nameof(ITenantEntityBase.TenantId))).IsNull();
        var tenantFilters = tenantType.GetDeclaredQueryFilters();
        await Assert.That(tenantFilters.Any(filter => filter.Key == ContextBase.TenantFilterName)).IsFalse();
        await Assert.That(tenantFilters.Any(filter => filter.Key == ContextBase.SoftDeletionFilterName)).IsTrue();
    }

    [Test]
    public async Task TenantScopedManager_WhenTenantIdIsMissing_ThrowsImmediately()
    {
        var userContext = new TestUserContext();
        Exception? exception = null;

        try
        {
            _ = new TenantFrameworkTestEntityManager(
                CreateAppDbFactory(),
                userContext,
                NullLogger<TenantFrameworkTestEntityManager>.Instance
            );
        }
        catch (Exception caught)
        {
            exception = caught;
        }

        await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        await Assert.That(exception!.Message).Contains("TenantId");
    }

    [Test]
    public async Task TenantManager_WhenTenantIdIsMissing_DoesNotRequireTenantContext()
    {
        var manager = new TenantManager(
            CreateAppDbFactory(),
            new TestUserContext(),
            NullLogger<TenantManager>.Instance
        );

        await Assert.That(manager).IsNotNull();
    }

    [Test]
    public async Task ClaimsTransformation_WhenTenantClaimIsMissing_AddsDefaultTenantClaims()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var optionsBuilder = new DbContextOptionsBuilder<DefaultDbContext>();
        optionsBuilder.UseSqlite(connection).UseDefaultDbContextSeeding();
        await using var context = new DefaultDbContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();

        var tenant = await context.Tenants
            .SingleAsync(t => t.Domain == DefaultDbContextSeeding.DefaultTenantDomain);
        var services = new ServiceCollection();
        services.Configure<CacheOption>(_ => { });
        services.Configure<ComponentOption>(_ => { });
        services.AddMemoryCache();
        services.AddHybridCache();
        using var serviceProvider = services.BuildServiceProvider();
        var cache = new CacheService(
            serviceProvider.GetRequiredService<HybridCache>(),
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<IOptions<CacheOption>>(),
            serviceProvider.GetRequiredService<IOptions<ComponentOption>>()
        );
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, Guid.CreateVersion7().ToString())],
                authenticationType: "test"
            )
        );

        var transformed = await new LocalUserClaimsTransformation(context, cache)
            .TransformAsync(principal);

        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantId)?.Value)
            .IsEqualTo(tenant.Id.ToString());
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantType)?.Value)
            .IsEqualTo(tenant.Type.ToString());
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantName)?.Value)
            .IsEqualTo(tenant.Name);
    }

    private static DefaultDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DefaultDbContext>()
            .UseNpgsql("Host=localhost;Database=model_metadata_test;Username=postgres;Password=postgres")
            .Options;
        return new DefaultDbContext(options);
    }

    private static ConventionTestDbContext CreateConventionContext()
    {
        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        return new ConventionTestDbContext(options);
    }

    private static ConventionTestDbContext CreateConventionContext(
        SqliteConnection connection,
        Guid tenantId
    )
    {
        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ConventionTestDbContext(options, tenantId);
    }

    private static AppDbFactory CreateAppDbFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{AppConst.Default}"] =
                        "Host=localhost;Database=tenant-framework-tests;Username=test;Password=test",
                }
            )
            .Build();

        return new AppDbFactory(
            Options.Create(new ComponentOption { Database = DatabaseType.PostgreSql }),
            cache: null!,
            configuration
        );
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
        public HttpContext? HttpContext { get; set; }

        public bool IsRole(string roleName) => false;
    }

    private sealed class TenantManager(
        AppDbFactory dbContextFactory,
        IUserContext userContext,
        Microsoft.Extensions.Logging.ILogger logger
    ) : ManagerBase<DefaultDbContext, Tenant>(dbContextFactory, userContext, logger)
    {
        public override Task<bool> HasPermissionAsync(Guid id) => Task.FromResult(true);
    }

    private sealed class TenantFrameworkTestEntityManager(
        AppDbFactory dbContextFactory,
        IUserContext userContext,
        Microsoft.Extensions.Logging.ILogger logger
    ) : ManagerBase<DefaultDbContext, TenantFrameworkTestEntity>(
        dbContextFactory,
        userContext,
        logger
    )
    {
        public override Task<bool> HasPermissionAsync(Guid id) => Task.FromResult(true);
    }
}

[Index(nameof(Code), IsUnique = true)]
[Table("TenantFrameworkTestEntities")]
internal sealed class TenantFrameworkTestEntity : EntityBase
{
    public required string Code { get; set; }
}

internal sealed class ConventionTestDbContext : ContextBase
{
    public ConventionTestDbContext(DbContextOptions<ConventionTestDbContext> options, Guid? tenantId = null)
        : base(options)
    {
        SetTenantId(tenantId);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<TenantFrameworkTestEntity>();
        builder.Entity<TenantFrameworkTestEntityWithIndex>();
    }
}

[Index(nameof(TenantId), nameof(Code), IsUnique = true)]
[Table("TenantFrameworkTestEntitiesWithIndex")]
internal sealed class TenantFrameworkTestEntityWithIndex : EntityBase
{
    public required string Code { get; set; }
}
