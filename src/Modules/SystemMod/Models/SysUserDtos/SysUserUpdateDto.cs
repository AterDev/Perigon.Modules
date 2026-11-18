namespace SystemMod.Models.SysUserDtos;

/// <summary>
/// 系统用户更新时请求结构
/// </summary>
/// <inheritdoc cref="SysUser"/>
public class SysUserUpdateDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [StringLength(30)]
    public string? UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [StringLength(60)]
    public string? Password { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [StringLength(30)]
    public string? RealName { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 头像url
    /// </summary>
    [StringLength(200)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderType? Sex { get; set; }

    /// <summary>
    /// 角色Id
    /// </summary>
    public List<Guid>? RoleIds { get; set; }
}
