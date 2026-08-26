namespace ResourceMod.Models.ResourceDtos;

/// <summary>
/// 资源详情响应结构，包含资源的动态属性值。
/// </summary>
/// <inheritdoc cref="Resource"/>
public class ResourceDetailDto : ResourceItemDto
{
    /// <summary>
    /// 资源属性值列表，包含保存时的名称和类型快照。
    /// </summary>
    public List<ResourceValueDetailDto> Values { get; set; } = [];
}
