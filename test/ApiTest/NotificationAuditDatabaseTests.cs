using Entity.AuditMod;
using Entity.NotificationMod;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Perigon.AspNetCore.Constants;
using Perigon.PostgreSQL;

namespace ApiTest;

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
    public async Task InAppNotifications_ShouldKeepIndependentReadStatePerUser()
    {
        Guid userA = Guid.CreateVersion7();
        Guid userB = Guid.CreateVersion7();
        Guid notificationId = Guid.CreateVersion7();

        try
        {
            await using (var context = AppDbFactory.CreateDbContext(ConnectionString))
            {
                await context.InAppNotifications.InsertAsync(new InAppNotification
                {
                    Id = notificationId,
                    Type = "announcement",
                    Title = "announcement",
                    Content = "content",
                    IsPublished = true,
                    PublishedTime = DateTimeOffset.UtcNow,
                });
                await context.InAppNotificationRecipients.InsertManyReturningAsync(
                [
                    new InAppNotificationRecipient { NotificationId = notificationId, UserId = userA },
                    new InAppNotificationRecipient { NotificationId = notificationId, UserId = userB },
                ]);
            }

            await using (var context = AppDbFactory.CreateDbContext(ConnectionString))
            {
                List<InAppNotificationRecipient> rows = await context.InAppNotificationRecipients
                    .Where(item => item.NotificationId == notificationId)
                    .ToListAsync();
                await Assert.That(rows.Count).IsEqualTo(2);
                await Assert.That(rows.All(item => !item.IsRead)).IsTrue();

                DateTimeOffset readTime = DateTimeOffset.UtcNow;
                int changed = await context.InAppNotificationRecipients
                    .Where(item => item.NotificationId == notificationId && item.UserId == userA)
                    .ExecuteUpdateAsync(update => update
                        .Set(item => item.IsRead, true)
                        .Set(item => item.ReadTime, readTime));
                await Assert.That(changed).IsEqualTo(1);

                InAppNotificationRecipient userBRow = await context.InAppNotificationRecipients
                    .Where(item => item.NotificationId == notificationId && item.UserId == userB)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("User B recipient row was not found.");
                await Assert.That(userBRow.IsRead).IsFalse();
            }
        }
        finally
        {
            await SoftDeleteNotificationRowsAsync(notificationId);
        }
    }

    [Test]
    public async Task AuditRows_ShouldPersistDecisionState()
    {
        Guid applicant = Guid.CreateVersion7();
        Guid auditorA = Guid.CreateVersion7();
        Guid auditorB = Guid.CreateVersion7();
        Guid flowId = Guid.CreateVersion7();
        Guid recordId = Guid.CreateVersion7();
        Guid detailId = Guid.CreateVersion7();
        Guid taskA = Guid.CreateVersion7();
        Guid taskB = Guid.CreateVersion7();

        try
        {
            await using var context = AppDbFactory.CreateDbContext(ConnectionString);
            await context.AuditFlows.InsertAsync(new AuditFlow
            {
                Id = flowId,
                FlowKey = "audit-flow",
                SceneCode = CustomAuditSceneCode,
                Name = "Audit flow",
                IsEnabled = true,
            });
            Guid stepDefinitionId = Guid.CreateVersion7();
            await context.AuditStepDefinitions.InsertAsync(new AuditStepDefinition
            {
                Id = stepDefinitionId,
                FlowId = flowId,
                StepNo = 1,
                Name = "Approve",
                AuditorUserIds = [auditorA, auditorB],
                AuditMode = AuditMode.All,
            });
            await context.AuditRecords.InsertAsync(new AuditRecord
            {
                Id = recordId,
                FlowId = flowId,
                FlowNameSnapshot = "Audit flow",
                SceneCode = CustomAuditSceneCode,
                BizId = "biz-a",
                ApplicantUserId = applicant,
                ApplicantNameSnapshot = "Applicant",
                ApplyReason = "database contract",
                Status = AuditRecordStatus.InProgress,
                CurrentStepNo = 1,
            });
            await context.AuditRecordDetails.InsertAsync(new AuditRecordDetail
            {
                Id = detailId,
                AuditRecordId = recordId,
                StepDefinitionId = stepDefinitionId,
                StepNoSnapshot = 1,
                StepNameSnapshot = "Approve",
                AuditModeSnapshot = AuditMode.All,
                Status = AuditStepStatus.Pending,
                StartedTime = DateTimeOffset.UtcNow,
            });
            await context.AuditTasks.InsertManyReturningAsync(
            [
                new AuditTask { Id = taskA, AuditRecordDetailId = detailId, AssigneeUserId = auditorA, AssigneeNameSnapshot = "Auditor A", Status = AuditTaskStatus.Pending },
                new AuditTask { Id = taskB, AuditRecordDetailId = detailId, AssigneeUserId = auditorB, AssigneeNameSnapshot = "Auditor B", Status = AuditTaskStatus.Pending },
            ]);

            int changed = await context.AuditTasks
                .Where(item => item.Id == taskA && item.Status == AuditTaskStatus.Pending)
                .ExecuteUpdateAsync(update => update
                    .Set(item => item.Status, AuditTaskStatus.Approved)
                    .Set(item => item.Comment, "approved")
                    .Set(item => item.ActedTime, DateTimeOffset.UtcNow));
            await Assert.That(changed).IsEqualTo(1);

            await context.AuditTasks.Where(item => item.Id == taskB).ExecuteUpdateAsync(update => update
                .Set(item => item.Status, AuditTaskStatus.Approved));
            await context.AuditRecordDetails.Where(item => item.Id == detailId).ExecuteUpdateAsync(update => update
                .Set(item => item.Status, AuditStepStatus.Approved)
                .Set(item => item.CompletedTime, DateTimeOffset.UtcNow));
            await context.AuditRecords.Where(item => item.Id == recordId).ExecuteUpdateAsync(update => update
                .Set(item => item.Status, AuditRecordStatus.Approved)
                .Set(item => item.CurrentStepNo, null)
                .Set(item => item.CompletedTime, DateTimeOffset.UtcNow));

            AuditRecord record = await context.AuditRecords.Where(item => item.Id == recordId).FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Audit record was not found.");
            await Assert.That(record.Status).IsEqualTo(AuditRecordStatus.Approved);
            await Assert.That(await context.AuditTasks
                .Where(item => item.AuditRecordDetailId == detailId && item.Status == AuditTaskStatus.Approved)
                .CountAsync()).IsEqualTo(2);
        }
        finally
        {
            await SoftDeleteAuditRowsAsync(flowId, recordId, detailId, taskA, taskB);
        }
    }

    private static async Task SoftDeleteNotificationRowsAsync(Guid notificationId)
    {
        await using var context = AppDbFactory.CreateDbContext(ConnectionString);
        await context.InAppNotificationRecipients.Where(item => item.NotificationId == notificationId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        await context.InAppNotifications.Where(item => item.Id == notificationId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
    }

    private static async Task SoftDeleteAuditRowsAsync(
        Guid flowId,
        Guid recordId,
        Guid detailId,
        Guid taskA,
        Guid taskB)
    {
        await using var context = AppDbFactory.CreateDbContext(ConnectionString);
        Guid[] taskIds = [taskA, taskB];
        await context.AuditTasks.Where(item => taskIds.Contains(item.Id))
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        await context.AuditRecordDetails.Where(item => item.Id == detailId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        await context.AuditRecords.Where(item => item.Id == recordId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        await context.AuditStepDefinitions.Where(item => item.FlowId == flowId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
        await context.AuditFlows.Where(item => item.Id == flowId)
            .ExecuteUpdateAsync(update => update.Set(item => item.IsDeleted, true));
    }
}
