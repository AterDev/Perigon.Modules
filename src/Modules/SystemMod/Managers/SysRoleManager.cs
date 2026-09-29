using System.Linq.Expressions;
using EntityFramework.AppDbFactory;
using SystemMod.Models.SysRoleDtos;

namespace SystemMod.Managers;

public class SysRoleManager(
    AppDbFactory dbContextFactory,
    ILogger<SysRoleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SysRole>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SysRole> AddAsync(SysRoleAddDto dto)
    {
        var entity = dto.MapTo<SysRole>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<PageList<SysRoleItemDto>> FilterAsync(SysRoleFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name.Contains(filter.Name!))
            .WhereNotNull(filter.NameValue, q => q.NameValue == filter.NameValue);
        return await PageListAsync<SysRoleFilterDto, SysRoleItemDto>(filter);
    }

    /// <summary>
    /// 获取菜单
    /// </summary>
    /// <param name="systemRoles"></param>
    /// <returns></returns>
    public async Task<List<SysMenu>> GetSysMenusAsync(List<SysRole> systemRoles)
    {
        IEnumerable<Guid> ids = systemRoles.Select(r => r.Id);
        return await _dbContext
            .SysMenus.Where(m => m.SysRoles.Any(r => ids.Contains(r.Id)))
            .ToListAsync();
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SysRole> UpdateAsync(Guid id, SysRoleUpdateDto dto)
    {
        var current = await FindAsync(id) ?? throw new BusinessException(Localizer.RoleNotFound);

        // 权限验证可以在这里进行，利用 _userContext
        if (!CanUserModifyRole(current))
        {
            throw new BusinessException(
                Localizer.InsufficientPermissions,
                StatusCodes.Status403Forbidden
            );
        }

        await base.UpdateAsync<SysRoleUpdateDto>(id, dto);
        return current;
    }

    /// <summary>
    /// 验证用户是否可以修改角色
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    private bool CanUserModifyRole(SysRole role)
    {
        // 实现具体的权限逻辑
        return _userContext.IsRole(WebConst.SuperAdmin);
    }

    /// <summary>
    /// 更新角色菜单
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SysRole> SetMenusAsync(SysRoleSetMenusDto dto)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var current = await FindAsync(dto.Id) ?? throw new BusinessException(Localizer.RoleNotFound);

            if (!CanUserModifyRole(current))
            {
                throw new BusinessException(
                    Localizer.InsufficientPermissions,
                    StatusCodes.Status403Forbidden
                );
            }

            await _dbContext.Entry(current).Collection(r => r.SysMenus).LoadAsync();

            var menus = await _dbContext
                .SysMenus.Where(m => dto.MenuIds.Contains(m.Id))
                .ToListAsync();

            current.SysMenus = menus;
            _dbSet.Update(current);
            await _dbContext.SaveChangesAsync();

            return current;
        });
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return await query.AnyAsync();
    }

    public async Task<List<SysRole>> ListAsync(Expression<Func<SysRole, bool>>? whereExp = null)
    {
        return await _dbContext.SysRoles.AsNoTracking().Where(whereExp ?? (e => true)).ToListAsync();
    }

    public async Task<SysRoleDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<SysRoleDetailDto>(d => d.Id == id);
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await DeleteOrUpdateAsync([id], false);
        }
        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
    }
}
