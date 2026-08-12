namespace Entity.ResourceMod;

/// <summary>资源运行环境配置。</summary>
public class ResEnvironment : EntityBase
{
    /// <summary>环境名称。</summary>
    [MaxLength(60)]
    public required string Name { get; set; }
    /// <summary>Material Icons 图标名称，以字符串形式持久化。</summary>
    [MaxLength(60)]
    public string? Icon { get; set; }
    /// <summary>显示颜色，例如 CSS 十六进制颜色值。</summary>
    [MaxLength(20)]
    public required string Color { get; set; }
    /// <summary>属于此环境的资源。</summary>
    public ICollection<Resource> Resources { get; set; } = [];
    /// <summary>此环境下的角色授权。</summary>
    public ICollection<ResPermission> Permissions { get; set; } = [];
}
