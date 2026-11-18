namespace AuditMod.Managers;

public sealed class AuditSubmissionManager(AppDbFactory dbContextFactory, IUserContext userContext, ILogger<AuditSubmissionManager> logger)
    : ManagerBase<DefaultDbContext, AuditRecord>(dbContextFactory, userContext, logger)
{
    public async Task<AuditRecord> SubmitAsync(string sceneCode, string bizId, string applyReason, string? formSnapshotJson, CancellationToken cancellationToken = default)
    {
        sceneCode = sceneCode.Trim();
        bizId = bizId.Trim();
        applyReason = applyReason.Trim();
        if (sceneCode.Length == 0 || bizId.Length == 0 || applyReason.Length == 0)
        {
            throw new BusinessException("审核业务参数不完整。", StatusCodes.Status400BadRequest);
        }

        if (sceneCode.Length > 100 || bizId.Length > 200 || applyReason.Length > 2000)
        {
            throw new BusinessException("审核业务参数长度超出限制。", StatusCodes.Status400BadRequest);
        }

        if (await _dbSet.AnyAsync(x => x.ApplicantUserId == _userContext.UserId && x.SceneCode == sceneCode && x.BizId == bizId && x.StatusValue == (int)AuditRecordStatus.InProgress, cancellationToken))
        {
            throw new BusinessException("已提交申请。", StatusCodes.Status409Conflict);
        }

        var flow = await _dbContext.AuditFlows.SingleOrDefaultAsync(x => x.SceneCode == sceneCode && x.IsEnabled, cancellationToken) ?? throw new BusinessException("当前业务场景没有启用的审核流程。", StatusCodes.Status400BadRequest);
        var definitions = await _dbContext.AuditStepDefinitions.Where(x => x.FlowId == flow.Id).OrderBy(x => x.StepNo).ToListAsync(cancellationToken);
        Validate(definitions);
        var ids = definitions.SelectMany(x => x.AuditorUserIds).Distinct().ToArray();
        // Perigon.PostgreSQL 0.3.1 does not translate a captured collection's
        // Perigon.PostgreSQL does not translate a captured collection for this
        // query shape, so load users and match the configured auditor ids in memory.
        var names = (await _dbContext.SysUsers.ToListAsync(cancellationToken))
            .Where(x => ids.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x.RealName ?? x.UserName ?? x.Id.ToString());
        if (names.Count != ids.Length)
        {
            throw new BusinessException("审核人不存在。", StatusCodes.Status400BadRequest);
        }

        var record = new AuditRecord { FlowId = flow.Id, FlowNameSnapshot = flow.Name, SceneCode = sceneCode, BizId = bizId, ApplicantUserId = _userContext.UserId, ApplicantNameSnapshot = _userContext.UserName ?? _userContext.UserId.ToString(), ApplyReason = applyReason, FormSnapshotJson = formSnapshotJson, CurrentStepNo = definitions[0].StepNo };
        await ExecuteInTransactionAsync(async () =>
        {
            await _dbContext.AuditRecords.InsertAsync(record, cancellationToken: cancellationToken);
            var details = new List<AuditRecordDetail>();
            var tasks = new List<AuditTask>();
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                var detail = new AuditRecordDetail { AuditRecordId = record.Id, StepDefinitionId = definition.Id, StepNoSnapshot = definition.StepNo, StepNameSnapshot = definition.Name, AuditModeSnapshotValue = definition.AuditModeValue, StatusValue = i == 0 ? (int)AuditStepStatus.Pending : (int)AuditStepStatus.Waiting, StartedTime = i == 0 ? DateTimeOffset.UtcNow : null };
                details.Add(detail);
                foreach (var userId in definition.AuditorUserIds)
                {
                    tasks.Add(new AuditTask { AuditRecordDetailId = detail.Id, AssigneeUserId = userId, AssigneeNameSnapshot = names[userId], StatusValue = i == 0 ? (int)AuditTaskStatus.Pending : (int)AuditTaskStatus.Waiting });
                }
            }
            await _dbContext.AuditRecordDetails.BulkInsertAsync(details, cancellationToken: cancellationToken);
            await _dbContext.AuditTasks.BulkInsertAsync(tasks, cancellationToken: cancellationToken);
            await AddTaskNotificationAsync(record, definitions[0].AuditorUserIds, cancellationToken);
        }, cancellationToken);
        return record;
    }
    public override Task<bool> HasPermissionAsync(Guid id) => _dbSet.AnyAsync(x => x.Id == id && x.ApplicantUserId == _userContext.UserId);
    private static void Validate(IReadOnlyList<AuditStepDefinition> definitions)
    {
        if (definitions.Count == 0 || !definitions.Select(x => x.StepNo).SequenceEqual(Enumerable.Range(1, definitions.Count)) || definitions.Any(x => x.AuditorUserIds.Length == 0 || x.AuditorUserIds.Distinct().Count() != x.AuditorUserIds.Length || !Enum.IsDefined(x.AuditMode)))
        {
            throw new BusinessException("审核步骤配置无效。", StatusCodes.Status400BadRequest);
        }
    }

    internal async Task AddTaskNotificationAsync(AuditRecord record, IEnumerable<Guid> recipientIds, CancellationToken cancellationToken)
    {
        var recipients = recipientIds.Distinct().ToArray();
        if (recipients.Length == 0)
        {
            return;
        }

        var notification = new InAppNotification
        {
            Type = "audit-task",
            Title = "新的审核待办",
            Content = $"您有一条待审核申请：{record.FlowNameSnapshot}",
            Link = $"/api/audit-records/{record.Id}",
            IsPublished = true,
            PublishedTime = DateTimeOffset.UtcNow,
            CreatedBy = record.ApplicantUserId,
            TargetUserIds = recipients,
        };
        await _dbContext.InAppNotifications.InsertAsync(notification, cancellationToken: cancellationToken);
        await _dbContext.InAppNotificationRecipients.BulkInsertAsync(recipients.Select(userId => new InAppNotificationRecipient
        {
            NotificationId = notification.Id,
            UserId = userId,
        }), cancellationToken: cancellationToken);
    }
}

