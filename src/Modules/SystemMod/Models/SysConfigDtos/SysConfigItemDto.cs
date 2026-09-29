namespace SystemMod.Models.SysConfigDtos;

/// <summary>
/// 系统配置列表元素
/// </summary>
/// <see cref="SysConfig"/>
public class SysConfigItemDto
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Key { get; set; } = default!;

    [MaxLength(500)]
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

    /// <summary>
    /// 以json字符串形式存储
    /// </summary>
    [MaxLength(2000)]
    public string? Value { get; set; }
}
