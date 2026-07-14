using CMSMod.Managers;
using CMSMod.Models.ArticleDtos;
using Entity.CMSMod;
using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.CMSMod;

public class ArticleController(
    Localizer localizer,
    IUserContext user,
    ILogger<ArticleManager> logger,
    ArticleManager manager
) : RestControllerBase<ArticleManager>(localizer, manager, user, logger)
{
    [HttpGet("list")]
    public Task<PageList<ArticleItemDto>> ListAsync([FromQuery] ArticleFilterDto filter)
    {
        return _manager.ToPageAsync(filter);
    }


    [HttpPost]
    public async Task<ActionResult<Article>> AddAsync(ArticleAddDto dto)
    {
        Article entity = await _manager.AddAsync(dto);
        return Created($"/api/Article/{entity.Id}", entity);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleDetailDto>> DetailAsync(Guid id)
    {
        ArticleDetailDto? entity = await _manager.GetAsync(id);
        return entity is null ? NotFound() : entity;
    }

    [HttpPatch("{id:guid}")]
    public Task<Article> UpdateAsync(Guid id, ArticleUpdateDto dto)
    {
        return _manager.EditAsync(id, dto);
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _manager.DeleteAsync(id);
        return NoContent();
    }
}
