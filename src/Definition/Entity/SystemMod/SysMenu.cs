using System.Text.Json.Serialization;

namespace Entity.SystemMod;

/// <summary>
/// 系统菜单
/// </summary>
public class SysMenu : EntityBase, ITreeNode<SysMenu>
{
    /// <summary>
    /// 菜单名称
    /// </summary>
    [MaxLength(60)]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 菜单路径
    /// </summary>
    [MaxLength(100)]
    public string? Path { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [MaxLength(30)]
    public string? Icon { get; set; }

    /// <summary>
    /// 父菜单
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    [NotMapped]
    public SysMenu? Parent { get; set; }
    public Guid? ParentId { get; set; }

    /// <summary>
    ///  是否有效
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// 子菜单
    /// </summary>
    [NotMapped]
    public List<SysMenu> Children { get; set; } = [];

    /// <summary>
    /// 所属角色关联
    /// </summary>
    [NotMapped]
    public ICollection<SysMenuRole> SysMenuRoles { get; set; } = [];

    /// <summary>
    /// 权限编码
    /// </summary>
    [MaxLength(50)]
    [Required]
    public string AccessCode { get; set; } = string.Empty;

    /// <summary>
    /// 菜单类型
    /// </summary>
    [NotMapped]
    public MenuType MenuType
    {
        get => (MenuType)MenuTypeValue;
        set => MenuTypeValue = (int)value;
    }

    [Column("MenuType")]
    [JsonIgnore]
    public int MenuTypeValue { get; set; } = (int)MenuType.Page;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否显示
    /// </summary>
    public bool Hidden { get; set; } = true;
}

public enum MenuType
{
    /// <summary>
    /// 页面
    /// </summary>
    [Description("页面")]
    Page,

    /// <summary>
    /// 按钮
    /// </summary>
    [Description("按钮")]
    Button,
}
