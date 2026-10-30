namespace Entity.SystemMod;

/// <summary>
/// 系统角色
/// </summary>
public class SysRole : EntityBase
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [MaxLength(30)]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色标识
    /// </summary>
    [MaxLength(60)]
    [Required]
    public string NameValue { get; set; } = string.Empty;

    /// <summary>
    /// 是否系统内置
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [MaxLength(30)]
    public string? Icon { get; set; }
    [NotMapped]
    public ICollection<SysUserRole> SysUserRoles { get; set; } = [];

    /// <summary>
    /// 菜单权限关联
    /// </summary>
    [NotMapped]
    public ICollection<SysMenuRole> SysMenuRoles { get; set; } = [];
}
