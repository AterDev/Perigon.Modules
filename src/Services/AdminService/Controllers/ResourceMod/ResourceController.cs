using Entity.ResourceMod;
using Perigon.AspNetCore.Models;

namespace AdminService.Controllers.ResourceMod;

/// <summary>
/// 资源管理。
/// </summary>
/// <see cref="ResourceManager"/>
public class ResourceController(
    Localizer localizer,
    IUserContext user,
    ILogger<ResourceManager> logger,
    ResourceManager manager
) : RestControllerBase<ResourceManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 分页查询资源列表。
    /// </summary>
    /// <param name="filter">资源查询筛选条件。</param>
    /// <returns>符合条件的分页资源列表。</returns>
    [HttpGet("list")]
    public Task<PageList<ResourceItemDto>> ListAsync([FromQuery] ResourceFilterDto filter)
    {
        return _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 获取资源详情。
    /// </summary>
    /// <param name="id">资源唯一标识。</param>
    /// <returns>资源详情；资源不存在或当前用户无权访问时由业务异常处理。</returns>
    [HttpGet("{id}")]
    public Task<ResourceDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return _manager.GetAsync(id);
    }

    /// <summary>
    /// 新增资源。
    /// </summary>
    /// <param name="input">资源新增信息，包括关联配置、标签和属性值。</param>
    /// <returns>新创建资源的唯一标识。</returns>
    [HttpPost]
    public async Task<ActionResult<ResourceCreatedDto>> AddAsync(ResourceAddDto input)
    {
        Resource resource = await _manager.AddAsync(input);
        return Created($"/api/Resource/{resource.Id}", new ResourceCreatedDto { Id = resource.Id });
    }

    /// <summary>
    /// 更新资源。
    /// </summary>
    /// <param name="id">资源唯一标识。</param>
    /// <param name="input">资源更新信息，包括关联配置、标签和属性值。</param>
    /// <returns>更新是否成功。</returns>
    [HttpPatch("{id}")]
    public Task<bool> UpdateAsync([FromRoute] Guid id, ResourceUpdateDto input)
    {
        return _manager.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除资源。
    /// </summary>
    /// <param name="id">资源唯一标识。</param>
    /// <returns>删除是否成功。</returns>
    [HttpDelete("{id}")]
    public Task<bool> DeleteAsync([FromRoute] Guid id)
    {
        return _manager.DeleteAsync(id);
    }
}
