namespace Entity.ResourceMod;

/// <summary>资源分类配置。</summary>
public class ResCategory : EntityBase
{
    /// <summary>分类名称。</summary>
    [StringLength(60)]
    public string Name { get; set; } = string.Empty;
    /// <summary>分类编码。</summary>
    [StringLength(60)]
    public string CatalogCode { get; set; } = string.Empty;
    /// <summary>Material Icons 图标名称，以字符串形式持久化。</summary>
    [StringLength(60)]
    public string? Icon { get; set; }
    /// <summary>显示颜色，例如 CSS 十六进制颜色值。</summary>
    [StringLength(20)]
    public string Color { get; set; } = string.Empty;
    /// <summary>属于此分类的分组。</summary>
    [NotMapped]
    public ICollection<ResGroup> Groups { get; set; } = [];
    /// <summary>属于此分类的资源。</summary>
    [NotMapped]
    public ICollection<Resource> Resources { get; set; } = [];
    /// <summary>此分类下的角色授权。</summary>
    [NotMapped]
    public ICollection<ResPermission> Permissions { get; set; } = [];
}
