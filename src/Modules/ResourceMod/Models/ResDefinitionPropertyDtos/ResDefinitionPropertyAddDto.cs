namespace ResourceMod.Models.ResDefinitionPropertyDtos;

/// <summary>
/// 资源属性定义新增请求结构。
/// </summary>
/// <inheritdoc cref="ResDefinitionProperty"/>
public class ResDefinitionPropertyAddDto
{
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
}
