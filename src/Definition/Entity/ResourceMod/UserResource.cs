namespace Entity.ResourceMod;

/// <summary>
/// 用户提交的资源。个人资源在转换为常规资源前不关联环境、分类、分组或标签。
/// </summary>
[Index(nameof(UserId))]
[Index(nameof(AuditStatus))]
public class UserResource : EntityBase
{
    /// <summary>
    /// 资源所有者用户 ID。
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 资源定义 ID。
    /// </summary>
    public Guid DefinitionId { get; set; }

    /// <summary>
    /// 资源可见性。私有资源仅所有者可见，公开申请由管理员审核后转换为常规资源。
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
    [MaxLength(500)]
    public string? ReviewComment { get; set; }

    /// <summary>
    /// 使用的资源定义。
    /// </summary>
    [ForeignKey(nameof(DefinitionId))]
    public ResDefinition Definition { get; set; } = null!;

    /// <summary>
    /// 用户填写的资源属性值。
    /// </summary>
    public ICollection<UserResValue> Values { get; set; } = [];
}
