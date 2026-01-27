namespace EntityFramework.AppDbContext;
/// <summary>
/// default data access for main business
/// </summary>
/// <param name = "options"></param>
public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options) : ContextBase(options)
{
    public DbSet<Article> Articles { get; set; }
    public DbSet<ArticleCategory> ArticleCategories { get; set; }
    public DbSet<SystemConfig> SystemConfigs { get; set; }
    public DbSet<SystemLogs> SystemLogs { get; set; }
    public DbSet<SystemMenu> SystemMenus { get; set; }
    public DbSet<SystemMenuRole> SystemMenuRoles { get; set; }
    public DbSet<SystemOrganization> SystemOrganizations { get; set; }
    public DbSet<SystemPermission> SystemPermissions { get; set; }
    public DbSet<SystemPermissionGroup> SystemPermissionGroups { get; set; }
    public DbSet<SystemRole> SystemRoles { get; set; }
    public DbSet<SystemUser> SystemUsers { get; set; }
    public DbSet<SystemUserRole> SystemUserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}