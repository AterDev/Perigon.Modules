namespace ResourceMod.Models.ResDefinitionDtos;

/// <summary>
/// 资源定义新增请求结构。
/// </summary>
/// <inheritdoc cref="ResDefinition"/>
public class ResDefinitionAddDto
{
    /// <summary>
    /// 资源定义名称。
    /// </summary>
    [MaxLength(60)] public required string Name { get; set; }

    /// <summary>
    /// Material Icons 图标名称，可选。
    /// </summary>
    [MaxLength(60)] public string? Icon { get; set; }

    /// <summary>
    /// 资源定义包含的属性配置，按 Sort 排序。
    /// </summary>
    public List<ResDefinitionPropertyDto> Properties { get; set; } = [];
}
