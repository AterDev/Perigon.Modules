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
    public DbSet<SysRole> SysRoles { get; set; }
    public DbSet<SysUser> SysUsers { get; set; }
    public DbSet<SysUserRole> SysUserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SysConfig>().ToTable("SystemConfigs");
        builder.Entity<SysLogs>().ToTable("SystemLogs");
        builder.Entity<SysMenu>().ToTable("SystemMenus");
        builder.Entity<SysMenuRole>().ToTable("SystemMenuRoles");
        builder.Entity<SysOrganization>().ToTable("SystemOrganizations");
        builder.Entity<SysDataScope>().ToTable("SystemPermissions");
        builder.Entity<SysDataScopeGroup>().ToTable("SystemPermissionGroups");
        builder.Entity<SysRole>().ToTable("SystemRoles");
        builder.Entity<SysUser>().ToTable("SystemUsers");
        builder.Entity<SysUserRole>().ToTable("SystemUserRoles");

        builder.Entity<UserResource>()
            .HasOne(resource => resource.Definition)
            .WithMany()
            .HasForeignKey(resource => resource.DefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<UserResValue>()
            .HasOne(value => value.UserResource)
            .WithMany(resource => resource.Values)
            .HasForeignKey(value => value.UserResourceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<UserResValue>()
            .HasOne(value => value.DefinitionProperty)
            .WithMany()
            .HasForeignKey(value => value.DefinitionPropertyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<UserFavoriteResource>()
            .HasOne(favorite => favorite.Resource)
            .WithMany()
            .HasForeignKey(favorite => favorite.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SysUserDataScopeGroup>()
            .HasOne(link => link.User)
            .WithMany(user => user.DataScopeGroups)
            .HasForeignKey(link => link.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<SysUserDataScopeGroup>()
            .HasOne(link => link.DataScopeGroup)
            .WithMany(group => group.Users)
            .HasForeignKey(link => link.DataScopeGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SysDataScopeGroup>()
            .Property(group => group.IsEnabled)
            .HasDefaultValue(true);

        var targetIds = builder.Entity<SysDataScope>().Property(scope => scope.TargetIds);
        if (Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            targetIds.HasDefaultValueSql("ARRAY[]::uuid[]");
        }
        else if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
        {
            targetIds.HasDefaultValueSql("N'[]'");
        }

        builder.Entity<SysMenu>()
            .HasMany(menu => menu.SysRoles)
            .WithMany(role => role.SysMenus)
            .UsingEntity<Dictionary<string, object>>(
                "SystemMenuSystemRole",
                role => role.HasOne<SysRole>().WithMany().HasForeignKey("SystemRolesId"),
                menu => menu.HasOne<SysMenu>().WithMany().HasForeignKey("SystemMenusId"),
                join =>
                {
                    join.HasKey("SystemMenusId", "SystemRolesId");
                    join.ToTable("SystemMenuSystemRole");
                });

        builder.Entity<SysUser>()
            .HasMany(user => user.SysRoles)
            .WithMany(role => role.Users)
            .UsingEntity<Dictionary<string, object>>(
                "SystemRoleSystemUser",
                role => role.HasOne<SysRole>().WithMany().HasForeignKey("SystemRolesId"),
                user => user.HasOne<SysUser>().WithMany().HasForeignKey("UsersId"),
                join =>
                {
                    join.HasKey("SystemRolesId", "UsersId");
                    join.ToTable("SystemRoleSystemUser");
                });

        builder.Entity<SysOrganization>()
            .HasMany(organization => organization.Users)
            .WithMany(user => user.SysOrganizations)
            .UsingEntity<Dictionary<string, object>>(
                "SystemOrganizationSystemUser",
                user => user.HasOne<SysUser>().WithMany().HasForeignKey("UsersId"),
                organization => organization.HasOne<SysOrganization>().WithMany().HasForeignKey("SystemOrganizationsId"),
                join =>
                {
                    join.HasKey("SystemOrganizationsId", "UsersId");
                    join.ToTable("SystemOrganizationSystemUser");
                });
    }
}
