using CMSMod.Models.ArticleDtos;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Perigon.AspNetCore.Abstraction;
using Perigon.PostgreSQL.Update;
using Microsoft.AspNetCore.Http;
using Share.Exceptions;

namespace CMSMod.Managers;

/// <summary>
/// 博客
/// </summary>
public class ArticleManager(
    AppDbFactory dbContextFactory,
    ILogger<ArticleManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, Article>(dbContextFactory, userContext, logger)
{
    public async Task<PageList<ArticleItemDto>> ToPageAsync(ArticleFilterDto filter)
    {
        int languageType = (int)filter.LanguageType.GetValueOrDefault();
        int contentType = (int)filter.BlogType.GetValueOrDefault();
        Queryable = Queryable
            .WhereNotNull(filter.Title, q => q.Title == filter.Title)
            .WhereNotNull(filter.LanguageType, q => q.LanguageTypeValue == languageType)
            .WhereNotNull(filter.BlogType, q => q.ContentTypeValue == contentType)
            .WhereNotNull(filter.IsAudit, q => q.IsAudit == filter.IsAudit)
            .WhereNotNull(filter.IsPublic, q => q.IsPublic == filter.IsPublic)
            .WhereNotNull(filter.IsOriginal, q => q.IsOriginal == filter.IsOriginal)
            .WhereNotNull(filter.UserId, q => q.UserId == filter.UserId)
            .WhereNotNull(filter.CatalogId, q => q.CatalogId == filter.CatalogId);


        return await PageListAsync<ArticleFilterDto, ArticleItemDto>(filter, MapItem);
    }

    private static ArticleItemDto MapItem(Article x) => new() { Id = x.Id, Title = x.Title, Description = x.Description, Authors = x.Authors, LanguageType = x.LanguageType, BlogType = x.ContentType, IsAudit = x.IsAudit, IsPublic = x.IsPublic, IsOriginal = x.IsOriginal, UserId = x.UserId, CatalogId = x.CatalogId, ViewCount = x.ViewCount, CreatedTime = x.CreatedTime, UpdatedTime = x.UpdatedTime };

    public async Task<Article> AddAsync(ArticleAddDto dto)
    {
        await EnsureCategoryAsync(dto.CatalogId);
        Article entity = new()
        {
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            Authors = GetCurrentAuthorName(),
            LanguageTypeValue = (int)dto.LanguageType,
            ContentTypeValue = (int)dto.BlogType,
            IsAudit = false,
            IsPublic = dto.IsPublic,
            IsOriginal = dto.IsOriginal,
            UserId = _userContext.UserId,
            CatalogId = dto.CatalogId,
            ViewCount = 0,
        };
        await InsertAsync(entity);
        return entity;
    }

    public async Task<ArticleDetailDto?> GetAsync(Guid id)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException("无权查看文章", StatusCodes.Status403Forbidden);
        }

        Article? entity = await _dbSet
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();
        if (entity is null)
        {
            return null;
        }

        ArticleCategory? catalog = await _dbContext.ArticleCategories
            .Where(c => c.Id == entity.CatalogId)
            .FirstOrDefaultAsync();
        return new ArticleDetailDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Content = entity.Content,
            Authors = entity.Authors,
            LanguageType = entity.LanguageType,
            BlogType = entity.ContentType,
            IsAudit = entity.IsAudit,
            IsPublic = entity.IsPublic,
            IsOriginal = entity.IsOriginal,
            UserId = entity.UserId,
            CatalogId = entity.CatalogId,
            Catalog = catalog!,
            ViewCount = entity.ViewCount,
            CreatedTime = entity.CreatedTime,
            UpdatedTime = entity.UpdatedTime,
        };
    }

    public async Task<Article> EditAsync(Guid id, ArticleUpdateDto dto)
    {
        Article entity = await GetOwnedAsync(id);
        Guid catalogId = dto.CatalogId ?? entity.CatalogId;
        await EnsureCategoryAsync(catalogId);
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Content = dto.Content;
        entity.LanguageTypeValue = dto.LanguageType is { } languageType
            ? (int)languageType
            : entity.LanguageTypeValue;
        entity.ContentTypeValue = dto.BlogType is { } contentType
            ? (int)contentType
            : entity.ContentTypeValue;
        entity.IsPublic = dto.IsPublic ?? entity.IsPublic;
        entity.IsOriginal = dto.IsOriginal ?? entity.IsOriginal;
        entity.CatalogId = catalogId;
        entity.UpdatedTime = DateTimeOffset.UtcNow;
        await _dbSet
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(update => update
                .Set(a => a.Title, entity.Title)
                .Set(a => a.Description, entity.Description)
                .Set(a => a.Content, entity.Content)
                .Set(a => a.LanguageTypeValue, entity.LanguageTypeValue)
                .Set(a => a.ContentTypeValue, entity.ContentTypeValue)
                .Set(a => a.IsPublic, entity.IsPublic)
                .Set(a => a.IsOriginal, entity.IsOriginal)
                .Set(a => a.CatalogId, entity.CatalogId)
                .Set(a => a.UpdatedTime, entity.UpdatedTime));
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        await GetOwnedAsync(id);
        await DeleteOrUpdateAsync([id], true);
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id);
        if (!_userContext.IsAdmin)
        {
            query = query.Where(q => q.UserId == _userContext.UserId);
        }

        return await query.AnyAsync();
    }

    private async Task<Article> GetOwnedAsync(Guid id)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException("无权维护文章", StatusCodes.Status403Forbidden);
        }

        Article? entity = await _dbSet
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();
        return entity ?? throw new BusinessException("文章不存在", StatusCodes.Status404NotFound);
    }

    private async Task EnsureCategoryAsync(Guid id)
    {
        bool exists = await _dbContext.ArticleCategories
            .Where(c => c.Id == id
                && (_userContext.IsAdmin || c.UserId == _userContext.UserId))
            .AnyAsync();
        if (!exists)
        {
            throw new BusinessException("文章分类不存在", StatusCodes.Status400BadRequest);
        }
    }

    private string GetCurrentAuthorName()
    {
        if (!string.IsNullOrWhiteSpace(_userContext.UserName))
        {
            return _userContext.UserName!;
        }

        if (!string.IsNullOrWhiteSpace(_userContext.Email))
        {
            return _userContext.Email!;
        }

        return _userContext.UserId.ToString();
    }
}
