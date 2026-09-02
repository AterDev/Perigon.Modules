namespace ResourceMod.Models.UserResourceDtos;

/// <summary>
/// 用户资源列表项结构。
/// </summary>
public class UserResourceItemDto
{
    /// <summary>
    /// 用户资源唯一标识。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 资源所有者用户 ID。
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 资源定义 ID。
    /// </summary>
    public Guid DefinitionId { get; set; }

    /// <summary>
    /// 资源定义名称。
    /// </summary>
    public required string DefinitionName { get; set; }

    /// <summary>
    /// 用户资源可见性。
    /// </summary>
    public UserResourceStatus Status { get; set; }

    /// <summary>
    /// 公开申请审核状态。
    /// </summary>
    public UserResourceAuditStatus AuditStatus { get; set; }

    /// <summary>
    /// 审核通过后创建的常规资源 ID。
    /// </summary>
    public Guid? ApprovedResourceId { get; set; }

    /// <summary>
    /// 审核意见或驳回原因。
    /// </summary>
    public string? ReviewComment { get; set; }

    /// <summary>
    /// 最后更新时间。
    /// </summary>
    public DateTimeOffset UpdatedTime { get; set; }
}
