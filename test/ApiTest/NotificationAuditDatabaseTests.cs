using Entity.AuditMod;
using Entity.NotificationMod;
using EntityFramework.AppDbContext;
using Perigon.AspNetCore.Constants;
using Perigon.PostgreSQL;

namespace ApiTest;

/// <summary>
/// Database contract tests for the notification and audit modules. The tests
/// use tenant-scoped PostgreSQL contexts so they exercise the same query
/// filters and update paths used by the managers.
/// </summary>
public sealed class NotificationAuditDatabaseTests
{
    private const string CustomAuditSceneCode = "business.change-event";

    private static string ConnectionString =>
        GlobalHooks.App?.GetConnectionStringAsync(AppConst.Default).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("The AppHost PostgreSQL connection is unavailable.");

    [Before(Class)]
    public static async Task WaitForPostgreSqlAsync()
    {
        if (GlobalHooks.NotificationService is not null)
        {
            await GlobalHooks.NotificationService
                .WaitForResourceAsync("ApiService", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(60));
        }
    }

    [Test]
    public async Task InAppNotifications_ShouldIsolateTenantsAndReadStatePerUser()
    {
        Guid tenantA = Guid.CreateVersion7();
        Guid tenantB = Guid.CreateVersion7();
        Guid userA = Guid.CreateVersion7();
        Guid userB = Guid.CreateVersion7();
        Guid notificationA = Guid.CreateVersion7();
        Guid notificationB = Guid.CreateVersion7();

        try
        {
            await using (var context = new DefaultDbContext(ConnectionString, tenantA))
            {
                await context.InAppNotifications.InsertAsync(new InAppNotification
                {
                    Id = notificationA,
                    TenantId = tenantA,
                    Type = "announcement",
                    Title = "tenant-a-announcement",
                    Content = "tenant-a-content",
                    IsPublished = true,
                    PublishedTime = DateTimeOffset.UtcNow
                });
                await context.InAppNotificationRecipients.InsertManyReturningAsync(
                [
                    new InAppNotificationRecipient
                    {
                        TenantId = tenantA,
                        NotificationId = notificationA,
                        UserId = userA
                    },
                    new InAppNotificationRecipient
                    {
                        TenantId = tenantA,
                        NotificationId = notificationA,
                        UserId = userB
                    }
                ]);
            }

            await using (var context = new DefaultDbContext(ConnectionString, tenantB))
            {
                await context.InAppNotifications.InsertAsync(new InAppNotification
                {
                    Id = notificationB,
                    TenantId = tenantB,
                    Type = "message",
                    Title = "tenant-b-message",
                    Content = "tenant-b-content",
                    IsPublished = true,
                    PublishedTime = DateTimeOffset.UtcNow
                });
                await context.InAppNotificationRecipients.InsertAsync(new InAppNotificationRecipient
                {
                    TenantId = tenantB,
                    NotificationId = notificationB,
                    UserId = userA
                });
            }

            await using (var context = new DefaultDbContext(ConnectionString, tenantA))
            {
                List<InAppNotification> notifications = await context.InAppNotifications.ToListAsync();
                await Assert.That(notifications.Select(item => item.Id)).IsEquivalentTo([notificationA]);

                List<InAppNotificationRecipient> userRows = await context.InAppNotificationRecipients
                    .Where(item => item.NotificationId == notificationA && item.UserId == userA)
                    .ToListAsync();
                await Assert.That(userRows.Count).IsEqualTo(1);
                await Assert.That(userRows[0].IsRead).IsFalse();

                DateTimeOffset readTime = DateTimeOffset.UtcNow;
                int changed = await context.InAppNotificationRecipients
                    .Where(item => item.NotificationId == notificationA && item.UserId == userA)
                    .ExecuteUpdateAsync(update => update
                        .Set(item => item.IsRead, true)
                        .Set(item => item.ReadTime, readTime));
                await Assert.That(changed).IsEqualTo(1);

                InAppNotificationRecipient userBRow = await context.InAppNotificationRecipients
                    .Where(item => item.NotificationId == notificationA && item.UserId == userB)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("User B recipient row was not found.");
                await Assert.That(userBRow.IsRead).IsFalse();

                int crossTenantUpdate = await context.InAppNotificationRecipients
                    .Where(item => item.NotificationId == notificationB)
                    .ExecuteUpdateAsync(update => update.Set(item => item.IsRead, true));
                await Assert.That(crossTenantUpdate).IsEqualTo(0);
            }

            await using (var context = new DefaultDbContext(ConnectionString, tenantB))
            {
                InAppNotificationRecipient row = await context.InAppNotificationRecipients
                    .Where(item => item.NotificationId == notificationB)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("Tenant B recipient row was not found.");
                await Assert.That(row.IsRead).IsFalse();
            }
        }
        finally
        {
            await SoftDeleteNotificationRowsAsync(tenantA, notificationA);
            await SoftDeleteNotificationRowsAsync(tenantB, notificationB);
        }
    }

