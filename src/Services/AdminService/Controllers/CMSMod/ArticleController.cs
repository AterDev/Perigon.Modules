using CMSMod.Managers;
using CMSMod.Models.ArticleDtos;
using CMSMod.Services;
using Entity.CMSMod;
using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.CMSMod;

/// <summary>
/// 文章管理。
/// </summary>
/// <see cref="ArticleManager"/>
public class ArticleController(
    Localizer localizer,
    IUserContext user,
    ILogger<ArticleManager> logger,
    ArticleManager manager,
    ArticleImageStorageService imageStorage
) : RestControllerBase<ArticleManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 分页查询文章列表。
    /// </summary>
    /// <param name="filter">文章查询筛选条件。</param>
    /// <returns>符合条件的分页文章列表。</returns>
    [HttpGet("list")]
    public Task<PageList<ArticleItemDto>> ListAsync([FromQuery] ArticleFilterDto filter)
    {
        return _manager.ToPageAsync(filter);
    }


    /// <summary>
    /// 新增文章。
    /// </summary>
    /// <param name="dto">文章新增信息。</param>
    /// <returns>创建的文章实体。</returns>
    [HttpPost]
    public async Task<ActionResult<Article>> AddAsync(ArticleAddDto dto)
    {
        Article entity = await _manager.AddAsync(dto);
        return Created($"/api/Article/{entity.Id}", entity);
    }

    /// <summary>
    /// 上传文章图片。
    /// </summary>
    /// <param name="file">要上传的图片文件。</param>
    /// <param name="cancellationToken">取消上传操作的令牌。</param>
    /// <returns>可用于文章内容的图片路径。</returns>
    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    public Task<ArticleImageUploadDto> UploadImageAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        return imageStorage.SaveAsync(file, cancellationToken);
    }

    /// <summary>
    /// 获取文章详情。
    /// </summary>
    /// <param name="id">文章唯一标识。</param>
    /// <returns>文章详情；文章不存在时返回 404。</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleDetailDto>> DetailAsync([FromRoute] Guid id)
    {
        ArticleDetailDto? entity = await _manager.GetAsync(id);
        return entity is null ? NotFound() : entity;
    }

    /// <summary>
    /// 更新文章。
    /// </summary>
    /// <param name="id">文章唯一标识。</param>
    /// <param name="dto">文章更新信息。</param>
    /// <returns>更新后的文章实体。</returns>
    [HttpPatch("{id:guid}")]
    public Task<Article> UpdateAsync([FromRoute] Guid id, ArticleUpdateDto dto)
    {
        return _manager.EditAsync(id, dto);
    }


    /// <summary>
    /// 删除文章。
    /// </summary>
    /// <param name="id">文章唯一标识。</param>
    /// <returns>删除成功时返回 204。</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _manager.DeleteAsync(id);
        return NoContent();
    }
}
