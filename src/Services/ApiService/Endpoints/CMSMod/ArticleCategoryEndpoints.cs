using CMSMod.Managers;
using CMSMod.Models.ArticleCategoryDtos;
using Entity.CMSMod;
using Perigon.AspNetCore.Models;
using Share.Implement;

namespace ApiService.Endpoints.CMSMod;

/// <summary>文章分类 Minimal API endpoints.</summary>
public sealed class ArticleCategoryEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/ArticleCategory", "ArticleCategory", WebConst.User);
        group.MapGet("list", ListAsync);
        group.MapPost("", AddAsync);
        group.MapPatch("{id}", UpdateAsync);
        group.MapGet("{id}", DetailAsync);
        group.MapDelete("{id}", DeleteAsync);
    }

    public static Task<PageList<ArticleCategoryItemDto>> ListAsync(
        [AsParameters] ArticleCategoryFilterDto filter,
        ArticleCategoryManager manager) => manager.FilterAsync(filter);

    public static async Task<ApiResult<ArticleCategory>> AddAsync(
        ArticleCategoryAddDto dto,
        ArticleCategoryManager manager)
    {
        ArticleCategory entity = await manager.AddAsync(dto);
        return TypedResults.Created($"/api/ArticleCategory/{entity.Id}", entity);
    }

    public static async Task<bool> UpdateAsync(
        Guid id,
        ArticleCategoryUpdateDto dto,
        ArticleCategoryManager manager) => await manager.EditAsync(id, dto) == 1;

    public static Task<ArticleCategoryDetailDto?> DetailAsync(
        Guid id,
        ArticleCategoryManager manager) => manager.GetAsync(id);

    public static Task<bool> DeleteAsync(Guid id, ArticleCategoryManager manager) =>
        manager.DeleteAsync([id], false);
}
