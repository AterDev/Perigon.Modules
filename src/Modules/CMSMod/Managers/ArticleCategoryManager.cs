using Perigon.AspNetCore.Abstraction;
using CMSMod.Models.ArticleCategoryDtos;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using Perigon.PostgreSQL.Update;
using Share;
using Share.Exceptions;

namespace CMSMod.Managers;

/// <summary>
/// 目录管理
/// </summary>
public class ArticleCategoryManager(
    AppDbFactory dbContextFactory,
    ILogger<ArticleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, ArticleCategory>(dbContextFactory, userContext, logger)
{
    /// <summary>
    /// add catalog
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ArticleCategory> AddAsync(ArticleCategoryAddDto dto)
    {
        if (dto.ParentId is Guid parentId)
        {
            await EnsureParentAsync(parentId);
        }

        var entity = new ArticleCategory
        {
            Name = dto.Name,
            ParentId = dto.ParentId,
            UserId = _userContext.UserId,
        };
        await InsertAsync(entity);
        return entity;
    }


    /// <summary>
    /// edit catalog
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="BusinessException"></exception>
    public async Task<int> EditAsync(Guid id, ArticleCategoryUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await _dbSet
                .Where(c => c.Id == id && c.TenantId == _userContext.TenantId)
                .ExecuteUpdateAsync(update => update
                    .Set(c => c.Name, dto.Name)
                    .Set(c => c.UpdatedTime, DateTimeOffset.UtcNow));
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    public async Task<ArticleCategoryDetailDto?> GetAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            ArticleCategory? entity = await _dbSet
                .Where(q => q.Id == id && q.TenantId == _userContext.TenantId)
                .FirstOrDefaultAsync();
            if (entity is null)
                return null;

            ArticleCategory? parent = entity.ParentId is Guid parentId
                ? await _dbContext.ArticleCategories
                    .Where(q => q.Id == parentId && q.TenantId == _userContext.TenantId)
                    .FirstOrDefaultAsync()
                : null;
            return new ArticleCategoryDetailDto
            {
                Name = entity.Name,
                Level = entity.Level,
                Parent = parent,
                ParentId = entity.ParentId,
                CreatedTime = entity.CreatedTime,
                UpdatedTime = entity.UpdatedTime,
            };
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    public async Task<PageList<ArticleCategoryItemDto>> FilterAsync(ArticleCategoryFilterDto filter)
    {
        Queryable = Queryable.WhereNotNull(filter.Name, q => q
            .Name
            .Contains(filter.Name!));
        return await PageListAsync<ArticleCategoryFilterDto, ArticleCategoryItemDto>(filter, MapItem);
    }

    private static ArticleCategoryItemDto MapItem(ArticleCategory x) => new() { Id = x.Id, Name = x.Name, Level = x.Level, ParentId = x.ParentId, CreatedTime = x.CreatedTime };

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

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        query = query.Where(q => q.UserId == _userContext.UserId);
        return await query.AnyAsync();
    }
    public async Task<List<Guid>> GetOwnedIdsAsync(IEnumerable<Guid> ids)
    {
        Guid[] requestedIds = ids.Distinct().ToArray();
        if (requestedIds.Length == 0)
        {
            return [];
        }
        var query = _dbSet
            .Where(q => requestedIds.Contains(q.Id)
                && q.TenantId == _userContext.TenantId
                && (_userContext.IsAdmin || q.UserId == _userContext.UserId))
            .ToListAsync();
        List<ArticleCategory> owned = await query;
        return owned.Select(q => q.Id).ToList();
    }

    /// <summary>
    /// 获取树型目录
    /// </summary>
    /// <returns></returns>
    public async Task<List<ArticleCategory>> GetTreeAsync()
    {
        List<ArticleCategory> data = await _dbSet.ToListAsync();
        List<ArticleCategory> tree = data.BuildTree();
        return tree;
    }

    /// <summary>
    /// 获取叶结点目录
    /// </summary>
    /// <returns></returns>
    public async Task<List<ArticleCategory>> GetLeafCatalogsAsync()
    {
        List<ArticleCategory> all = await _dbSet.ToListAsync();
        HashSet<Guid> parentIds = all
            .Where(c => c.ParentId is not null)
            .Select(c => c.ParentId!.Value)
            .ToHashSet();

        List<ArticleCategory> source = all
            .Where(c => !parentIds.Contains(c.Id))
            .ToList();
        Guid[] sourceParentIds = source
            .Where(c => c.ParentId is not null)
            .Select(c => c.ParentId!.Value)
            .Distinct()
            .ToArray();
        if (sourceParentIds.Length > 0)
        {
            Dictionary<Guid, ArticleCategory> parentLookup = all.ToDictionary(c => c.Id);
            foreach (ArticleCategory category in source)
            {
                if (category.ParentId is Guid parentId && parentLookup.TryGetValue(parentId, out ArticleCategory? parent))
                    category.Parent = parent;
            }
        }
        return source;
    }

    private async Task EnsureParentAsync(Guid parentId)
    {
        bool exists = await _dbContext.ArticleCategories
            .Where(category => category.Id == parentId
                && category.TenantId == _userContext.TenantId
                && (_userContext.IsAdmin || category.UserId == _userContext.UserId))
            .AnyAsync();
        if (!exists)
        {
            throw new BusinessException("父目录不存在", StatusCodes.Status400BadRequest);
        }
    }
}
