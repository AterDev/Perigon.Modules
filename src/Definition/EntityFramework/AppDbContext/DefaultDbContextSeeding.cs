using Perigon.AspNetCore.Constants;

namespace EntityFramework.AppDbContext;

public static class DefaultDbContextSeeding
{
    public const string DefaultTenantDomain = "default.com";

    public static DbContextOptionsBuilder UseDefaultDbContextSeeding(
        this DbContextOptionsBuilder optionsBuilder
    )
    {
        return optionsBuilder
            .UseSeeding((context, _) => SeedDefaultTenant(context))
            .UseAsyncSeeding((context, _, cancellationToken) =>
                SeedDefaultTenantAsync(context, cancellationToken)
            );
    }

    private static void SeedDefaultTenant(DbContext context)
    {
        var tenants = context.Set<Tenant>();
        var defaultTenant = tenants
            .IgnoreQueryFilters([ContextBase.SoftDeletionFilterName])
            .SingleOrDefault(t => t.Domain == DefaultTenantDomain);
        if (defaultTenant is not null)
        {
            if (defaultTenant.IsDeleted)
            {
                defaultTenant.IsDeleted = false;
                context.SaveChanges();
            }
            return;
        }

        tenants.Add(new Tenant
        {
            Domain = DefaultTenantDomain,
            Name = AppConst.Default,
            Description = "This is default tenant, created by system.",
        });

        context.SaveChanges();
    }

    private static async Task SeedDefaultTenantAsync(
        DbContext context,
        CancellationToken cancellationToken
    )
    {
        var tenants = context.Set<Tenant>();
        var defaultTenant = await tenants
            .IgnoreQueryFilters([ContextBase.SoftDeletionFilterName])
            .SingleOrDefaultAsync(t => t.Domain == DefaultTenantDomain, cancellationToken);
        if (defaultTenant is not null)
        {
            if (defaultTenant.IsDeleted)
            {
                defaultTenant.IsDeleted = false;
                await context.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        tenants.Add(new Tenant
        {
            Domain = DefaultTenantDomain,
            Name = AppConst.Default,
            Description = "This is default tenant, created by system.",
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
