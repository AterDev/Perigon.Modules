namespace SystemMod.Models.SysRoleDtos;

/// <summary>
/// 角色概要
/// </summary>
/// <inheritdoc cref="SysRole"/>
public class SysRoleDetailDto
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [MaxLength(30)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 角色标识
    /// </summary>
    public string NameValue { get; set; } = default!;

    /// <summary>
    /// 是否系统内置
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [MaxLength(30)]
    public string? Icon { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
}
