using Entity;
using Entity.AuditMod;
using Entity.NotificationMod;
using Entity.CMSMod;
using Entity.ResourceMod;
using Entity.SystemMod;
using Perigon.PostgreSQL;
using System.Diagnostics.CodeAnalysis;

namespace EntityFramework.AppDbContext;

/// <summary>
/// PostgreSQL context for the application database.
/// </summary>
public class DefaultDbContext : DbContext
{
    public DefaultDbContext(string connectionString)
        : base(options => options.UsePostgres(connectionString))
    {
    }

    public new DbSet<TEntity> Set<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
        where TEntity : class
    {
        return base.Set<TEntity>();
    }

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

    public DbSet<InAppNotification> InAppNotifications => Set<InAppNotification>();
    public DbSet<InAppNotificationRecipient> InAppNotificationRecipients => Set<InAppNotificationRecipient>();
    public DbSet<NotificationSource> NotificationSources => Set<NotificationSource>();
    public DbSet<NotificationReceiver> NotificationReceivers => Set<NotificationReceiver>();
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<NotificationRuleReceiver> NotificationRuleReceivers => Set<NotificationRuleReceiver>();
    public DbSet<NotificationEvent> NotificationEvents => Set<NotificationEvent>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();
    public DbSet<AuditFlow> AuditFlows => Set<AuditFlow>();
    public DbSet<AuditStepDefinition> AuditStepDefinitions => Set<AuditStepDefinition>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();
    public DbSet<AuditRecordDetail> AuditRecordDetails => Set<AuditRecordDetail>();
    public DbSet<AuditTask> AuditTasks => Set<AuditTask>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        EnableSoftDeleteFilter<EntityBase>("SoftDelete", entity => !entity.IsDeleted);
    }
}
