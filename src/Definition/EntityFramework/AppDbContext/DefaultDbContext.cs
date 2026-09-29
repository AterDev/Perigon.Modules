namespace EntityFramework.AppDbContext;

/// <summary>
/// default data access for main business
/// </summary>
/// <param name = "options"></param>
public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options) : ContextBase(options)
{
    public DbSet<Article> Articles { get; set; }
    public DbSet<ArticleCategory> ArticleCategories { get; set; }
    public DbSet<ResEnvironment> ResEnvironments { get; set; }
    public DbSet<ResCategory> ResCategories { get; set; }
    public DbSet<ResGroup> ResGroups { get; set; }
    public DbSet<ResTag> ResTags { get; set; }
    public DbSet<ResDefinition> ResDefinitions { get; set; }
    public DbSet<ResDefinitionProperty> ResDefinitionProperties { get; set; }
    public DbSet<ResDefinitionPropertyMap> ResDefinitionPropertyMaps { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<UserResource> UserResources { get; set; }
    public DbSet<UserResValue> UserResValues { get; set; }
    public DbSet<UserFavoriteResource> UserFavoriteResources { get; set; }
    public DbSet<ResValue> ResValues { get; set; }
    public DbSet<ResPermission> ResPermissions { get; set; }
    public DbSet<SysConfig> SysConfigs { get; set; }
    public DbSet<SysLogs> SysLogs { get; set; }
    public DbSet<SysMenu> SysMenus { get; set; }
    public DbSet<SysMenuRole> SysMenuRoles { get; set; }
    public DbSet<SysOrganization> SysOrganizations { get; set; }
    public DbSet<SysDataScope> SysDataScopes { get; set; }
    public DbSet<SysDataScopeGroup> SysDataScopeGroups { get; set; }
    public DbSet<SysUserDataScopeGroup> SysUserDataScopeGroups { get; set; }
    public DbSet<SysUserOrganization> SysUserOrganizations { get; set; }
    public DbSet<SysRole> SysRoles { get; set; }
    public DbSet<SysUser> SysUsers { get; set; }
    public DbSet<SysUserRole> SysUserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(DefaultDbContext).Assembly);
    }
}
