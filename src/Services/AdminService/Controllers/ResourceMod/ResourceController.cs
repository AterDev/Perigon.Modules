using Entity.ResourceMod;
using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.ResourceMod;

public class ResourceController(
    Localizer localizer,
    IUserContext user,
    ILogger<ResourceManager> logger,
    ResourceManager manager
) : RestControllerBase<ResourceManager>(localizer, manager, user, logger)
{
    [HttpGet("list")]
    public Task<PageList<ResourceItemDto>> ListAsync([FromQuery] ResourceFilterDto filter)
    {
        return _manager.FilterAsync(filter);
    }

    [HttpGet("{id}")]
    public Task<ResourceDetailDto?> DetailAsync(Guid id)
    {
        return _manager.GetAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<ResourceCreatedDto>> AddAsync(ResourceAddDto input)
    {
        Resource resource = await _manager.AddAsync(input);
        return Created($"/api/Resource/{resource.Id}", new ResourceCreatedDto { Id = resource.Id });
    }

    [HttpPatch("{id}")]
    public Task<bool> UpdateAsync(Guid id, ResourceUpdateDto input)
    {
        return _manager.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public Task<bool> DeleteAsync(Guid id)
    {
        return _manager.DeleteAsync(id);
    }
}
