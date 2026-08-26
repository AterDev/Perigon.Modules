namespace ResourceMod.Models.ResourceDtos;

/// <summary>
/// 资源属性值写入请求结构。
/// </summary>
public class ResourceValueDto
{
    /// <summary>
    /// 资源定义属性 ID。
    /// </summary>
    public Guid DefinitionPropertyId { get; set; }

    /// <summary>
    /// 属性值；保存时会根据属性类型规范化，最大长度为 1000。
    /// </summary>
    [MaxLength(1000)] public required string Value { get; set; }
}
