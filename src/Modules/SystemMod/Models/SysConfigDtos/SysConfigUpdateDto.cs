namespace SystemMod.Models.SysConfigDtos;

/// <summary>
/// 系统配置更新时请求结构
/// </summary>
/// <see cref="SysConfig"/>
public class SysConfigUpdateDto
{
    [MaxLength(100)]
    public string? Key { get; set; }

    /// <summary>
    /// 以json字符串形式存储
    /// </summary>
    [MaxLength(2000)]
    public string? Value { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
    public bool? Valid { get; set; }

    /// <summary>
    /// 分组
    /// </summary>
    [MaxLength(60)]
    public string? GroupName { get; set; }
}
