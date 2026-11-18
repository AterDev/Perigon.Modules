namespace ResourceMod.Models.ResTagDtos;

/// <summary>
/// 资源标签更新请求结构。
/// </summary>
/// <inheritdoc cref="ResTag"/>
public class ResTagUpdateDto
{
    /// <summary>
    /// 标签名称。
    /// </summary>
    [StringLength(60)] public required string Name { get; set; }

    /// <summary>
    /// 显示颜色，例如 CSS 十六进制颜色值。
    /// </summary>
    [StringLength(20)] public required string Color { get; set; }

    /// <summary>
    /// Material Icons 图标名称，可选。
    /// </summary>
    [StringLength(60)] public string? Icon { get; set; }
}

