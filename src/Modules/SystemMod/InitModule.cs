using Entity;
using EntityFramework.AppDbFactory;
using Perigon.AspNetCore.Options;
using Share.Services;
using Share.Serialization;
using SystemMod.Managers;

namespace SystemMod;

public class InitModule
{
    /// <summary>
    /// Initializes module data with a context explicitly bound to each tenant.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider provider)
    {
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        var catalogContext = provider.GetRequiredService<DefaultDbContext>();
        var dbContextFactory = provider.GetRequiredService<AppDbFactory>();
        var logger = loggerFactory.CreateLogger<InitModule>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var cache = provider.GetRequiredService<CacheService>();
        var tenantService = provider.GetRequiredService<TenantService>();
        var appJsonSerializer = provider.GetRequiredService<IAppJsonSerializer>();

        try
        {
            var tenants = await catalogContext.Tenants
                .ToListAsync();
            if (tenants.Count == 0)
            {
                throw new InvalidOperationException("No tenant was found after database seeding.");
            }

            foreach (var tenant in tenants)
            {
                // AppDbFactory selects an independent tenant connection synchronously
                // from this cache before creating the tenant context.
                tenantService.SetCache(tenant);

                await using var context = dbContextFactory.CreateDbContext(tenant.Id);
                if (!await context.SysUsers.AnyAsync())
                {
                    logger.LogInformation("⛏️ Start init [System] Module for tenant {TenantId}", tenant.Id);
                    await InitTenantAdminAccountAsync(context, tenant);
                }

                if (!await context.SysConfigs.AnyAsync())
                {
                    await InitConfigAsync(context, configuration, logger, appJsonSerializer);
                }

                await InitCacheAsync(context, cache, tenant.Id, logger);
            }

            logger.LogInformation("✅ Database and cache check completed for {TenantCount} tenants", tenants.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize system configuration");
            throw;
        }
    }

    private static async Task InitTenantAdminAccountAsync(
        DefaultDbContext context,
        Tenant tenant
    )
    {
        var defaultPassword = "Perigon.2026";
        var superRole = new SysRole
        {
            Name = WebConst.SuperAdmin,
            NameValue = WebConst.SuperAdmin,
            TenantId = tenant.Id,
            IsSystem = true,
        };

        var adminRole = new SysRole
        {
            Name = WebConst.AdminUser,
            NameValue = WebConst.AdminUser,
            TenantId = tenant.Id,
            IsSystem = true,
        };
        var salt = HashCrypto.BuildSalt();
        var adminUser = new SysUser
        {
            UserName = "admin",
            Email = $"admin@{tenant.Domain}",
            PasswordSalt = salt,
            PasswordHash = HashCrypto.GeneratePwd(defaultPassword, salt),
            TenantId = tenant.Id,
        };

        await context.TransactionAsync(async cancellationToken =>
        {
            await context.SysRoles.BulkInsertAsync([superRole, adminRole], cancellationToken: cancellationToken);
            await context.SysUsers.InsertAsync(adminUser, cancellationToken);
            await context.SysUserRoles.BulkInsertAsync([
                new SysUserRole { UserId = adminUser.Id, RoleId = superRole.Id, TenantId = tenant.Id },
                new SysUserRole { UserId = adminUser.Id, RoleId = adminRole.Id, TenantId = tenant.Id },
            ], cancellationToken: cancellationToken);
        });

        Console.WriteLine($"✨ Created admin for {tenant.Domain} : {adminUser.Email}/{defaultPassword}");
    }

    private static async Task InitConfigAsync(
        DefaultDbContext context,
        IConfiguration configuration,
        ILogger logger,
        IAppJsonSerializer appJsonSerializer
    )
    {
        var initConfig = SysConfig.NewSysConfig(
            WebConst.SystemGroup,
            WebConst.IsInit,
            "true"
        );
        initConfig.TenantId = context.RequiredTenantId;

        var loginSecurityPolicy =
            configuration.GetSection(WebConst.LoginSecurityPolicy).Get<LoginSecurityPolicyOption>()
            ?? new LoginSecurityPolicyOption();

        var loginSecurityPolicyConfig = SysConfig.NewSysConfig(
            WebConst.SystemGroup,
            WebConst.LoginSecurityPolicy,
            appJsonSerializer.Serialize(loginSecurityPolicy)
        );
        loginSecurityPolicyConfig.TenantId = context.RequiredTenantId;

        await context.SysConfigs.BulkInsertAsync([loginSecurityPolicyConfig, initConfig]);
        logger.LogInformation("写入登录安全策略成功");
    }

    private static async Task InitCacheAsync(
        DefaultDbContext context,
        CacheService cache,
        Guid tenantId,
        ILogger logger
    )
    {
        logger.LogInformation("加载租户 {TenantId} 配置缓存", tenantId);
        var securityPolicyConfig = await context
            .SysConfigs
            .Where(c => c.Key == WebConst.LoginSecurityPolicy)
            .Where(c => c.GroupName == WebConst.SystemGroup)
            .FirstOrDefaultAsync();
        var securityPolicy = securityPolicyConfig?.Value;

        if (securityPolicy != null)
        {
            await cache.SetValueAsync(
                SysConfigManager.GetLoginSecurityPolicyCacheKey(tenantId),
                securityPolicy,
                null
            );
        }
    }
}
