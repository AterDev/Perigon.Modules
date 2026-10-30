using System.Text.Json.Serialization;

namespace Entity.SystemMod;

/// <summary>
/// 系统日志
/// </summary>
public class SysLogs : EntityBase
{
    /// <summary>
    /// 操作人名称
    /// </summary>
    [MaxLength(100)]
    [Required]
    public string ActionUserName { get; set; } = string.Empty;

    /// <summary>
    /// 操作对象名称
    /// </summary>
    [MaxLength(100)]
    public string? TargetName { get; set; }

    [NotMapped]
    [JsonIgnore]
    public object? Data { get; set; }

    /// <summary>
    /// 操作路由
    /// </summary>
    [MaxLength(200)]
    [Required]
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// 操作类型
    /// </summary>
    [NotMapped]
    public UserActionType ActionType
    {
        get => (UserActionType)ActionTypeValue;
        set => ActionTypeValue = (int)value;
    }

    [Column("ActionType")]
    [JsonIgnore]
    public int ActionTypeValue { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    [ForeignKey(nameof(SysUserId))]
    [NotMapped]
    public SysUser SysUser { get; set; } = null!;

    public Guid SysUserId { get; set; } = default!;

    public static SysLogs NewLog(
        Guid tenantId,
        string userName,
        Guid userId,
        object entity,
        UserActionType actionType,
        string? route = null,
        string? description = null
    )
    {
        return new SysLogs
        {
            TenantId = tenantId,
            SysUserId = userId,
            ActionUserName = userName,
            Data = entity,
            Route = route ?? string.Empty,
            ActionType = actionType,
            Description = description,
        };
    }
}
