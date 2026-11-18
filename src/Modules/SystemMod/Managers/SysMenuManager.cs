using EntityFramework.AppDbFactory;
using SystemMod.Models.SysMenuDtos;

namespace SystemMod.Managers;

/// <summary>
/// 系统菜单
/// </summary>
public class SysMenuManager(
    AppDbFactory dbContextFactory,
    ILogger<SysMenuManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, SysMenu>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<SysMenu> AddAsync(SysMenuAddDto dto)
    {
        var entity = new SysMenu { Name = dto.Name, Path = dto.Path, Icon = dto.Icon, ParentId = dto.ParentId, IsValid = dto.IsValid, AccessCode = dto.AccessCode, MenuType = dto.MenuType, Sort = dto.Sort, Hidden = dto.Hidden };
        // other property set
        if (dto.ParentId != null)
        {
            entity.ParentId = dto.ParentId.Value;
        }
        await InsertAsync(entity);
        return entity;
    }

    public async Task<bool> EditAsync(Guid id, SysMenuUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            var current = await _dbSet.Where(menu => menu.Id == id).FirstOrDefaultAsync()
                ?? throw new BusinessException(Localizer.NotFoundResource, StatusCodes.Status404NotFound);
            await _dbSet
                .Where(menu => menu.Id == id)
                .ExecuteUpdateAsync(update => update
                    .Set(menu => menu.Name, dto.Name ?? current.Name)
                    .Set(menu => menu.Path, dto.Path ?? current.Path)
                    .Set(menu => menu.Icon, dto.Icon ?? current.Icon)
                    .Set(menu => menu.IsValid, dto.IsValid ?? current.IsValid)
                    .Set(menu => menu.AccessCode, dto.AccessCode ?? current.AccessCode)
                    .Set(menu => menu.MenuTypeValue, (int)(dto.MenuType ?? current.MenuType))
                    .Set(menu => menu.Sort, dto.Sort ?? current.Sort)
                    .Set(menu => menu.Hidden, dto.Hidden ?? current.Hidden));
            return true;
        }
        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// 菜单同步
    /// </summary>
    /// <param name="menus"></param>
    /// <returns></returns>
    public async Task<bool> SyncSysMenusAsync(List<SysMenuSyncDto> menus)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            List<SysMenu> currentMenus = await _dbSet.ToListAsync();
            List<SysMenu> flatMenus = FlatTree(menus);
            var byAccessCode = currentMenus.ToDictionary(m => m.AccessCode, StringComparer.Ordinal);
            var accessCodes = flatMenus.Select(m => m.AccessCode).ToHashSet(StringComparer.Ordinal);
            var removed = currentMenus.Where(m => !accessCodes.Contains(m.AccessCode)).ToList();
            if (removed.Count != 0)
            {
                var removedIds = removed.Select(m => m.Id).ToArray();
                await _dbContext.SysMenuRoles
                    .Where(link => removedIds.Contains(link.MenuId))
                    .ExecuteDeleteAsync();
                await _dbSet.Where(menu => removedIds.Contains(menu.Id)).ExecuteDeleteAsync();
            }

            var changed = removed.Count != 0;
            foreach (SysMenu menu in flatMenus)
            {
                if (byAccessCode.TryGetValue(menu.AccessCode, out var existing))
                {
                    var count = await _dbSet.Where(item => item.Id == existing.Id).ExecuteUpdateAsync(update => update
                        .Set(item => item.Name, menu.Name)
                        .Set(item => item.Sort, menu.Sort)
                        .Set(item => item.Icon, menu.Icon));
                    changed |= count > 0;
                    continue;
                }

                if (menu.Parent is not null && byAccessCode.TryGetValue(menu.Parent.AccessCode, out var parent))
                {
                    menu.ParentId = parent.Id;
                }
                menu.Parent = null;
                await _dbSet.InsertAsync(menu);
                byAccessCode[menu.AccessCode] = menu;
                changed = true;
            }
            return changed;
        });
    }

    /// <summary>
    /// flat tree
    /// </summary>
    /// <param name="list"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private static List<SysMenu> FlatTree(
        List<SysMenuSyncDto> list,
        SysMenu? parent = null
    )
    {
        var res = new List<SysMenu>();
        foreach (SysMenuSyncDto item in list)
        {
            if (item.Children.Count != 0)
            {
                var menu = new SysMenu
                {
                    Name = item.Name,
                    AccessCode = item.AccessCode,
                    MenuType = (MenuType)item.MenuType,
                    Parent = parent,
                    Sort = item.Sort ?? 0,
                    Icon = item.Icon,
                };
                res.Add(menu);
                List<SysMenu> children = FlatTree(item.Children, menu);
                res.AddRange(children);
            }
            else
            {
                var menu = new SysMenu
                {
                    Name = item.Name,
                    AccessCode = item.AccessCode,
                    MenuType = (MenuType)item.MenuType,
                    Parent = parent,
                    Sort = item.Sort ?? 0,
                    Icon = item.Icon,
                };
                res.Add(menu);
            }
        }
        return res;
    }

    public async Task<PageList<SysMenu>> FilterAsync(SysMenuFilterDto filter)
    {
        List<SysMenu>? menus;
        if (filter.RoleId != null)
        {
            var menuLinks = await _dbContext.SysMenuRoles
                .Where(link => link.RoleId == filter.RoleId)
                .ToListAsync();
            var menuIds = menuLinks.Select(link => link.MenuId).Distinct().ToArray();
            menus = await Queryable.Where(q => menuIds.Contains(q.Id)).ToListAsync();
            menus.BuildTree();
        }
        else if (filter.ParentId != null)
        {
            menus = await Queryable
                .Where(q => q.ParentId == filter.ParentId)
                .ToListAsync();
        }
        else
        {
            menus = await Queryable
                .OrderByDescending(t => t.Sort)
                .ThenByDescending(t => t.CreatedTime)
                .Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20))
                .Take(filter.PageSize.GetValueOrDefault(20))
                .ToListAsync();
            menus = menus.BuildTree();
        }

        return new PageList<SysMenu>() { Data = menus, PageIndex = filter.PageIndex.GetValueOrDefault(1) };
    }

    public async Task<bool> DeleteAsync(IEnumerable<Guid> ids, bool isDelete = false)
    {
        if (!ids.Any())
        {
            return false;
        }

        if (ids.Count() == 1)
        {
            Guid id = ids.First();
            if (await HasPermissionAsync(id))
            {
                return await DeleteOrUpdateAsync(ids, !isDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
        else
        {
            var ownedIds = await GetOwnedIdsAsync(ids);
            if (ownedIds.Any())
            {
                return await DeleteOrUpdateAsync(ownedIds, !isDelete) > 0;
            }
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }
    }

    /// <summary>
    /// wheather user have edit/delete permission
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id);
        // TODO: other conditions
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        if (!ids.Any())
        {
            return [];
        }
        Guid[] ownedIds = ids.ToArray();
        var ownedMenus = await _dbSet
            .Where(q => ownedIds.Contains(q.Id))
            // TODO: other conditions
            .ToListAsync();
        return ownedMenus.Select(menu => menu.Id).ToList();
    }
}

