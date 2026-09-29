namespace SystemMod.Models.SysLogsDtos;

/// <summary>
/// 系统日志列表元素
/// </summary>
/// <see cref="SysLogs"/>
public class SysLogsItemDto
{
    /// <summary>
    /// 操作人名称
    /// </summary>
    [MaxLength(100)]
    public string ActionUserName { get; set; } = default!;

    /// <summary>
    /// 操作对象名称
    /// </summary>
    [MaxLength(100)]
    public string TargetName { get; set; } = default!;

    /// <summary>
    /// 操作路由
    /// </summary>
    [MaxLength(200)]
    public string Route { get; set; } = default!;

    /// <summary>
    /// 操作类型
    /// </summary>
    public UserActionType ActionType { get; set; } = default!;

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
}
