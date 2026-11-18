using CMSMod.Managers;
using CMSMod.Models.ArticleDtos;
using CMSMod.Services;
using Entity.CMSMod;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Perigon.AspNetCore.Models;
using Share.Implement;

namespace ApiService.Endpoints.CMSMod;

/// <summary>文章管理 Minimal API endpoints.</summary>
public sealed class ArticleEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/Article", "Article", WebConst.User);
        group.MapGet("list", ListAsync);
        group.MapPost("", AddAsync);
        group.MapPost("images", UploadImageAsync)
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery();
        group.MapGet("{id:guid}", DetailAsync);
        group.MapPatch("{id:guid}", UpdateAsync);
        group.MapDelete("{id:guid}", DeleteAsync);
    }

    public static Task<PageList<ArticleItemDto>> ListAsync(
        [AsParameters] ArticleFilterDto filter,
        ArticleManager manager) => manager.ToPageAsync(filter);

    public static async Task<ApiResult<Article>> AddAsync(
        ArticleAddDto dto,
        ArticleManager manager)
    {
        Article entity = await manager.AddAsync(dto);
        return TypedResults.Created($"/api/Article/{entity.Id}", entity);
    }

    public static Task<ArticleImageUploadDto> UploadImageAsync(
        IFormFile file,
        ArticleImageStorageService imageStorage,
        CancellationToken cancellationToken) => imageStorage.SaveAsync(file, cancellationToken);

    public static async Task<ApiResult<ArticleDetailDto>> DetailAsync(
        Guid id,
        ArticleManager manager)
    {
        ArticleDetailDto? entity = await manager.GetAsync(id);
        return ApiResult<ArticleDetailDto>.FromResult(
            entity is null ? NotFound() : TypedResults.Ok(entity));
    }

    public static Task<Article> UpdateAsync(
        Guid id,
        ArticleUpdateDto dto,
        ArticleManager manager) => manager.EditAsync(id, dto);

    public static async Task<NoContent> DeleteAsync(Guid id, ArticleManager manager)
    {
        await manager.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}
