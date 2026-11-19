using Entity.AuditMod;
using Entity.NotificationMod;

namespace EntityFramework.AppDbContext;

/// <summary>
/// PostgreSQL context for the application database.
/// </summary>
public class DefaultDbContext : DbContext
{
#pragma warning disable CS8618 // DbSet properties are initialized by the Perigon source generator.
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options)
        : base(options)
    {
    }

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

    public DbSet<InAppNotification> InAppNotifications { get; set; }
    public DbSet<InAppNotificationRecipient> InAppNotificationRecipients { get; set; }
    public DbSet<NotificationSource> NotificationSources { get; set; }
    public DbSet<NotificationReceiver> NotificationReceivers { get; set; }
    public DbSet<NotificationRule> NotificationRules { get; set; }
    public DbSet<NotificationRuleReceiver> NotificationRuleReceivers { get; set; }
    public DbSet<NotificationEvent> NotificationEvents { get; set; }
    public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
    public DbSet<AuditFlow> AuditFlows { get; set; }
    public DbSet<AuditStepDefinition> AuditStepDefinitions { get; set; }
    public DbSet<AuditRecord> AuditRecords { get; set; }
    public DbSet<AuditRecordDetail> AuditRecordDetails { get; set; }
    public DbSet<AuditTask> AuditTasks { get; set; }

#pragma warning restore CS8618

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        EnableSoftDeleteFilter<EntityBase>("SoftDelete", entity => !entity.IsDeleted);
        builder.Entity<AuditRecord>()
            .HasIndex(record => new ValueTuple<object?, object?, object?>(
                record.ApplicantUserId, record.SceneCode, record.BizId))
            .IsUnique()
            .HasFilter("\"StatusValue\" = 0");
    }
}
