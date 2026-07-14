using CMSMod.Models.ArticleDtos;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Perigon.AspNetCore.Abstraction;
using Microsoft.AspNetCore.Http;
using Share.Exceptions;

namespace CMSMod.Managers;

/// <summary>
/// 博客
/// </summary>
public class ArticleManager(
    TenantDbFactory dbContextFactory,
    ILogger<ArticleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, Article>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ArticleItemDto>> ToPageAsync(ArticleFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Title, q => q.Title == filter.Title)
            .WhereNotNull(filter.LanguageType, q => q.LanguageType == filter.LanguageType)
            .WhereNotNull(filter.BlogType, q => q.ContentType == filter.BlogType)
            .WhereNotNull(filter.IsAudit, q => q.IsAudit == filter.IsAudit)
            .WhereNotNull(filter.IsPublic, q => q.IsPublic == filter.IsPublic)
            .WhereNotNull(filter.IsOriginal, q => q.IsOriginal == filter.IsOriginal)
            .WhereNotNull(filter.UserId, q => q.UserId == filter.UserId)
            .WhereNotNull(filter.CatalogId, q => q.Catalog.Id == filter.CatalogId);


        return await PageListAsync<ArticleFilterDto, ArticleItemDto>(filter);
    }

    public async Task<Article> AddAsync(ArticleAddDto dto)
    {
        await EnsureCategoryAsync(dto.CatalogId);
        Article entity = new()
        {
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            Authors = dto.Authors,
            LanguageType = dto.LanguageType,
            ContentType = dto.BlogType,
            IsAudit = dto.IsAudit,
            IsPublic = dto.IsPublic,
            IsOriginal = dto.IsOriginal,
            UserId = _userContext.UserId,
            CatalogId = dto.CatalogId,
            ViewCount = dto.ViewCount,
            TenantId = _userContext.TenantId,
        };
        await InsertAsync(entity);
        return entity;
    }

    public async Task<ArticleDetailDto?> GetAsync(Guid id)
    {
        if (!await HasPermissionAsync(id))
            throw new BusinessException("无权查看文章", StatusCodes.Status403Forbidden);

        return await _dbSet
            .Where(a => a.Id == id && a.TenantId == _userContext.TenantId)
            .Select(a => new ArticleDetailDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Content = a.Content,
                Authors = a.Authors,
                LanguageType = a.LanguageType,
                BlogType = a.ContentType,
                IsAudit = a.IsAudit,
                IsPublic = a.IsPublic,
                IsOriginal = a.IsOriginal,
                UserId = a.UserId,
                CatalogId = a.CatalogId,
                Catalog = a.Catalog,
                ViewCount = a.ViewCount,
                CreatedTime = a.CreatedTime,
                UpdatedTime = a.UpdatedTime,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Article> EditAsync(Guid id, ArticleUpdateDto dto)
    {
        Article entity = await GetOwnedAsync(id);
        Guid catalogId = dto.CatalogId ?? entity.CatalogId;
        await EnsureCategoryAsync(catalogId);
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Content = dto.Content;
        entity.Authors = dto.Authors;
        entity.LanguageType = dto.LanguageType ?? entity.LanguageType;
        entity.ContentType = dto.BlogType ?? entity.ContentType;
        entity.IsAudit = dto.IsAudit ?? entity.IsAudit;
        entity.IsPublic = dto.IsPublic ?? entity.IsPublic;
        entity.IsOriginal = dto.IsOriginal ?? entity.IsOriginal;
        entity.CatalogId = catalogId;
        entity.ViewCount = dto.ViewCount ?? entity.ViewCount;
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        await GetOwnedAsync(id);
        await DeleteOrUpdateAsync([id], true);
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        if (!_userContext.IsAdmin)
            query = query.Where(q => q.UserId == _userContext.UserId);
        return await query.AnyAsync();
    }

    private async Task<Article> GetOwnedAsync(Guid id)
    {
        if (!await HasPermissionAsync(id))
            throw new BusinessException("无权维护文章", StatusCodes.Status403Forbidden);
        return await _dbSet.FirstAsync(a => a.Id == id && a.TenantId == _userContext.TenantId);
    }

    private async Task EnsureCategoryAsync(Guid id)
    {
        bool exists = await _dbContext.ArticleCategories.AnyAsync(c =>
            c.Id == id
            && c.TenantId == _userContext.TenantId
            && (_userContext.IsAdmin || c.UserId == _userContext.UserId));
        if (!exists)
            throw new BusinessException("文章分类不存在", StatusCodes.Status400BadRequest);
    }
}
