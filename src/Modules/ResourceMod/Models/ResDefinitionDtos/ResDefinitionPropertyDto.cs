namespace ResourceMod.Models.ResDefinitionDtos;

/// <summary>
/// 资源定义中的属性配置请求结构。
/// </summary>
/// <inheritdoc cref="ResDefinitionProperty"/>
public class ResDefinitionPropertyDto
{
    /// <summary>
    /// 已有资源属性的唯一标识；为空时按名称匹配已有属性或创建新属性。
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// 属性名称。
    /// </summary>
    [MaxLength(60)] public required string Name { get; set; }

    /// <summary>
    /// 属性值类型。
    /// </summary>
    public ResValueType ValueType { get; set; }

    /// <summary>
    /// 是否为必填属性。
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 属性值最大长度，取值范围为 1 到 1000。
    /// </summary>
    [Range(1, 1000)] public int MaxLength { get; set; } = 200;

    /// <summary>
    /// 属性在资源定义中的显示排序。
    /// </summary>
    public int Sort { get; set; }
}
