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
    public Task<PageList<ResourceItemDto>> ListAsync([FromQuery] ResourceFilter filter) => _manager.FilterAsync(filter);

    [HttpGet("{id}")]
    public Task<ResourceDetailDto?> DetailAsync(Guid id) => _manager.GetAsync(id);

    [HttpPost]
    public async Task<ActionResult<Resource>> AddAsync(ResourceInput input)
    {
        Resource resource = await _manager.AddAsync(input);
        return CreatedAtAction(nameof(DetailAsync), new { id = resource.Id }, resource);
    }

    [HttpPatch("{id}")]
    public Task<bool> UpdateAsync(Guid id, ResourceInput input) => _manager.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task<bool> DeleteAsync(Guid id) => _manager.DeleteAsync(id);
}
