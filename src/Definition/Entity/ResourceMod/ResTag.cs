namespace Entity.ResourceMod;

/// <summary>资源标签配置。</summary>
public class ResTag : EntityBase
{
    /// <summary>标签名称。</summary>
    [StringLength(60)]
    public string Name { get; set; } = string.Empty;
    /// <summary>显示颜色，例如 CSS 十六进制颜色值。</summary>
    [StringLength(20)]
    public string Color { get; set; } = string.Empty;
    /// <summary>Material Icons 图标名称，以字符串形式持久化。</summary>
    [StringLength(60)]
    public string? Icon { get; set; }
}
