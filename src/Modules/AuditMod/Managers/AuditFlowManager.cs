namespace AuditMod.Managers;

public sealed class AuditFlowManager(
    AppDbFactory dbContextFactory,
    IUserContext userContext,
    ILogger<AuditFlowManager> logger)
    : ManagerBase<DefaultDbContext, AuditFlow>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<AuditFlowItemDto>> FilterAsync(AuditFlowFilterDto filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        if (!string.IsNullOrWhiteSpace(filter.SceneCode)) Queryable = Queryable.Where(x => x.SceneCode == filter.SceneCode!.Trim());
        if (!string.IsNullOrWhiteSpace(filter.Keyword)) { var keyword = filter.Keyword!.Trim(); Queryable = Queryable.Where(x => x.FlowKey.Contains(keyword) || x.Name.Contains(keyword)); }
        if (filter.IsEnabled.HasValue) Queryable = Queryable.Where(x => x.IsEnabled == filter.IsEnabled.Value);
        return await PageListAsync<AuditFlowFilterDto, AuditFlowItemDto>(filter, MapItem, cancellationToken);
    }

    public async Task<AuditFlowDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var flow = await _dbContext.AuditFlows.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (flow is null) return null;
        var steps = (await _dbContext.AuditStepDefinitions.Where(x => x.FlowId == id).OrderBy(x => x.StepNo)
            .ToListAsync(cancellationToken)).Select(ToDto).ToList();
        return ToDetail(flow, steps);
    }

    public async Task<AuditFlowDetailDto> AddAsync(AuditFlowAddDto dto, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(dto);
        if (await _dbContext.AuditFlows.AnyAsync(x => x.FlowKey == normalized.FlowKey, cancellationToken))
            throw new BusinessException("流程标识已存在。", StatusCodes.Status409Conflict);
        await EnsureAuditorsExistAsync(normalized.Steps, cancellationToken);
        if (normalized.IsEnabled) await EnsureUniqueSceneAsync(normalized.SceneCode, null, cancellationToken);
        var flow = new AuditFlow { TenantId = _userContext.TenantId, FlowKey = normalized.FlowKey, SceneCode = normalized.SceneCode, Name = normalized.Name, IsEnabled = normalized.IsEnabled };
        var steps = CreateSteps(flow.Id, normalized.Steps); foreach (var step in steps) step.TenantId = _userContext.TenantId;
        await ExecuteInTransactionAsync(async () =>
        {
            await _dbContext.AuditFlows.InsertAsync(flow, cancellationToken: cancellationToken);
            foreach (var step in steps)
                await _dbContext.AuditStepDefinitions.InsertAsync(step, cancellationToken: cancellationToken);
        }, cancellationToken);
        return ToDetail(flow, steps.OrderBy(x => x.StepNo).Select(ToDto).ToList());
    }

    public async Task<AuditFlowDetailDto> UpdateAsync(Guid id, AuditFlowUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(dto);
        var flow = await _dbContext.AuditFlows.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new BusinessException("未找到流程。", StatusCodes.Status404NotFound);
        if (await _dbContext.AuditFlows.AnyAsync(x => x.Id != id && x.FlowKey == normalized.FlowKey, cancellationToken)) throw new BusinessException("流程标识已存在。", StatusCodes.Status409Conflict);
        await EnsureAuditorsExistAsync(normalized.Steps, cancellationToken);
        if (normalized.IsEnabled) await EnsureUniqueSceneAsync(normalized.SceneCode, id, cancellationToken);
        var old = await _dbContext.AuditStepDefinitions.Where(x => x.FlowId == id).ToListAsync(cancellationToken);
        await ExecuteInTransactionAsync(async () =>
        {
            var now = DateTimeOffset.UtcNow;
            await _dbContext.AuditStepDefinitions.Where(x => x.FlowId == id).ExecuteUpdateAsync(s => s.Set(x => x.IsDeleted, true).Set(x => x.UpdatedTime, now), cancellationToken: cancellationToken);
            await _dbContext.AuditFlows.Where(x => x.Id == id).ExecuteUpdateAsync(s => s.Set(x => x.FlowKey, normalized.FlowKey).Set(x => x.SceneCode, normalized.SceneCode).Set(x => x.Name, normalized.Name).Set(x => x.IsEnabled, normalized.IsEnabled).Set(x => x.UpdatedTime, now), cancellationToken: cancellationToken);
            var fresh = CreateSteps(id, normalized.Steps);
            foreach (var step in fresh) step.TenantId = _userContext.TenantId;
            foreach (var step in fresh)
                await _dbContext.AuditStepDefinitions.InsertAsync(step, cancellationToken: cancellationToken);
        }, cancellationToken);
        return await GetAsync(id, cancellationToken) ?? throw new BusinessException("未找到流程。", StatusCodes.Status404NotFound);
    }

    public async Task SetEnabledAsync(Guid id, AuditFlowEnabledUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var flow = await _dbContext.AuditFlows.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new BusinessException("未找到流程。", StatusCodes.Status404NotFound);
        if (dto.Enabled) await EnsureUniqueSceneAsync(flow.SceneCode, id, cancellationToken);
        await _dbContext.AuditFlows.Where(x => x.Id == id).ExecuteUpdateAsync(s => s.Set(x => x.IsEnabled, dto.Enabled).Set(x => x.UpdatedTime, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var flow = await _dbContext.AuditFlows.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new BusinessException("未找到流程。", StatusCodes.Status404NotFound);
        var now = DateTimeOffset.UtcNow;
        await ExecuteInTransactionAsync(async () =>
        {
            await _dbContext.AuditStepDefinitions.Where(x => x.FlowId == id).ExecuteUpdateAsync(s => s.Set(x => x.IsDeleted, true).Set(x => x.UpdatedTime, now), cancellationToken: cancellationToken);
            await _dbContext.AuditFlows.Where(x => x.Id == id).ExecuteUpdateAsync(s => s.Set(x => x.IsDeleted, true).Set(x => x.IsEnabled, false).Set(x => x.UpdatedTime, now), cancellationToken: cancellationToken);
        }, cancellationToken);
    }

    public override Task<bool> HasPermissionAsync(Guid id) => _dbSet.AnyAsync(x => x.Id == id);

    private async Task EnsureUniqueSceneAsync(string scene, Guid? exclude, CancellationToken ct)
    {
        IQueryable<AuditFlow> query = _dbContext.AuditFlows.Where(x => x.SceneCode == scene && x.IsEnabled);
        if (exclude is Guid excludedId)
            query = query.Where(x => x.Id != excludedId);
        if (await query.AnyAsync(ct))
            throw new BusinessException("同一场景只能启用一个审核流程。", StatusCodes.Status409Conflict);
    }
    private static Normalized Normalize(AuditFlowAddDto dto)
    {
        var flowKey = dto.FlowKey.Trim(); var scene = dto.SceneCode.Trim(); var name = dto.Name.Trim();
        if (flowKey.Length == 0 || scene.Length == 0 || name.Length == 0) throw new BusinessException("流程标识、场景编码和流程名称不能为空。", StatusCodes.Status400BadRequest);
        if (flowKey.Length > 100 || scene.Length > 100 || name.Length > 200) throw new BusinessException("流程字段长度超出限制。", StatusCodes.Status400BadRequest);
        if (dto.Steps is null || dto.Steps.Count == 0) throw new BusinessException("流程至少需要一个步骤。", StatusCodes.Status400BadRequest);
        var steps = dto.Steps.OrderBy(x => x.StepNo).Select((x, i) => { var n = x.Name.Trim(); var ids = x.AuditorUserIds ?? []; if (x.StepNo != i + 1 || n.Length == 0 || n.Length > 200 || ids.Count == 0 || ids.Any(Guid.Empty.Equals) || ids.Distinct().Count() != ids.Count || !Enum.IsDefined(x.AuditMode)) throw new BusinessException("审核步骤配置无效。", StatusCodes.Status400BadRequest); return new NormalizedStep(x.StepNo, n, ids, x.AuditMode); }).ToList();
        return new(flowKey, scene, name, dto.IsEnabled, steps);
    }
    private async Task EnsureAuditorsExistAsync(IReadOnlyList<NormalizedStep> steps, CancellationToken cancellationToken)
    {
        var ids = steps.SelectMany(step => step.UserIds).Distinct().ToArray();
        var count = await _dbContext.SysUsers.Where(user => ids.Contains(user.Id)).CountAsync(cancellationToken);
        if (count != ids.Length) throw new BusinessException("审核人必须属于当前租户。", StatusCodes.Status400BadRequest);
    }
    private static List<AuditStepDefinition> CreateSteps(Guid flowId, IEnumerable<NormalizedStep> steps) => steps.Select(x => new AuditStepDefinition { FlowId = flowId, StepNo = x.StepNo, Name = x.Name, AuditorUserIds = [.. x.UserIds], AuditMode = x.Mode }).ToList();
    private static AuditFlowItemDto MapItem(AuditFlow x) => new() { Id = x.Id, FlowKey = x.FlowKey, SceneCode = x.SceneCode, Name = x.Name, IsEnabled = x.IsEnabled, CreatedTime = x.CreatedTime, UpdatedTime = x.UpdatedTime };
    private static AuditFlowDetailDto ToDetail(AuditFlow x, List<AuditFlowStepDto> steps) => new() { Id = x.Id, FlowKey = x.FlowKey, SceneCode = x.SceneCode, Name = x.Name, IsEnabled = x.IsEnabled, CreatedTime = x.CreatedTime, UpdatedTime = x.UpdatedTime, Steps = steps };
    private static AuditFlowStepDto ToDto(AuditStepDefinition x) => new() { StepNo = x.StepNo, Name = x.Name, AuditorUserIds = x.AuditorUserIds.ToList(), AuditMode = x.AuditMode };
    private sealed record Normalized(string FlowKey, string SceneCode, string Name, bool IsEnabled, List<NormalizedStep> Steps);
    private sealed record NormalizedStep(int StepNo, string Name, List<Guid> UserIds, AuditMode Mode);
}

