using EntityFramework.AppDbFactory;

namespace SystemMod.Managers;

/// <summary>
/// 系统用户角色关联管理器
/// </summary>
public class SysUserRoleManager(
    AppDbFactory dbContextFactory,
    ILogger<SysUserRoleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SysUserRole>(dbContextFactory, userContext, logger)
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
        await dbContext.SysUserRoles
            .Where(ur => ur.UserId == userId)
            .ExecuteDeleteAsync();

        if (roleIds.Count == 0)
        {
            return true;
        }

        await dbContext.SysUserRoles.BulkInsertAsync(roleIds.Select(roleId => new SysUserRole
        {
            UserId = userId,
            RoleId = roleId,
        }));
        return true;
    }

    /// <summary>
    /// 获取用户的角色ID列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    public async Task<List<Guid>> GetUserRoleIdsAsync(Guid userId)
    {
        var links = await _dbSet
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
        return links.Select(link => link.RoleId).ToList();
    }

    /// <summary>
    /// 获取拥有某个角色的用户ID列表
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns></returns>
    public async Task<List<Guid>> GetRoleUserIdsAsync(Guid roleId)
    {
        var links = await _dbSet
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();
        return links.Select(link => link.UserId).ToList();
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
            .Where(ur => ur.UserId == userId && ur.RoleId == roleId)
            .AnyAsync();
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id).Join(
            _dbContext.SysUsers,
            ur => ur.UserId,
            u => u.Id,
            (ur, u) => u
        );
        return await query.AnyAsync();
    }
}
