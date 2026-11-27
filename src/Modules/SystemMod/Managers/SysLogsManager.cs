using EntityFramework.AppDbFactory;
using SystemMod.Models.SysLogsDtos;

namespace SystemMod.Managers;

/// <summary>
/// 系统日志
/// </summary>
public class SysLogsManager : ManagerBase<DefaultDbContext, SysLogs>
{
    public SysLogsManager(
        AppDbFactory dbContextFactory,
        ILogger<SysLogsManager> logger,
        IUserContext userContext
    ) : base(dbContextFactory, userContext, logger)
    {
    }

    public async Task<PageList<SysLogsItemDto>> ToPageAsync(SysLogsFilterDto filter)
    {
        Queryable = _dbContext.SetWithoutFilters<SysLogs>("SoftDelete")
            .WhereNotNull(filter.ActionUserName, q => q.ActionUserName.Contains(filter.ActionUserName!))
            .WhereNotNull(filter.TargetName, q => q.TargetName == filter.TargetName)
            .WhereNotNull(filter.ActionType, q => q.ActionType == filter.ActionType!.Value);

        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
        {
            // 包含今天
            var endDate = filter.EndDate.Value.AddDays(1);
            Queryable = Queryable.Between(q => q.CreatedTime, filter.StartDate.Value, endDate);
        }
        return await PageListAsync<SysLogsFilterDto, SysLogsItemDto>(filter, MapItem);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SysLogs?> GetOwnedAsync(Guid id)
    {
        IQueryable<SysLogs> query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        return await query.FirstOrDefaultAsync();
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id);
        return await query.AnyAsync();
    }
    private static SysLogsItemDto MapItem(SysLogs x) => new() { Id = x.Id, ActionUserName = x.ActionUserName, TargetName = x.TargetName!, Route = x.Route, ActionType = x.ActionType, Description = x.Description, CreatedTime = x.CreatedTime };
}
