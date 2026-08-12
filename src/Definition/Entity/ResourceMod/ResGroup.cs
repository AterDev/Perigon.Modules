namespace Entity.ResourceMod;

/// <summary>资源分组配置。</summary>
public class ResGroup : EntityBase
{
    /// <summary>分组名称。</summary>
    [MaxLength(60)]
    public required string Name { get; set; }
    /// <summary>分组描述。</summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    /// <summary>Material Icons 图标名称，以字符串形式持久化。</summary>
    [MaxLength(60)]
    public string? Icon { get; set; }
    /// <summary>显示颜色，例如 CSS 十六进制颜色值。</summary>
    [MaxLength(20)]
    public required string Color { get; set; }
    /// <summary>所属分类 ID。</summary>
    public Guid CategoryId { get; set; }
    /// <summary>所属分类。</summary>
    [ForeignKey(nameof(CategoryId))]
    public ResCategory Category { get; set; } = null!;
    /// <summary>属于此分组的资源。</summary>
    public ICollection<Resource> Resources { get; set; } = [];
}
