namespace AuditMod.Managers;

public sealed class AuditRecordManager(AppDbFactory dbContextFactory, IUserContext userContext, ILogger<AuditRecordManager> logger)
    : ManagerBase<DefaultDbContext, AuditRecord>(dbContextFactory, userContext, logger)
{
    public override Task<bool> HasPermissionAsync(Guid id) => _dbSet.AnyAsync(x => x.Id == id && (_userContext.IsAdmin || x.ApplicantUserId == _userContext.UserId || _dbContext.AuditRecordDetails.Any(d => d.AuditRecordId == id && _dbContext.AuditTasks.Any(t => t.AuditRecordDetailId == d.Id && t.AssigneeUserId == _userContext.UserId))));
    public Task<PageList<AuditRecordItemDto>> FilterMineAsync(AuditRecordFilterDto filter, CancellationToken ct = default) => PageAsync(Build(filter).Where(x => x.ApplicantUserId == _userContext.UserId), filter, ct);
    public Task<PageList<AuditRecordItemDto>> FilterAllAsync(AuditRecordFilterDto filter, CancellationToken ct = default) => PageAsync(Build(filter), filter, ct);
    public async Task<AuditRecordDetailDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _dbSet.FirstOrDefaultAsync(x => x.Id == id, ct); if (record is null) return null;
        var details = await _dbContext.AuditRecordDetails.Where(x => x.AuditRecordId == id).OrderBy(x => x.StepNoSnapshot).ToListAsync(ct);
        Guid[] ids = details.Select(x => x.Id).ToArray();
        var tasks = await _dbContext.AuditTasks.Where(x => ids.Contains(x.AuditRecordDetailId)).OrderBy(x => x.CreatedTime).ToListAsync(ct);
        if (!_userContext.IsAdmin && record.ApplicantUserId != _userContext.UserId && !tasks.Any(x => x.AssigneeUserId == _userContext.UserId)) return null;
        return Map(record, details, tasks);
    }
    public async Task<AuditRecordDetailDto> WithdrawAsync(Guid id, Guid userId, AuditRecordWithdrawDto dto, CancellationToken ct = default)
    {
        if (_userContext.UserId != userId) throw new BusinessException("无权限。", StatusCodes.Status403Forbidden);
        await ExecuteInTransactionAsync(async () =>
        {
            var locked = await _dbContext.AuditRecords.Where(x => x.Id == id && x.ApplicantUserId == userId && x.StatusValue == (int)AuditRecordStatus.InProgress).ExecuteUpdateAsync(s => s.Set(x => x.UpdatedTime, DateTimeOffset.UtcNow), cancellationToken: ct);
            if (locked == 0)
            {
                var existing = await _dbContext.AuditRecords.Where(x => x.Id == id).FirstOrDefaultAsync(ct);
                if (existing is null) throw new BusinessException("未找到审核记录。", StatusCodes.Status404NotFound);
                if (existing.ApplicantUserId != userId) throw new BusinessException("无权限。", StatusCodes.Status403Forbidden);
                throw new BusinessException("审核记录已结束。", StatusCodes.Status409Conflict);
            }
            var record = await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync(ct) ?? throw new BusinessException("未找到审核记录。", StatusCodes.Status404NotFound);
            if (record.ApplicantUserId != userId) throw new BusinessException("无权限。", StatusCodes.Status403Forbidden); if (record.StatusValue != (int)AuditRecordStatus.InProgress) throw new BusinessException("审核记录已结束。", StatusCodes.Status409Conflict);
            var now = DateTimeOffset.UtcNow;
            await _dbContext.AuditRecords.Where(x => x.Id == id).ExecuteUpdateAsync(s => s.Set(x => x.StatusValue, (int)AuditRecordStatus.Withdrawn).Set(x => x.WithdrawnTime, now).Set(x => x.WithdrawReason, dto.Reason).Set(x => x.CurrentStepNo, (int?)null), cancellationToken: ct);
            var details = await _dbContext.AuditRecordDetails.Where(x => x.AuditRecordId == id && (x.StatusValue == (int)AuditStepStatus.Waiting || x.StatusValue == (int)AuditStepStatus.Pending)).ToListAsync(ct);
            foreach (var detail in details) { await _dbContext.AuditRecordDetails.Where(x => x.Id == detail.Id).ExecuteUpdateAsync(s => s.Set(x => x.StatusValue, (int)AuditStepStatus.Cancelled).Set(x => x.CompletedTime, now), cancellationToken: ct); await _dbContext.AuditTasks.Where(x => x.AuditRecordDetailId == detail.Id && (x.StatusValue == (int)AuditTaskStatus.Waiting || x.StatusValue == (int)AuditTaskStatus.Pending)).ExecuteUpdateAsync(s => s.Set(x => x.StatusValue, (int)AuditTaskStatus.Cancelled), cancellationToken: ct); }
        }, ct);
        return await GetAsync(id, ct) ?? throw new BusinessException("未找到审核记录。", StatusCodes.Status404NotFound);
    }
    private IQueryable<AuditRecord> Build(AuditRecordFilterDto f) => _dbSet.Where(x => (!f.Status.HasValue || x.StatusValue == (int)f.Status.Value) && (string.IsNullOrWhiteSpace(f.SceneCode) || x.SceneCode == f.SceneCode) && (string.IsNullOrWhiteSpace(f.BizId) || x.BizId == f.BizId));
    private static async Task<PageList<AuditRecordItemDto>> PageAsync(IQueryable<AuditRecord> q, AuditRecordFilterDto f, CancellationToken ct)
    {
        var count = await q.CountAsync(ct);
        var records = await q.OrderByDescending(x => x.CreatedTime)
            .Skip((f.PageIndex.GetValueOrDefault(1) - 1) * f.PageSize.GetValueOrDefault(20)).Take(f.PageSize.GetValueOrDefault(20)).ToListAsync(ct);
        var rows = records.Select(x => new AuditRecordItemDto
        {
            Id = x.Id, FlowNameSnapshot = x.FlowNameSnapshot, SceneCode = x.SceneCode,
            BizId = x.BizId, ApplicantUserId = x.ApplicantUserId,
            ApplicantNameSnapshot = x.ApplicantNameSnapshot, Status = (AuditRecordStatus)x.StatusValue,
            CurrentStepNo = x.CurrentStepNo, SubmittedTime = x.SubmittedTime,
            CompletedTime = x.CompletedTime, WithdrawnTime = x.WithdrawnTime,
        }).ToList();
        return new() { Count = count, Data = rows, PageIndex = f.PageIndex.GetValueOrDefault(1) };
    }
    internal static AuditRecordDetailDto Map(AuditRecord r, IReadOnlyList<AuditRecordDetail> ds, IReadOnlyList<AuditTask> ts) => new() { Id = r.Id, FlowNameSnapshot = r.FlowNameSnapshot, SceneCode = r.SceneCode, BizId = r.BizId, ApplicantUserId = r.ApplicantUserId, ApplicantNameSnapshot = r.ApplicantNameSnapshot, Status = r.Status, CurrentStepNo = r.CurrentStepNo, SubmittedTime = r.SubmittedTime, CompletedTime = r.CompletedTime, WithdrawnTime = r.WithdrawnTime, ApplyReason = r.ApplyReason, FormSnapshotJson = r.FormSnapshotJson, WithdrawReason = r.WithdrawReason, Steps = ds.Select(d => new AuditRecordStepDto { Id = d.Id, StepNoSnapshot = d.StepNoSnapshot, StepNameSnapshot = d.StepNameSnapshot, AuditModeSnapshot = d.AuditModeSnapshot, Status = d.Status, StartedTime = d.StartedTime, CompletedTime = d.CompletedTime, Tasks = ts.Where(t => t.AuditRecordDetailId == d.Id).Select(t => new AuditRecordTaskDto { Id = t.Id, AuditRecordDetailId = t.AuditRecordDetailId, AssigneeUserId = t.AssigneeUserId, AssigneeNameSnapshot = t.AssigneeNameSnapshot, Status = t.Status, Comment = t.Comment, ActedTime = t.ActedTime }).ToList() }).ToList() };
}


