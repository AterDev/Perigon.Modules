namespace SystemMod.Models.SysConfigDtos;

/// <summary>
/// 系统配置添加时请求结构
/// </summary>
/// <see cref="SysConfig"/>
public class SysConfigAddDto
{
    [StringLength(100)]
    public required string Key { get; set; }

    /// <summary>
    /// 以json字符串形式存储
    /// </summary>
    [StringLength(2000)]
    public string Value { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
    public bool Valid { get; set; } = true;

    /// <summary>
    /// 是否属于系统配置
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 组
    /// </summary>
    public string? GroupName { get; set; }
}
