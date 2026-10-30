namespace EntityFramework.AppDbContext;

/// <summary>Tenant-aware PostgreSQL context. A null tenant is reserved for the tenant catalog.</summary>
public partial class DefaultDbContext : ContextBase
{
    public DefaultDbContext(string connectionString) : this(connectionString, null) { }
    public DefaultDbContext(string connectionString, Guid? tenantId) : base(connectionString, tenantId) { }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<ResEnvironment> ResEnvironments => Set<ResEnvironment>();
    public DbSet<ResCategory> ResCategories => Set<ResCategory>();
    public DbSet<ResGroup> ResGroups => Set<ResGroup>();
    public DbSet<ResTag> ResTags => Set<ResTag>();
    public DbSet<ResDefinition> ResDefinitions => Set<ResDefinition>();
    public DbSet<ResDefinitionProperty> ResDefinitionProperties => Set<ResDefinitionProperty>();
    public DbSet<ResDefinitionPropertyMap> ResDefinitionPropertyMaps => Set<ResDefinitionPropertyMap>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<UserResource> UserResources => Set<UserResource>();
    public DbSet<UserResValue> UserResValues => Set<UserResValue>();
    public DbSet<UserFavoriteResource> UserFavoriteResources => Set<UserFavoriteResource>();
    public DbSet<ResValue> ResValues => Set<ResValue>();
    public DbSet<ResPermission> ResPermissions => Set<ResPermission>();
    public DbSet<SysConfig> SysConfigs => Set<SysConfig>();
    public DbSet<SysLogs> SysLogs => Set<SysLogs>();
    public DbSet<SysMenu> SysMenus => Set<SysMenu>();
    public DbSet<SysMenuRole> SysMenuRoles => Set<SysMenuRole>();
    public DbSet<SysOrganization> SysOrganizations => Set<SysOrganization>();
    public DbSet<SysDataScope> SysDataScopes => Set<SysDataScope>();
    public DbSet<SysDataScopeGroup> SysDataScopeGroups => Set<SysDataScopeGroup>();
    public DbSet<SysUserDataScopeGroup> SysUserDataScopeGroups => Set<SysUserDataScopeGroup>();
    public DbSet<SysUserOrganization> SysUserOrganizations => Set<SysUserOrganization>();
    public DbSet<SysRole> SysRoles => Set<SysRole>();
    public DbSet<SysUser> SysUsers => Set<SysUser>();
    public DbSet<SysUserRole> SysUserRoles => Set<SysUserRole>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Article>(entity =>
        {
            entity.ToTable("Articles");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Title).HasColumnName("Title");
            entity.Property(x => x.Description).HasColumnName("Description");
            entity.Property(x => x.Content).HasColumnName("Content");
            entity.Property(x => x.Authors).HasColumnName("Authors");
            entity.Property(x => x.LanguageTypeValue).HasColumnName("LanguageType").HasColumnType("integer");
            entity.Property(x => x.ContentTypeValue).HasColumnName("ContentType").HasColumnType("integer");
            entity.Property(x => x.IsAudit).HasColumnName("IsAudit");
            entity.Property(x => x.IsPublic).HasColumnName("IsPublic");
            entity.Property(x => x.IsOriginal).HasColumnName("IsOriginal");
            entity.Property(x => x.UserId).HasColumnName("UserId");
            entity.Property(x => x.CatalogId).HasColumnName("CatalogId");
            entity.Property(x => x.ViewCount).HasColumnName("ViewCount");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.CatalogId).HasDatabaseName("IX_Articles_CatalogId");
            entity.HasIndex(x => new { x.TenantId, x.CatalogId })
                .HasDatabaseName("IX_Articles_TenantId_CatalogId");
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.Title }).IsUnique()
                .HasDatabaseName("IX_Articles_TenantId_UserId_Title")
                .HasFilter("\"IsDeleted\" = false");
        });
        AddTenantFilters<Article>();
        builder.Entity<ArticleCategory>(entity =>
        {
            entity.ToTable("ArticleCategories");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Level).HasColumnName("Level");
            entity.Property(x => x.ParentId).HasColumnName("ParentId");
            entity.Property(x => x.UserId).HasColumnName("UserId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.ParentId).HasDatabaseName("IX_ArticleCategories_ParentId");
            entity.HasIndex(x => new { x.TenantId, x.ParentId })
                .HasDatabaseName("IX_ArticleCategories_TenantId_ParentId");
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.Name }).IsUnique()
                .HasDatabaseName("IX_ArticleCategories_TenantId_UserId_Name")
                .HasFilter("\"IsDeleted\" = false");
        });
        AddTenantFilters<ArticleCategory>();
        builder.Entity<ResEnvironment>(entity =>
        {
            entity.ToTable("ResEnvironments");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Icon).HasColumnName("Icon");
            entity.Property(x => x.Color).HasColumnName("Color");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
        });
        AddTenantFilters<ResEnvironment>();
        builder.Entity<ResCategory>(entity =>
        {
            entity.ToTable("ResCategories");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.CatalogCode).HasColumnName("CatalogCode");
            entity.Property(x => x.Icon).HasColumnName("Icon");
            entity.Property(x => x.Color).HasColumnName("Color");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => new { x.TenantId, x.CatalogCode }).IsUnique()
                .HasDatabaseName("IX_ResCategories_TenantId_CatalogCode");
        });
        AddTenantFilters<ResCategory>();
        builder.Entity<ResGroup>(entity =>
        {
            entity.ToTable("ResGroups");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Description).HasColumnName("Description");
            entity.Property(x => x.Icon).HasColumnName("Icon");
            entity.Property(x => x.Color).HasColumnName("Color");
            entity.Property(x => x.CategoryId).HasColumnName("CategoryId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.CategoryId).HasDatabaseName("IX_ResGroups_CategoryId");
            entity.HasIndex(x => new { x.TenantId, x.CategoryId })
                .HasDatabaseName("IX_ResGroups_TenantId_CategoryId");
        });
        AddTenantFilters<ResGroup>();
        builder.Entity<ResTag>(entity =>
        {
            entity.ToTable("ResTags");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Color).HasColumnName("Color");
            entity.Property(x => x.Icon).HasColumnName("Icon");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
        });
        AddTenantFilters<ResTag>();
        builder.Entity<ResDefinition>(entity =>
        {
            entity.ToTable("ResDefinitions");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Icon).HasColumnName("Icon");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
        });
        AddTenantFilters<ResDefinition>();
        builder.Entity<ResDefinitionProperty>(entity =>
        {
            entity.ToTable("ResDefinitionProperties");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.ValueTypeValue).HasColumnName("ValueType").HasColumnType("integer");
            entity.Property(x => x.IsRequired).HasColumnName("IsRequired");
            entity.Property(x => x.MaxLength).HasColumnName("MaxLength");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => new { x.TenantId, x.Name }).IsUnique()
                .HasDatabaseName("IX_ResDefinitionProperties_TenantId_Name");
        });
        AddTenantFilters<ResDefinitionProperty>();
        builder.Entity<ResDefinitionPropertyMap>(entity =>
        {
            entity.ToTable("ResDefinitionPropertyMaps");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.DefinitionId).HasColumnName("DefinitionId");
            entity.Property(x => x.PropertyId).HasColumnName("PropertyId");
            entity.Property(x => x.Sort).HasColumnName("Sort");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.DefinitionId).HasDatabaseName("IX_ResDefinitionPropertyMaps_DefinitionId");
            entity.HasIndex(x => x.PropertyId).HasDatabaseName("IX_ResDefinitionPropertyMaps_PropertyId");
            entity.HasIndex(x => new { x.TenantId, x.DefinitionId, x.PropertyId }).IsUnique()
                .HasDatabaseName("IX_ResDefinitionPropertyMaps_TenantId_DefinitionId_PropertyId")
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(x => new { x.TenantId, x.PropertyId })
                .HasDatabaseName("IX_ResDefinitionPropertyMaps_TenantId_PropertyId");
        });
        AddTenantFilters<ResDefinitionPropertyMap>();
        builder.Entity<Resource>(entity =>
        {
            entity.ToTable("Resources");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.EnvironmentId).HasColumnName("EnvironmentId");
            entity.Property(x => x.CategoryId).HasColumnName("CategoryId");
            entity.Property(x => x.GroupId).HasColumnName("GroupId");
            entity.Property(x => x.DefinitionId).HasColumnName("DefinitionId");
            entity.Property(x => x.TagNames).HasColumnName("TagNames");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.CategoryId).HasDatabaseName("IX_Resources_CategoryId");
            entity.HasIndex(x => x.DefinitionId).HasDatabaseName("IX_Resources_DefinitionId");
            entity.HasIndex(x => x.EnvironmentId).HasDatabaseName("IX_Resources_EnvironmentId");
            entity.HasIndex(x => x.GroupId).HasDatabaseName("IX_Resources_GroupId");
            entity.HasIndex(x => new { x.TenantId, x.CategoryId })
                .HasDatabaseName("IX_Resources_TenantId_CategoryId");
            entity.HasIndex(x => new { x.TenantId, x.DefinitionId })
                .HasDatabaseName("IX_Resources_TenantId_DefinitionId");
            entity.HasIndex(x => new { x.TenantId, x.EnvironmentId })
                .HasDatabaseName("IX_Resources_TenantId_EnvironmentId");
            entity.HasIndex(x => new { x.TenantId, x.GroupId })
                .HasDatabaseName("IX_Resources_TenantId_GroupId");
        });
        AddTenantFilters<Resource>();
        builder.Entity<UserResource>(entity =>
        {
            entity.ToTable("UserResources");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.UserId).HasColumnName("UserId");
            entity.Property(x => x.DefinitionId).HasColumnName("DefinitionId");
            entity.Property(x => x.StatusValue).HasColumnName("Status").HasColumnType("integer");
            entity.Property(x => x.AuditStatusValue).HasColumnName("AuditStatus").HasColumnType("integer");
            entity.Property(x => x.ApprovedResourceId).HasColumnName("ApprovedResourceId");
            entity.Property(x => x.ReviewComment).HasColumnName("ReviewComment");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.DefinitionId).HasDatabaseName("IX_UserResources_DefinitionId");
            entity.HasIndex(x => new { x.TenantId, x.AuditStatusValue })
                .HasDatabaseName("IX_UserResources_TenantId_AuditStatus");
            entity.HasIndex(x => new { x.TenantId, x.DefinitionId })
                .HasDatabaseName("IX_UserResources_TenantId_DefinitionId");
            entity.HasIndex(x => new { x.TenantId, x.UserId })
                .HasDatabaseName("IX_UserResources_TenantId_UserId");
        });
        AddTenantFilters<UserResource>();
        builder.Entity<UserResValue>(entity =>
        {
            entity.ToTable("UserResValues");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.UserResourceId).HasColumnName("UserResourceId");
            entity.Property(x => x.DefinitionPropertyId).HasColumnName("DefinitionPropertyId");
            entity.Property(x => x.Value).HasColumnName("Value");
            entity.Property(x => x.PropertyNameSnapshot).HasColumnName("PropertyNameSnapshot");
            entity.Property(x => x.ValueTypeSnapshotValue).HasColumnName("ValueTypeSnapshot").HasColumnType("integer");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.DefinitionPropertyId)
                .HasDatabaseName("IX_UserResValues_DefinitionPropertyId");
            entity.HasIndex(x => new { x.TenantId, x.DefinitionPropertyId })
                .HasDatabaseName("IX_UserResValues_TenantId_DefinitionPropertyId");
            entity.HasIndex(x => new { x.TenantId, x.UserResourceId, x.DefinitionPropertyId }).IsUnique()
                .HasDatabaseName("IX_UserResValues_TenantId_UserResourceId_DefinitionPropertyId")
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(x => x.UserResourceId)
                .HasDatabaseName("IX_UserResValues_UserResourceId");
        });
        AddTenantFilters<UserResValue>();
        builder.Entity<UserFavoriteResource>(entity =>
        {
            entity.ToTable("UserFavoriteResources");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.UserId).HasColumnName("UserId");
            entity.Property(x => x.ResourceId).HasColumnName("ResourceId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.ResourceId)
                .HasDatabaseName("IX_UserFavoriteResources_ResourceId");
            entity.HasIndex(x => new { x.TenantId, x.ResourceId })
                .HasDatabaseName("IX_UserFavoriteResources_TenantId_ResourceId");
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.ResourceId }).IsUnique()
                .HasDatabaseName("IX_UserFavoriteResources_TenantId_UserId_ResourceId")
                .HasFilter("\"IsDeleted\" = false");
        });
        AddTenantFilters<UserFavoriteResource>();
        builder.Entity<ResValue>(entity =>
        {
            entity.ToTable("ResValues");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.ResourceId).HasColumnName("ResourceId");
            entity.Property(x => x.DefinitionPropertyId).HasColumnName("DefinitionPropertyId");
            entity.Property(x => x.Value).HasColumnName("Value");
            entity.Property(x => x.PropertyNameSnapshot).HasColumnName("PropertyNameSnapshot");
            entity.Property(x => x.ValueTypeSnapshotValue).HasColumnName("ValueTypeSnapshot").HasColumnType("integer");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.DefinitionPropertyId)
                .HasDatabaseName("IX_ResValues_DefinitionPropertyId");
            entity.HasIndex(x => x.ResourceId).HasDatabaseName("IX_ResValues_ResourceId");
            entity.HasIndex(x => new { x.TenantId, x.DefinitionPropertyId })
                .HasDatabaseName("IX_ResValues_TenantId_DefinitionPropertyId");
            entity.HasIndex(x => new { x.TenantId, x.ResourceId, x.DefinitionPropertyId }).IsUnique()
                .HasDatabaseName("IX_ResValues_TenantId_ResourceId_DefinitionPropertyId")
                .HasFilter("\"IsDeleted\" = false");
        });
        AddTenantFilters<ResValue>();
        builder.Entity<ResPermission>(entity =>
        {
            entity.ToTable("ResPermissions");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.RoleId).HasColumnName("RoleId");
            entity.Property(x => x.EnvironmentId).HasColumnName("EnvironmentId");
            entity.Property(x => x.CategoryId).HasColumnName("CategoryId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.CategoryId).HasDatabaseName("IX_ResPermissions_CategoryId");
            entity.HasIndex(x => x.EnvironmentId).HasDatabaseName("IX_ResPermissions_EnvironmentId");
            entity.HasIndex(x => new { x.TenantId, x.CategoryId })
                .HasDatabaseName("IX_ResPermissions_TenantId_CategoryId");
            entity.HasIndex(x => new { x.TenantId, x.EnvironmentId })
                .HasDatabaseName("IX_ResPermissions_TenantId_EnvironmentId");
            entity.HasIndex(x => new { x.TenantId, x.RoleId, x.EnvironmentId, x.CategoryId }).IsUnique()
                .HasDatabaseName("IX_ResPermissions_TenantId_RoleId_EnvironmentId_CategoryId");
        });
        AddTenantFilters<ResPermission>();
        builder.Entity<SysConfig>(entity =>
        {
            entity.ToTable("SysConfigs");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Key).HasColumnName("Key");
            entity.Property(x => x.Value).HasColumnName("Value");
            entity.Property(x => x.Description).HasColumnName("Description");
            entity.Property(x => x.Valid).HasColumnName("Valid");
            entity.Property(x => x.IsSystem).HasColumnName("IsSystem");
            entity.Property(x => x.GroupName).HasColumnName("GroupName");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => new { x.TenantId, x.GroupName, x.Key }).IsUnique()
                .HasDatabaseName("IX_SysConfigs_TenantId_GroupName_Key")
                .HasFilter("\"IsDeleted\" = false");
        });
        AddTenantFilters<SysConfig>();
        builder.Entity<SysLogs>(entity =>
        {
            entity.ToTable("SysLogs");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.ActionUserName).HasColumnName("ActionUserName");
            entity.Property(x => x.TargetName).HasColumnName("TargetName");
            entity.Property(x => x.Route).HasColumnName("Route");
            entity.Property(x => x.ActionTypeValue).HasColumnName("ActionType").HasColumnType("integer");
            entity.Property(x => x.Description).HasColumnName("Description");
            entity.Property(x => x.SysUserId).HasColumnName("SysUserId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.SysUserId).HasDatabaseName("IX_SysLogs_SysUserId");
            entity.HasIndex(x => new { x.TenantId, x.ActionTypeValue, x.CreatedTime })
                .HasDatabaseName("IX_SysLogs_TenantId_ActionType_CreatedTime");
            entity.HasIndex(x => new { x.TenantId, x.ActionUserName, x.CreatedTime })
                .HasDatabaseName("IX_SysLogs_TenantId_ActionUserName_CreatedTime");
            entity.HasIndex(x => new { x.TenantId, x.CreatedTime })
                .HasDatabaseName("IX_SysLogs_TenantId_CreatedTime");
            entity.HasIndex(x => new { x.TenantId, x.SysUserId })
                .HasDatabaseName("IX_SysLogs_TenantId_SysUserId");
        });
        AddTenantFilters<SysLogs>();
        builder.Entity<SysMenu>(entity =>
        {
            entity.ToTable("SysMenus");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Path).HasColumnName("Path");
            entity.Property(x => x.Icon).HasColumnName("Icon");
            entity.Property(x => x.ParentId).HasColumnName("ParentId");
            entity.Property(x => x.IsValid).HasColumnName("IsValid");
            entity.Property(x => x.AccessCode).HasColumnName("AccessCode");
            entity.Property(x => x.MenuTypeValue).HasColumnName("MenuType").HasColumnType("integer");
            entity.Property(x => x.Sort).HasColumnName("Sort");
            entity.Property(x => x.Hidden).HasColumnName("Hidden");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => new { x.TenantId, x.AccessCode }).IsUnique()
                .HasDatabaseName("IX_SysMenus_TenantId_AccessCode")
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(x => x.ParentId).HasDatabaseName("IX_SysMenus_ParentId");
            entity.HasIndex(x => new { x.TenantId, x.ParentId })
                .HasDatabaseName("IX_SysMenus_TenantId_ParentId");
        });
        AddTenantFilters<SysMenu>();
        builder.Entity<SysMenuRole>(entity =>
        {
            entity.ToTable("SysMenuRoles");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.MenuId).HasColumnName("MenuId");
            entity.Property(x => x.RoleId).HasColumnName("RoleId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.MenuId).HasDatabaseName("IX_SysMenuRoles_MenuId");
            entity.HasIndex(x => x.RoleId).HasDatabaseName("IX_SysMenuRoles_RoleId");
            entity.HasIndex(x => new { x.TenantId, x.MenuId })
                .HasDatabaseName("IX_SysMenuRoles_TenantId_MenuId");
            entity.HasIndex(x => new { x.TenantId, x.RoleId, x.MenuId }).IsUnique()
                .HasDatabaseName("IX_SysMenuRoles_TenantId_RoleId_MenuId")
                .HasFilter("\"IsDeleted\" = false");
        });
        AddTenantFilters<SysMenuRole>();
        builder.Entity<SysOrganization>(entity =>
        {
            entity.ToTable("SysOrganizations");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.ParentId).HasColumnName("ParentId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.ParentId).HasDatabaseName("IX_SysOrganizations_ParentId");
            entity.HasIndex(x => new { x.TenantId, x.Name })
                .HasDatabaseName("IX_SysOrganizations_TenantId_Name");
            entity.HasIndex(x => new { x.TenantId, x.ParentId })
                .HasDatabaseName("IX_SysOrganizations_TenantId_ParentId");
        });
        AddTenantFilters<SysOrganization>();
        builder.Entity<SysDataScope>(entity =>
        {
            entity.ToTable("SysDataScopes");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.ResourceCode).HasColumnName("ResourceCode");
            entity.Property(x => x.TargetIds).HasColumnName("TargetIds");
            entity.Property(x => x.ScopeTypeValue).HasColumnName("ScopeType").HasColumnType("integer");
            entity.Property(x => x.GroupId).HasColumnName("GroupId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.GroupId).HasDatabaseName("IX_SysDataScopes_GroupId");
            entity.HasIndex(x => new { x.TenantId, x.GroupId })
                .HasDatabaseName("IX_SysDataScopes_TenantId_GroupId");
            entity.HasIndex(x => new { x.TenantId, x.Name })
                .HasDatabaseName("IX_SysDataScopes_TenantId_Name");
        });
        AddTenantFilters<SysDataScope>();
        builder.Entity<SysDataScopeGroup>(entity =>
        {
            entity.ToTable("SysDataScopeGroups");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Description).HasColumnName("Description");
            entity.Property(x => x.IsEnabled).HasColumnName("IsEnabled").HasDefaultSql("true");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => new { x.TenantId, x.Name })
                .HasDatabaseName("IX_SysDataScopeGroups_TenantId_Name");
        });
        AddTenantFilters<SysDataScopeGroup>();
        builder.Entity<SysUserDataScopeGroup>(entity =>
        {
            entity.ToTable("SysUserDataScopeGroups");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.UserId).HasColumnName("UserId");
            entity.Property(x => x.DataScopeGroupId).HasColumnName("DataScopeGroupId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.DataScopeGroupId)
                .HasDatabaseName("IX_SysUserDataScopeGroups_DataScopeGroupId");
            entity.HasIndex(x => new { x.TenantId, x.DataScopeGroupId })
                .HasDatabaseName("IX_SysUserDataScopeGroups_TenantId_DataScopeGroupId");
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.DataScopeGroupId }).IsUnique()
                .HasDatabaseName("IX_SysUserDataScopeGroups_TenantId_UserId_DataScopeGroupId")
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(x => x.UserId).HasDatabaseName("IX_SysUserDataScopeGroups_UserId");
        });
        AddTenantFilters<SysUserDataScopeGroup>();
        builder.Entity<SysUserOrganization>(entity =>
        {
            entity.ToTable("SysUserOrganizations");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.UserId).HasColumnName("UserId");
            entity.Property(x => x.OrganizationId).HasColumnName("OrganizationId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.OrganizationId)
                .HasDatabaseName("IX_SysUserOrganizations_OrganizationId");
            entity.HasIndex(x => new { x.TenantId, x.OrganizationId })
                .HasDatabaseName("IX_SysUserOrganizations_TenantId_OrganizationId");
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.OrganizationId }).IsUnique()
                .HasDatabaseName("IX_SysUserOrganizations_TenantId_UserId_OrganizationId")
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(x => x.UserId).HasDatabaseName("IX_SysUserOrganizations_UserId");
        });
        AddTenantFilters<SysUserOrganization>();
        builder.Entity<SysRole>(entity =>
        {
            entity.ToTable("SysRoles");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.NameValue).HasColumnName("NameValue");
            entity.Property(x => x.IsSystem).HasColumnName("IsSystem");
            entity.Property(x => x.Icon).HasColumnName("Icon");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => new { x.TenantId, x.Name })
                .HasDatabaseName("IX_SysRoles_TenantId_Name");
            entity.HasIndex(x => new { x.TenantId, x.NameValue }).IsUnique()
                .HasDatabaseName("IX_SysRoles_TenantId_NameValue")
                .HasFilter("\"IsDeleted\" = false");
        });
        AddTenantFilters<SysRole>();
        builder.Entity<SysUser>(entity =>
        {
            entity.ToTable("SysUsers");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.UserName).HasColumnName("UserName");
            entity.Property(x => x.RealName).HasColumnName("RealName");
            entity.Property(x => x.Email).HasColumnName("Email");
            entity.Property(x => x.EmailConfirmed).HasColumnName("EmailConfirmed");
            entity.Property(x => x.PasswordHash).HasColumnName("PasswordHash");
            entity.Property(x => x.PasswordSalt).HasColumnName("PasswordSalt");
            entity.Property(x => x.PhoneNumber).HasColumnName("PhoneNumber");
            entity.Property(x => x.PhoneNumberConfirmed).HasColumnName("PhoneNumberConfirmed");
            entity.Property(x => x.TwoFactorEnabled).HasColumnName("TwoFactorEnabled");
            entity.Property(x => x.LockoutEnd).HasColumnName("LockoutEnd");
            entity.Property(x => x.LockoutEnabled).HasColumnName("LockoutEnabled");
            entity.Property(x => x.AccessFailedCount).HasColumnName("AccessFailedCount");
            entity.Property(x => x.LastLoginTime).HasColumnName("LastLoginTime");
            entity.Property(x => x.LastPwdEditTime).HasColumnName("LastPwdEditTime");
            entity.Property(x => x.RetryCount).HasColumnName("RetryCount");
            entity.Property(x => x.Avatar).HasColumnName("Avatar");
            entity.Property(x => x.SexValue).HasColumnName("Sex").HasColumnType("integer");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique()
                .HasDatabaseName("IX_SysUsers_TenantId_Email")
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(x => new { x.TenantId, x.PhoneNumber }).IsUnique()
                .HasDatabaseName("IX_SysUsers_TenantId_PhoneNumber")
                .HasFilter("\"IsDeleted\" = false");
        });
        AddTenantFilters<SysUser>();
        builder.Entity<SysUserRole>(entity =>
        {
            entity.ToTable("SysUserRoles");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.UserId).HasColumnName("UserId");
            entity.Property(x => x.RoleId).HasColumnName("RoleId");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.Property(x => x.TenantId).HasColumnName("TenantId");
            entity.HasIndex(x => x.RoleId).HasDatabaseName("IX_SysUserRoles_RoleId");
            entity.HasIndex(x => new { x.TenantId, x.RoleId })
                .HasDatabaseName("IX_SysUserRoles_TenantId_RoleId");
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.RoleId }).IsUnique()
                .HasDatabaseName("IX_SysUserRoles_TenantId_UserId_RoleId")
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(x => x.UserId).HasDatabaseName("IX_SysUserRoles_UserId");
        });
        AddTenantFilters<SysUserRole>();
        builder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Description).HasColumnName("Description");
            entity.Property(x => x.DbConnectionString).HasColumnName("DbConnectionString");
            entity.Property(x => x.AnalysisConnectionString).HasColumnName("AnalysisConnectionString");
            entity.Property(x => x.Domain).HasColumnName("Domain");
            entity.Property(x => x.TypeValue).HasColumnName("Type").HasColumnType("integer");
            entity.Property(x => x.Disabled).HasColumnName("Disabled");
            entity.Property(x => x.CreatedTime).HasColumnName("CreatedTime");
            entity.Property(x => x.UpdatedTime).HasColumnName("UpdatedTime");
            entity.Property(x => x.IsDeleted).HasColumnName("IsDeleted");
            entity.HasIndex(x => x.Domain).IsUnique()
                .HasDatabaseName("IX_Tenants_Domain");
        });
        HasQueryFilter<Tenant>("SoftDelete", entity => !entity.IsDeleted);

        // Preserve the foreign keys and delete behavior of the existing PostgreSQL schema.
        builder.Entity<ArticleCategory>().HasOne<ArticleCategory>().WithMany().HasForeignKey(x => x.ParentId).HasConstraintName("FK_ArticleCategories_ArticleCategories_ParentId");
        builder.Entity<SysMenu>().HasOne<SysMenu>().WithMany().HasForeignKey(x => x.ParentId).HasConstraintName("FK_SysMenus_SysMenus_ParentId");
        builder.Entity<SysOrganization>().HasOne<SysOrganization>().WithMany().HasForeignKey(x => x.ParentId).HasConstraintName("FK_SysOrganizations_SysOrganizations_ParentId");
        builder.Entity<Article>().HasOne<ArticleCategory>().WithMany().HasForeignKey(x => x.CatalogId).HasConstraintName("FK_Articles_ArticleCategories_CatalogId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<ResGroup>().HasOne<ResCategory>().WithMany().HasForeignKey(x => x.CategoryId).HasConstraintName("FK_ResGroups_ResCategories_CategoryId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<ResDefinitionPropertyMap>().HasOne<ResDefinitionProperty>().WithMany().HasForeignKey(x => x.PropertyId).HasConstraintName("FK_ResDefinitionPropertyMaps_ResDefinitionProperties_PropertyId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<ResDefinitionPropertyMap>().HasOne<ResDefinition>().WithMany().HasForeignKey(x => x.DefinitionId).HasConstraintName("FK_ResDefinitionPropertyMaps_ResDefinitions_DefinitionId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<UserResource>().HasOne<ResDefinition>().WithMany().HasForeignKey(x => x.DefinitionId).HasConstraintName("FK_UserResources_ResDefinitions_DefinitionId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Restrict);
        builder.Entity<ResPermission>().HasOne<ResCategory>().WithMany().HasForeignKey(x => x.CategoryId).HasConstraintName("FK_ResPermissions_ResCategories_CategoryId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<ResPermission>().HasOne<ResEnvironment>().WithMany().HasForeignKey(x => x.EnvironmentId).HasConstraintName("FK_ResPermissions_ResEnvironments_EnvironmentId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysDataScope>().HasOne<SysDataScopeGroup>().WithMany().HasForeignKey(x => x.GroupId).HasConstraintName("FK_SysDataScopes_SysDataScopeGroups_GroupId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysMenuRole>().HasOne<SysMenu>().WithMany().HasForeignKey(x => x.MenuId).HasConstraintName("FK_SysMenuRoles_SysMenus_MenuId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysMenuRole>().HasOne<SysRole>().WithMany().HasForeignKey(x => x.RoleId).HasConstraintName("FK_SysMenuRoles_SysRoles_RoleId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysLogs>().HasOne<SysUser>().WithMany().HasForeignKey(x => x.SysUserId).HasConstraintName("FK_SysLogs_SysUsers_SysUserId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysUserDataScopeGroup>().HasOne<SysDataScopeGroup>().WithMany().HasForeignKey(x => x.DataScopeGroupId).HasConstraintName("FK_SysUserDataScopeGroups_SysDataScopeGroups_DataScopeGroupId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysUserDataScopeGroup>().HasOne<SysUser>().WithMany().HasForeignKey(x => x.UserId).HasConstraintName("FK_SysUserDataScopeGroups_SysUsers_UserId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysUserOrganization>().HasOne<SysOrganization>().WithMany().HasForeignKey(x => x.OrganizationId).HasConstraintName("FK_SysUserOrganizations_SysOrganizations_OrganizationId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysUserOrganization>().HasOne<SysUser>().WithMany().HasForeignKey(x => x.UserId).HasConstraintName("FK_SysUserOrganizations_SysUsers_UserId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysUserRole>().HasOne<SysRole>().WithMany().HasForeignKey(x => x.RoleId).HasConstraintName("FK_SysUserRoles_SysRoles_RoleId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<SysUserRole>().HasOne<SysUser>().WithMany().HasForeignKey(x => x.UserId).HasConstraintName("FK_SysUserRoles_SysUsers_UserId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<Resource>().HasOne<ResCategory>().WithMany().HasForeignKey(x => x.CategoryId).HasConstraintName("FK_Resources_ResCategories_CategoryId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<Resource>().HasOne<ResDefinition>().WithMany().HasForeignKey(x => x.DefinitionId).HasConstraintName("FK_Resources_ResDefinitions_DefinitionId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<Resource>().HasOne<ResEnvironment>().WithMany().HasForeignKey(x => x.EnvironmentId).HasConstraintName("FK_Resources_ResEnvironments_EnvironmentId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<Resource>().HasOne<ResGroup>().WithMany().HasForeignKey(x => x.GroupId).HasConstraintName("FK_Resources_ResGroups_GroupId");
        builder.Entity<UserResValue>().HasOne<ResDefinitionProperty>().WithMany().HasForeignKey(x => x.DefinitionPropertyId).HasConstraintName("FK_UserResValues_ResDefinitionProperties_DefinitionPropertyId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Restrict);
        builder.Entity<UserResValue>().HasOne<UserResource>().WithMany().HasForeignKey(x => x.UserResourceId).HasConstraintName("FK_UserResValues_UserResources_UserResourceId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<ResValue>().HasOne<ResDefinitionProperty>().WithMany().HasForeignKey(x => x.DefinitionPropertyId).HasConstraintName("FK_ResValues_ResDefinitionProperties_DefinitionPropertyId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<ResValue>().HasOne<Resource>().WithMany().HasForeignKey(x => x.ResourceId).HasConstraintName("FK_ResValues_Resources_ResourceId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
        builder.Entity<UserFavoriteResource>().HasOne<Resource>().WithMany().HasForeignKey(x => x.ResourceId).HasConstraintName("FK_UserFavoriteResources_Resources_ResourceId").OnDelete(Perigon.PostgreSQL.Metadata.ReferentialAction.Cascade);
    }
}
