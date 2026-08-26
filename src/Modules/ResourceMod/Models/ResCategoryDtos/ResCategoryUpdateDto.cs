namespace ResourceMod.Models.ResCategoryDtos;

/// <summary>
/// 资源分类更新请求结构。
/// </summary>
/// <inheritdoc cref="ResCategory"/>
public class ResCategoryUpdateDto
{
    /// <summary>
    /// 分类名称。
    /// </summary>
    [MaxLength(60)] public required string Name { get; set; }

    /// <summary>
    /// 分类编码，在当前租户内必须唯一。
    /// </summary>
    [MaxLength(60)] public required string CatalogCode { get; set; }

    /// <summary>
    /// Material Icons 图标名称，可选。
    /// </summary>
    [MaxLength(60)] public string? Icon { get; set; }

    /// <summary>
    /// 显示颜色，例如 CSS 十六进制颜色值。
    /// </summary>
    [MaxLength(20)] public required string Color { get; set; }
}
