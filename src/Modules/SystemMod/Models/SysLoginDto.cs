namespace SystemMod.Models;

/// <summary>
/// 登录
/// </summary>
public class SysLoginDto
{
    [StringLength(50)]
    public string Email { get; set; } = default!;
    [StringLength(60, MinimumLength = 6)]
    public string Password { get; set; } = default!;
    /// <summary>
    /// 验证码
    /// </summary>
    [StringLength(50)]
    public string? VerifyCode { get; set; }
}
