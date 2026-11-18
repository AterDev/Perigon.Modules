namespace SystemMod.Models.SysUserDtos;

/// <summary>
/// 系统用户添加时请求结构
/// </summary>
/// <inheritdoc cref="SysUser"/>
public class SysUserAddDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [StringLength(30)]
    public required string UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [StringLength(60)]
    public required string Password { get; set; }

    /// <summary>
    /// 角色id
    /// </summary>
    public List<Guid>? RoleIds { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [StringLength(30)]
    public string? RealName { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [StringLength(100)]
    [EmailAddress()]
    public string? Email { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [StringLength(20)]
    [Phone()]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 头像 url
    /// </summary>
    [StringLength(200)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderType Sex { get; set; } = GenderType.Male;
}
