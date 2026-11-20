using System.Text.Json.Serialization;

namespace Entity.SystemMod;

/// <summary>
/// 系统用户
/// </summary>
public class SysUser : EntityBase
{
    /// <summary>
    /// 用户名
    /// </summary>
    [StringLength(30, MinimumLength = 4)]
    public string? UserName { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [StringLength(30, MinimumLength = 2)]
    public string? RealName { get; set; }

    [StringLength(100, MinimumLength = 6)]
    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }

    [JsonIgnore]
    [StringLength(100)]
    public string PasswordHash { get; set; } = default!;

    [JsonIgnore]
    [StringLength(100)]
    public string PasswordSalt { get; set; } = default!;

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTimeOffset? LastLoginTime { get; set; }

    /// <summary>
    /// 最后密码修改时间
    /// </summary>
    public DateTimeOffset LastPwdEditTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 密码重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 头像url
    /// </summary>
    [StringLength(200)]
    public string? Avatar { get; set; }
    public ICollection<SysUserRole> SysUserRoles { get; set; } = [];
    public ICollection<SysLogs> SysLogs { get; set; } = [];
    public ICollection<SysUserOrganization> SysUserOrganizations { get; set; } = [];

    /// <summary>
    /// 数据权限关联表
    /// </summary>
    public ICollection<SysUserDataScopeGroup> DataScopeGroups { get; set; } = [];

    /// <summary>
    /// 性别
    /// </summary>
    public Sex Sex { get; set; } = Sex.Male;

    public string GetUniqueKey(string prefix, string client)
    {
        return prefix + client + Id.ToString();
    }
}

/// <summary>
/// 性别
/// </summary>
public enum Sex
{
    /// <summary>
    /// 男性
    /// </summary>
    [Description("男性")]
    Male,

    /// <summary>
    /// 女性
    /// </summary>
    [Description("女性")]
    Female,

    /// <summary>
    /// 其他
    /// </summary>
    [Description("其他")]
    Else,
}
