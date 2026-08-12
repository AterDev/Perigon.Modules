namespace Entity.ResourceMod;

/// <summary>资源标签配置。</summary>
public class ResTag : EntityBase
{
    /// <summary>标签名称。</summary>
    [MaxLength(60)]
    public required string Name { get; set; }
    /// <summary>显示颜色，例如 CSS 十六进制颜色值。</summary>
    [MaxLength(20)]
    public required string Color { get; set; }
    /// <summary>Material Icons 图标名称，以字符串形式持久化。</summary>
    [MaxLength(60)]
    public string? Icon { get; set; }
}
