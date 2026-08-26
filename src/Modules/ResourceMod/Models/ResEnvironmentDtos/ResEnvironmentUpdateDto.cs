namespace ResourceMod.Models.ResEnvironmentDtos;

/// <summary>
/// 资源环境更新请求结构。
/// </summary>
/// <inheritdoc cref="ResEnvironment"/>
public class ResEnvironmentUpdateDto
{
    /// <summary>
    /// 环境名称。
    /// </summary>
    [MaxLength(60)] public required string Name { get; set; }

    /// <summary>
    /// Material Icons 图标名称，可选。
    /// </summary>
    [MaxLength(60)] public string? Icon { get; set; }

    /// <summary>
    /// 显示颜色，例如 CSS 十六进制颜色值。
    /// </summary>
    [MaxLength(20)] public required string Color { get; set; }
}
