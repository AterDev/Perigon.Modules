namespace SystemMod.Models.SysLogsDtos;

/// <summary>
/// 系统日志更新时请求结构
/// </summary>
/// <see cref="SysLogs"/>
public class SysLogsUpdateDto
{
    /// <summary>
    /// 操作人名称
    /// </summary>
    [MaxLength(100)]
    public string? ActionUserName { get; set; }

    /// <summary>
    /// 操作对象名称
    /// </summary>
    [MaxLength(100)]
    public string? TargetName { get; set; }

    /// <summary>
    /// 操作路由
    /// </summary>
    [MaxLength(200)]
    public string? Route { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public UserActionType? ActionType { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }
}
