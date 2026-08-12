namespace Entity.ResourceMod;

/// <summary>资源分类配置。</summary>
[Index(nameof(TenantId), nameof(CatalogCode), IsUnique = true)]
public class ResCategory : EntityBase
{
    /// <summary>分类名称。</summary>
    [MaxLength(60)]
    public required string Name { get; set; }
    /// <summary>分类编码。</summary>
    [MaxLength(60)]
    public required string CatalogCode { get; set; }
    /// <summary>Material Icons 图标名称，以字符串形式持久化。</summary>
    [MaxLength(60)]
    public string? Icon { get; set; }
    /// <summary>显示颜色，例如 CSS 十六进制颜色值。</summary>
    [MaxLength(20)]
    public required string Color { get; set; }
    /// <summary>属于此分类的分组。</summary>
    public ICollection<ResGroup> Groups { get; set; } = [];
    /// <summary>属于此分类的资源。</summary>
    public ICollection<Resource> Resources { get; set; } = [];
    /// <summary>此分类下的角色授权。</summary>
    public ICollection<ResPermission> Permissions { get; set; } = [];
}
