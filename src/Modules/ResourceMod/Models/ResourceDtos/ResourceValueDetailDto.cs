namespace ResourceMod.Models.ResourceDtos;

/// <summary>
/// 资源属性值详情响应结构。
/// </summary>
public class ResourceValueDetailDto
{
    /// <summary>
    /// 资源定义属性 ID。
    /// </summary>
    public Guid DefinitionPropertyId { get; set; }

    /// <summary>
    /// 保存时的属性名称快照。
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 保存时的属性值类型快照。
    /// </summary>
    public ResValueType ValueType { get; set; }

    /// <summary>
    /// 属性值。
    /// </summary>
    public required string Value { get; set; }
}
