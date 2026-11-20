namespace AuditMod.Managers;

public sealed class AuditTaskManager(
    AppDbFactory dbContextFactory,
    IUserContext userContext,
    ILogger<AuditTaskManager> logger)
    : ManagerBase<DefaultDbContext, AuditTask>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<AuditTaskItemDto>> FilterMineAsync(Guid userId, AuditTaskFilterDto filter, CancellationToken ct = default)
    {
        var query = _dbContext.AuditTasks.Where(task => task.AssigneeUserId == userId);
        if (filter.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
        }

        if (filter.RecordStatus is { } recordStatus)
        {
            query = query.Where(task => _dbContext.AuditRecordDetails.Any(detail =>
                detail.Id == task.AuditRecordDetailId &&
                _dbContext.AuditRecords.Any(record =>
                    record.Id == detail.AuditRecordId && record.Status == recordStatus)));
        }
        else
        {
            query = query.Where(task => _dbContext.AuditRecordDetails.Any(detail =>
                detail.Id == task.AuditRecordDetailId &&
                _dbContext.AuditRecords.Any(record => record.Id == detail.AuditRecordId)));
        }
        var count = await query.CountAsync(ct);
        var tasks = await query.OrderByDescending(x => x.CreatedTime).Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20))
            .Take(filter.PageSize.GetValueOrDefault(20)).ToListAsync(ct);
        var detailIdsForPage = tasks.Select(x => x.AuditRecordDetailId).Distinct().ToArray();
        var details = (await _dbContext.AuditRecordDetails.Where(x => detailIdsForPage.Contains(x.Id)).ToListAsync(ct))
            .ToDictionary(x => x.Id);
        var recordIdsForPage = details.Values.Select(x => x.AuditRecordId).Distinct().ToArray();
        var records = (await _dbContext.AuditRecords.Where(x => recordIdsForPage.Contains(x.Id)).ToListAsync(ct))
            .ToDictionary(x => x.Id);
        var rows = tasks.Where(task => details.TryGetValue(task.AuditRecordDetailId, out var detail)
                && records.ContainsKey(detail.AuditRecordId))
            .Select(task =>
            {
                var detail = details[task.AuditRecordDetailId];
                var record = records[detail.AuditRecordId];
                return new AuditTaskItemDto
                {
                    Id = task.Id,
                    AuditRecordId = record.Id,
                    AuditRecordDetailId = detail.Id,
                    AssigneeUserId = task.AssigneeUserId,
                    AssigneeNameSnapshot = task.AssigneeNameSnapshot,
                    Status = task.Status,
                    StepNo = detail.StepNoSnapshot,
                    StepName = detail.StepNameSnapshot,
                    BizId = record.BizId,
                    Comment = task.Comment,
                    ActedTime = task.ActedTime,
                };
            }).ToList();
        return new()
        {
            Count = count,
            Data = rows,
            PageIndex = filter.PageIndex.GetValueOrDefault(1)
        };
    }
    public async Task<AuditTaskDetailDto?> GetAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        var task = await _dbContext.AuditTasks.Where(x => x.Id == taskId && (x.AssigneeUserId == userId || _userContext.IsAdmin)).FirstOrDefaultAsync(ct);
        if (task is null)
        {
            return null;
        }

        var detail = await _dbContext.AuditRecordDetails.Where(x => x.Id == task.AuditRecordDetailId).FirstOrDefaultAsync(ct);
        if (detail is null)
        {
            return null;
        }

        var record = await _dbContext.AuditRecords.Where(x => x.Id == detail.AuditRecordId).FirstOrDefaultAsync(ct);
        if (record is null)
        {
            return null;
        }

        var details = await _dbContext.AuditRecordDetails.Where(x => x.AuditRecordId == record.Id).OrderBy(x => x.StepNoSnapshot).ToListAsync(ct);
        Guid[] ids = details.Select(x => x.Id).ToArray();
        var tasks = await _dbContext.AuditTasks.Where(x => ids.Contains(x.AuditRecordDetailId)).ToListAsync(ct);
        return new AuditTaskDetailDto { Id = task.Id, AuditRecordId = record.Id, AuditRecordDetailId = detail.Id, AssigneeUserId = task.AssigneeUserId, AssigneeNameSnapshot = task.AssigneeNameSnapshot, Status = task.Status, StepNo = detail.StepNoSnapshot, StepName = detail.StepNameSnapshot, BizId = record.BizId, Comment = task.Comment, ActedTime = task.ActedTime, Record = AuditRecordManager.Map(record, details, tasks) };
    }
    public Task<AuditTaskDetailDto> ApproveAsync(Guid id, Guid userId, AuditDecisionDto dto, Func<DefaultDbContext, AuditNotificationMessage, CancellationToken, Task> publishNotification, CancellationToken ct = default) => DecideAndGetAsync(id, userId, AuditDecision.Approve, dto.Comment, publishNotification, ct);
    public Task<AuditTaskDetailDto> RejectAsync(Guid id, Guid userId, AuditDecisionDto dto, Func<DefaultDbContext, AuditNotificationMessage, CancellationToken, Task> publishNotification, CancellationToken ct = default) => DecideAndGetAsync(id, userId, AuditDecision.Reject, dto.Comment, publishNotification, ct);
    private async Task<AuditTaskDetailDto> DecideAndGetAsync(Guid id, Guid userId, AuditDecision decision, string? comment, Func<DefaultDbContext, AuditNotificationMessage, CancellationToken, Task> publishNotification, CancellationToken ct)
    {
        if (_userContext.UserId != userId)
        {
            throw new BusinessException("无权限。", StatusCodes.Status403Forbidden);
        }

        await DecideAsync(id, decision, comment, publishNotification, ct);
        return await GetAsync(id, userId, ct) ?? throw new BusinessException("未找到审核任务。", StatusCodes.Status404NotFound);
    }
    public async Task<AuditRecord> DecideAsync(Guid taskId, AuditDecision decision, string? comment, Func<DefaultDbContext, AuditNotificationMessage, CancellationToken, Task> publishNotification, CancellationToken ct = default)
    {
        var task = await _dbSet.FirstOrDefaultAsync(x => x.Id == taskId, ct) ?? throw new BusinessException("未找到审核任务。", StatusCodes.Status404NotFound);
        if (task.AssigneeUserId != _userContext.UserId)
        {
            throw new BusinessException("无权限。", StatusCodes.Status403Forbidden);
        }

        if (task.Status != AuditTaskStatus.Pending)
        {
            throw new BusinessException("审核任务已处理。", StatusCodes.Status409Conflict);
        }

        var detailReference = await _dbContext.AuditRecordDetails.Where(x => x.Id == task.AuditRecordDetailId).FirstOrDefaultAsync(ct);
        var recordId = detailReference?.AuditRecordId ?? Guid.Empty;
        if (recordId == Guid.Empty)
        {
            throw new BusinessException("未找到审核记录。", StatusCodes.Status404NotFound);
        }

        AuditRecord? result = null;
        await ExecuteInTransactionAsync(async () =>
        {
            var locked = await _dbContext.AuditRecords.Where(x => x.Id == recordId && x.Status == AuditRecordStatus.InProgress).ExecuteUpdateAsync(s => s.Set(x => x.UpdatedTime, DateTimeOffset.UtcNow), cancellationToken: ct);
            if (locked == 0)
            {
                if (!await _dbContext.AuditRecords.Where(x => x.Id == recordId).AnyAsync(ct))
                {
                    throw new BusinessException("未找到审核记录。", StatusCodes.Status404NotFound);
                }

                throw new BusinessException("审核记录已结束。", StatusCodes.Status409Conflict);
            }
            var record = await _dbContext.AuditRecords.Where(x => x.Id == recordId).FirstOrDefaultAsync(ct) ?? throw new BusinessException("未找到审核记录。", StatusCodes.Status404NotFound);
            var detail = await _dbContext.AuditRecordDetails.FirstOrDefaultAsync(x => x.Id == task.AuditRecordDetailId, ct) ?? throw new BusinessException("未找到审核步骤。", StatusCodes.Status404NotFound);
            var decisionStatus = decision == AuditDecision.Approve ? AuditTaskStatus.Approved : AuditTaskStatus.Rejected;
            var affected = await _dbContext.AuditTasks.Where(x => x.Id == taskId && x.AssigneeUserId == _userContext.UserId && x.Status == AuditTaskStatus.Pending).ExecuteUpdateAsync(s => s.Set(x => x.Status, decisionStatus).Set(x => x.Comment, comment).Set(x => x.ActedTime, DateTimeOffset.UtcNow), cancellationToken: ct);
            if (affected == 0)
            {
                throw new BusinessException("审核任务已处理。", StatusCodes.Status409Conflict);
            }

            if (record.Status != AuditRecordStatus.InProgress || detail.Status != AuditStepStatus.Pending)
            {
                throw new BusinessException("审核记录已结束。", StatusCodes.Status409Conflict);
            }

            var siblings = await _dbContext.AuditTasks.Where(x => x.AuditRecordDetailId == detail.Id).ToListAsync(ct);
            var approved = detail.AuditModeSnapshot == AuditMode.Any ? siblings.Any(x => x.Status == AuditTaskStatus.Approved) : siblings.All(x => x.Status == AuditTaskStatus.Approved);
            var rejected = detail.AuditModeSnapshot == AuditMode.All ? siblings.Any(x => x.Status == AuditTaskStatus.Rejected) : siblings.All(x => x.Status == AuditTaskStatus.Rejected);
            if (!approved && !rejected)
            {
                result = record;
                return;
            }
            var completed = DateTimeOffset.UtcNow;
            var detailStatus = approved ? AuditStepStatus.Approved : AuditStepStatus.Rejected;
            var detailClaimed = await _dbContext.AuditRecordDetails.Where(x => x.Id == detail.Id && x.Status == AuditStepStatus.Pending).ExecuteUpdateAsync(s => s.Set(x => x.Status, detailStatus).Set(x => x.CompletedTime, completed), cancellationToken: ct);
            if (detailClaimed == 0)
            {
                result = record;
                return;
            }
            await _dbContext.AuditTasks.Where(x => x.AuditRecordDetailId == detail.Id && (x.Status == AuditTaskStatus.Pending || x.Status == AuditTaskStatus.Waiting)).ExecuteUpdateAsync(s => s.Set(x => x.Status, AuditTaskStatus.Cancelled), cancellationToken: ct);
            detail.Status = detailStatus;
            if (rejected)
            {
                record.Status = AuditRecordStatus.Rejected;
                record.CompletedTime = completed;
                await _dbContext.AuditRecords.Where(x => x.Id == record.Id).ExecuteUpdateAsync(s => s.Set(x => x.Status, AuditRecordStatus.Rejected).Set(x => x.CompletedTime, completed), cancellationToken: ct);
                await CancelRemainingAsync(record.Id, ct);
            }
            if (rejected)
            {
                await AddResultNotificationAsync(record, false, publishNotification, ct);
            }
            else
            {
                var next = await _dbContext.AuditRecordDetails.Where(x => x.AuditRecordId == record.Id && x.Status == AuditStepStatus.Waiting).OrderBy(x => x.StepNoSnapshot).FirstOrDefaultAsync(ct);
                if (next is null)
                {
                    record.Status = AuditRecordStatus.Approved;
                    record.CompletedTime = completed;
                    await _dbContext.AuditRecords.Where(x => x.Id == record.Id).ExecuteUpdateAsync(s => s.Set(x => x.Status, AuditRecordStatus.Approved).Set(x => x.CompletedTime, completed), cancellationToken: ct);
                    await AddResultNotificationAsync(record, true, publishNotification, ct);
                }
                else
                {
                    next.Status = AuditStepStatus.Pending;
                    next.StartedTime = completed;
                    record.CurrentStepNo = next.StepNoSnapshot;
                    await _dbContext.AuditRecordDetails.Where(x => x.Id == next.Id).ExecuteUpdateAsync(s => s.Set(x => x.Status, AuditStepStatus.Pending).Set(x => x.StartedTime, completed), cancellationToken: ct);
                    await _dbContext.AuditRecords.Where(x => x.Id == record.Id).ExecuteUpdateAsync(s => s.Set(x => x.CurrentStepNo, next.StepNoSnapshot), cancellationToken: ct);
                    var nextTasks = await _dbContext.AuditTasks.Where(x => x.AuditRecordDetailId == next.Id && x.Status == AuditTaskStatus.Waiting).ToListAsync(ct);
                    await _dbContext.AuditTasks.Where(x => x.AuditRecordDetailId == next.Id && x.Status == AuditTaskStatus.Waiting).ExecuteUpdateAsync(s => s.Set(x => x.Status, AuditTaskStatus.Pending), cancellationToken: ct);
                    await AddTaskNotificationAsync(record, nextTasks.Select(x => x.AssigneeUserId), publishNotification, ct);
                }
            }
            result = record;
        }, ct);
        return result ?? throw new BusinessException("未找到审核记录。", StatusCodes.Status404NotFound);
    }
    private async Task CancelRemainingAsync(Guid id, CancellationToken ct)
    {
        var details = await _dbContext.AuditRecordDetails.Where(x => x.AuditRecordId == id && (x.Status == AuditStepStatus.Waiting || x.Status == AuditStepStatus.Pending)).ToListAsync(ct);
        foreach (var d in details)
        {
            var now = DateTimeOffset.UtcNow;
            await _dbContext.AuditRecordDetails.Where(x => x.Id == d.Id).ExecuteUpdateAsync(s => s.Set(x => x.Status, AuditStepStatus.Cancelled).Set(x => x.CompletedTime, now), cancellationToken: ct);
            await _dbContext.AuditTasks.Where(x => x.AuditRecordDetailId == d.Id && (x.Status == AuditTaskStatus.Waiting || x.Status == AuditTaskStatus.Pending)).ExecuteUpdateAsync(s => s.Set(x => x.Status, AuditTaskStatus.Cancelled), cancellationToken: ct);
        }
    }
    public override Task<bool> HasPermissionAsync(Guid id) => _dbSet.AnyAsync(x => x.Id == id && x.AssigneeUserId == _userContext.UserId);

    private async Task AddTaskNotificationAsync(AuditRecord record, IEnumerable<Guid> recipientIds, Func<DefaultDbContext, AuditNotificationMessage, CancellationToken, Task> publishNotification, CancellationToken cancellationToken)
    {
        var recipients = recipientIds.Distinct().ToArray();
        if (recipients.Length == 0)
        {
            return;
        }

        await publishNotification(
            _dbContext,
            new AuditNotificationMessage(
                "audit-task",
                "新的审核待办",
                $"您有一条待审核申请：{record.FlowNameSnapshot}",
                $"/api/audit-records/{record.Id}",
                record.ApplicantUserId,
                recipients),
            cancellationToken);
    }

    private async Task AddResultNotificationAsync(AuditRecord record, bool approved, Func<DefaultDbContext, AuditNotificationMessage, CancellationToken, Task> publishNotification, CancellationToken cancellationToken)
    {
        await publishNotification(
            _dbContext,
            new AuditNotificationMessage(
                "audit-result",
                approved ? "审核已通过" : "审核未通过",
                approved ? $"申请“{record.FlowNameSnapshot}”已通过。" : $"申请“{record.FlowNameSnapshot}”未通过。",
                $"/api/audit-records/{record.Id}",
                record.ApplicantUserId,
                [record.ApplicantUserId]),
            cancellationToken);
    }
}