    [Test]
    public async Task AuditRows_ShouldIsolateTenantsAndPersistDecisionState()
    {
        Guid tenantA = Guid.CreateVersion7();
        Guid tenantB = Guid.CreateVersion7();
        Guid applicant = Guid.CreateVersion7();
        Guid auditorA = Guid.CreateVersion7();
        Guid auditorB = Guid.CreateVersion7();
        Guid flowA = Guid.CreateVersion7();
        Guid flowB = Guid.CreateVersion7();
        Guid recordA = Guid.CreateVersion7();
        Guid detailA = Guid.CreateVersion7();
        Guid taskA = Guid.CreateVersion7();
        Guid taskB = Guid.CreateVersion7();

        try
        {
            await using (var context = new DefaultDbContext(ConnectionString, tenantA))
            {
                await context.AuditFlows.InsertAsync(new AuditFlow
                {
                    Id = flowA,
                    TenantId = tenantA,
                    FlowKey = "audit-isolation-flow",
                    SceneCode = CustomAuditSceneCode,
                    Name = "Tenant A flow",
                    IsEnabled = true
                });
                await context.AuditStepDefinitions.InsertAsync(new AuditStepDefinition
                {
                    TenantId = tenantA,
                    FlowId = flowA,
                    StepNo = 1,
                    Name = "Approve",
                    AuditorUserIds = [auditorA, auditorB],
                    AuditMode = AuditMode.All
                });
                await context.AuditRecords.InsertAsync(new AuditRecord
                {
                    Id = recordA,
                    TenantId = tenantA,
                    FlowId = flowA,
                    FlowNameSnapshot = "Tenant A flow",
                    SceneCode = CustomAuditSceneCode,
                    BizId = "biz-a",
                    ApplicantUserId = applicant,
                    ApplicantNameSnapshot = "Applicant A",
                    ApplyReason = "database contract",
                    Status = AuditRecordStatus.InProgress,
                    CurrentStepNo = 1
                });
                await context.AuditRecordDetails.InsertAsync(new AuditRecordDetail
                {
                    Id = detailA,
                    TenantId = tenantA,
                    AuditRecordId = recordA,
                    StepDefinitionId = Guid.CreateVersion7(),
                    StepNoSnapshot = 1,
                    StepNameSnapshot = "Approve",
                    AuditModeSnapshot = AuditMode.All,
                    Status = AuditStepStatus.Pending,
                    StartedTime = DateTimeOffset.UtcNow
                });
                await context.AuditTasks.InsertManyReturningAsync(
                [
                    new AuditTask
                    {
                        Id = taskA,
                        TenantId = tenantA,
                        AuditRecordDetailId = detailA,
                        AssigneeUserId = auditorA,
                        AssigneeNameSnapshot = "Auditor A",
                        Status = AuditTaskStatus.Pending
                    },
                    new AuditTask
                    {
                        Id = taskB,
                        TenantId = tenantA,
                        AuditRecordDetailId = detailA,
                        AssigneeUserId = auditorB,
                        AssigneeNameSnapshot = "Auditor B",
                        Status = AuditTaskStatus.Pending
                    }
                ]);
            }

            await using (var context = new DefaultDbContext(ConnectionString, tenantB))
            {
                await context.AuditFlows.InsertAsync(new AuditFlow
                {
                    Id = flowB,
                    TenantId = tenantB,
                    FlowKey = "audit-isolation-flow",
                    SceneCode = CustomAuditSceneCode,
                    Name = "Tenant B flow",
                    IsEnabled = true
                });
            }

            await using (var context = new DefaultDbContext(ConnectionString, tenantA))
            {
                List<AuditFlow> flows = await context.AuditFlows.ToListAsync();
                await Assert.That(flows.Select(item => item.Id)).IsEquivalentTo([flowA]);

                List<AuditTask> auditorRows = await context.AuditTasks
                    .Where(item => item.AssigneeUserId == auditorA)
                    .ToListAsync();
                await Assert.That(auditorRows.Select(item => item.Id)).IsEquivalentTo([taskA]);

                int changed = await context.AuditTasks
                    .Where(item => item.Id == taskA && item.AssigneeUserId == auditorA && item.StatusValue == (int)AuditTaskStatus.Pending)
                    .ExecuteUpdateAsync(update => update
                        .Set(item => item.StatusValue, (int)AuditTaskStatus.Approved)
                        .Set(item => item.Comment, "approved")
                        .Set(item => item.ActedTime, DateTimeOffset.UtcNow));
                await Assert.That(changed).IsEqualTo(1);

                await context.AuditTasks.Where(item => item.Id == taskB && item.StatusValue == (int)AuditTaskStatus.Pending)
                    .ExecuteUpdateAsync(update => update.Set(item => item.StatusValue, (int)AuditTaskStatus.Approved));
                await context.AuditRecordDetails.Where(item => item.Id == detailA && item.StatusValue == (int)AuditStepStatus.Pending)
                    .ExecuteUpdateAsync(update => update
                        .Set(item => item.StatusValue, (int)AuditStepStatus.Approved)
                        .Set(item => item.CompletedTime, DateTimeOffset.UtcNow));
                await context.AuditRecords.Where(item => item.Id == recordA && item.StatusValue == (int)AuditRecordStatus.InProgress)
                    .ExecuteUpdateAsync(update => update
                        .Set(item => item.StatusValue, (int)AuditRecordStatus.Approved)
                        .Set(item => item.CurrentStepNo, null)
                        .Set(item => item.CompletedTime, DateTimeOffset.UtcNow));

                AuditRecord record = await context.AuditRecords.Where(item => item.Id == recordA).FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("Audit record was not found.");
                AuditRecordDetail detail = await context.AuditRecordDetails.Where(item => item.Id == detailA).FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("Audit record detail was not found.");
                await Assert.That(record.Status).IsEqualTo(AuditRecordStatus.Approved);
                await Assert.That(detail.Status).IsEqualTo(AuditStepStatus.Approved);
                await Assert.That(await context.AuditTasks
                    .Where(item => item.AuditRecordDetailId == detailA && item.Status == AuditTaskStatus.Approved)
                    .CountAsync()).IsEqualTo(2);
            }
        }
        finally
        {
            await SoftDeleteAuditRowsAsync(tenantA, flowA, recordA, detailA, taskA, taskB);
            await SoftDeleteAuditRowsAsync(tenantB, flowB, null, null, null, null);
        }
    }

    private static async Task SoftDeleteNotificationRowsAsync(Guid tenantId, Guid notificationId)
    {
        await using var context = new DefaultDbContext(ConnectionString, tenantId);
        await context.InAppNotificationRecipients.Where(item => item.NotificationId == notificationId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        await context.InAppNotifications.Where(item => item.Id == notificationId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
    }

    private static async Task SoftDeleteAuditRowsAsync(
        Guid tenantId,
        Guid flowId,
        Guid? recordId,
        Guid? detailId,
        Guid? taskA,
        Guid? taskB)
    {
        await using var context = new DefaultDbContext(ConnectionString, tenantId);
        Guid[] taskIds = [.. new[] { taskA, taskB }.Where(item => item.HasValue).Select(item => item!.Value)];
        if (taskIds.Length > 0)
            await context.AuditTasks.Where(item => taskIds.Contains(item.Id))
                .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        if (detailId is Guid detail)
            await context.AuditRecordDetails.Where(item => item.Id == detail)
                .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        if (recordId is Guid record)
            await context.AuditRecords.Where(item => item.Id == record)
                .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        await context.AuditStepDefinitions.Where(item => item.FlowId == flowId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        await context.AuditFlows.Where(item => item.Id == flowId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
    }
}
