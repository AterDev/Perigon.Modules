namespace ResourceMod.Models.ResGroupDtos;

/// <summary>
/// 资源分组新增请求结构。
/// </summary>
/// <inheritdoc cref="ResGroup"/>
public class ResGroupAddDto
{
    /// <summary>
    /// 分组名称。
    /// </summary>
    [MaxLength(60)] public required string Name { get; set; }

    /// <summary>
    /// 分组描述，可选。
    /// </summary>
    [MaxLength(500)] public string? Description { get; set; }

    /// <summary>
    /// Material Icons 图标名称，可选。
    /// </summary>
    [MaxLength(60)] public string? Icon { get; set; }

    /// <summary>
    /// 显示颜色，例如 CSS 十六进制颜色值。
    /// </summary>
    [MaxLength(20)] public required string Color { get; set; }

    /// <summary>
    /// 所属资源分类 ID。
    /// </summary>
    public Guid CategoryId { get; set; }
}
