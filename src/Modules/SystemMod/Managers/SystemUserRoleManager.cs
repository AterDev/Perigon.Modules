using EntityFramework.AppDbFactory;

namespace SystemMod.Managers;

/// <summary>
/// 系统用户角色关联管理器
/// </summary>
public class SystemUserRoleManager(
    AppDbFactory dbContextFactory,
    ILogger<SystemUserRoleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SystemUserRole>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// 批量设置用户角色
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="roleIds">角色ID列表</param>
    /// <param name="dbContext">可选的外部上下文；传入时复用调用方事务</param>
    /// <returns></returns>
    public async Task<bool> SetUserRolesAsync(
        Guid userId,
        List<Guid> roleIds,
        DefaultDbContext? dbContext = null)
    {
        if (dbContext != null)
        {
            return await SetUserRolesAsync(dbContext, userId, roleIds);
        }

        return await ExecuteInTransactionAsync(
            () => SetUserRolesAsync(_dbContext, userId, roleIds));
    }

    private async Task<bool> SetUserRolesAsync(
        DefaultDbContext dbContext,
        Guid userId,
        List<Guid> roleIds)
    {
        // 先删除现有的用户角色关联
        await dbContext.SystemUserRoles
            .Where(ur => ur.UserId == userId)
            .ExecuteDeleteAsync();

        if (roleIds.Count == 0)
        {
            return true;
        }

        dbContext.SystemUserRoles.AddRange(roleIds.Select(roleId => new SystemUserRole
        {
            UserId = userId,
            RoleId = roleId,
            TenantId = _userContext.TenantId
        }));
        await dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 获取用户的角色ID列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    public async Task<List<Guid>> GetUserRoleIdsAsync(Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();
    }

    /// <summary>
    /// 获取拥有某个角色的用户ID列表
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns></returns>
    public async Task<List<Guid>> GetRoleUserIdsAsync(Guid roleId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();
    }

    /// <summary>
    /// 检查用户是否拥有指定角色
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="roleId">角色ID</param>
    /// <returns></returns>
    public async Task<bool> HasUserRoleAsync(Guid userId, Guid roleId)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id).Join(
            _dbContext.SystemUsers,
            ur => ur.UserId,
            u => u.Id,
            (ur, u) => u
        ).Where(u => u.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }
}
