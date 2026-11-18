using Entity.AuditMod;
using Entity.NotificationMod;

namespace EntityFramework.AppDbContext;

public partial class DefaultDbContext
{
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

    private void ConfigureNotificationAndAudit(ModelBuilder builder)
    {
        builder.Entity<InAppNotification>().ToTable("InAppNotifications");
        builder.Entity<InAppNotificationRecipient>(entity => {
            entity.ToTable("InAppNotificationRecipients");
            entity.HasIndex(x => new ValueTuple<object?, object?, object?>(x.TenantId, x.NotificationId, x.UserId)).IsUnique().HasDatabaseName("UX_InAppRecipient_Tenant_Notification_User").HasFilter("\"is_deleted\" = false");
            entity.HasIndex(x => new ValueTuple<object?, object?, object?, object?>(x.TenantId, x.UserId, x.IsRead, x.CreatedTime)).HasDatabaseName("IX_InAppRecipient_Tenant_User_Read_Created");
        });
        builder.Entity<NotificationSource>().ToTable("NotificationSources");
        builder.Entity<NotificationReceiver>().ToTable("NotificationReceivers");
        builder.Entity<NotificationRule>().ToTable("NotificationRules");
        builder.Entity<NotificationRuleReceiver>(entity => {
            entity.ToTable("NotificationRuleReceivers");
            entity.HasIndex(x => new ValueTuple<object?, object?, object?>(x.TenantId, x.RuleId, x.ReceiverId)).IsUnique().HasDatabaseName("UX_NotificationRuleReceiver_Tenant_Rule_Receiver").HasFilter("\"is_deleted\" = false");
        });
        builder.Entity<NotificationEvent>(entity => {
            entity.ToTable("NotificationEvents");
            entity.HasIndex(x => new ValueTuple<object?, object?, object?>(x.TenantId, x.SourceId, x.EventKey)).IsUnique().HasDatabaseName("UX_NotificationEvent_Tenant_Source_Key").HasFilter("\"is_deleted\" = false");
        });
        builder.Entity<NotificationDelivery>(entity => {
            entity.ToTable("NotificationDeliveries");
            entity.HasIndex(x => new ValueTuple<object?, object?, object?>(x.TenantId, x.EventId, x.ReceiverId)).IsUnique().HasDatabaseName("UX_NotificationDelivery_Tenant_Event_Receiver").HasFilter("\"is_deleted\" = false");
        });
        builder.Entity<AuditFlow>(entity => {
            entity.ToTable("AuditFlows");
            entity.HasIndex(x => new ValueTuple<object, object>(x.TenantId, x.FlowKey)).IsUnique().HasDatabaseName("UX_AuditFlow_Tenant_Key").HasFilter("\"is_deleted\" = false");
            entity.HasIndex(x => new ValueTuple<object?, object?>(x.TenantId, x.SceneCode)).IsUnique().HasDatabaseName("UX_AuditFlow_Tenant_EnabledScene").HasFilter("\"is_deleted\" = false AND \"is_enabled\" = true");
        });
        builder.Entity<AuditStepDefinition>(entity => {
            entity.ToTable("AuditStepDefinitions");
            entity.HasIndex(x => new ValueTuple<object?, object?, object?>(x.TenantId, x.FlowId, x.StepNo)).IsUnique().HasDatabaseName("UX_AuditStep_Tenant_Flow_Number").HasFilter("\"is_deleted\" = false");
        });
        builder.Entity<AuditRecord>(entity => {
            entity.ToTable("AuditRecords");
            entity.HasIndex(x => new ValueTuple<object?, object?, object?, object?>(x.TenantId, x.ApplicantUserId, x.SceneCode, x.BizId)).IsUnique().HasDatabaseName("UX_AuditRecord_Tenant_ActiveBusiness").HasFilter("\"is_deleted\" = false AND \"status_value\" = 0");
        });
        builder.Entity<AuditRecordDetail>(entity => {
            entity.ToTable("AuditRecordDetails");
            entity.HasIndex(x => new ValueTuple<object?, object?, object?>(x.TenantId, x.AuditRecordId, x.StepNoSnapshot)).IsUnique().HasDatabaseName("UX_AuditDetail_Tenant_Record_Number").HasFilter("\"is_deleted\" = false");
        });
        builder.Entity<AuditTask>(entity => {
            entity.ToTable("AuditTasks");
            entity.HasIndex(x => new ValueTuple<object?, object?, object?>(x.TenantId, x.AuditRecordDetailId, x.AssigneeUserId)).IsUnique().HasDatabaseName("UX_AuditTask_Tenant_Detail_User").HasFilter("\"is_deleted\" = false");
        });
        AddTenantFilters<InAppNotification>(); AddTenantFilters<InAppNotificationRecipient>();
        AddTenantFilters<NotificationSource>(); AddTenantFilters<NotificationReceiver>();
        AddTenantFilters<NotificationRule>(); AddTenantFilters<NotificationRuleReceiver>();
        AddTenantFilters<NotificationEvent>(); AddTenantFilters<NotificationDelivery>();
        AddTenantFilters<AuditFlow>(); AddTenantFilters<AuditStepDefinition>();
        AddTenantFilters<AuditRecord>(); AddTenantFilters<AuditRecordDetail>(); AddTenantFilters<AuditTask>();
    }
}

