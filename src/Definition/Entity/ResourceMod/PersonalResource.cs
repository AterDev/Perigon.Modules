namespace Entity.ResourceMod;

/// <summary>
/// 用户提交的个人资源。个人资源在审核通过前不会关联环境、分类、分组或标签。
/// </summary>
[Index(nameof(UserId))]
[Index(nameof(AuditStatus))]
public class PersonalResource : EntityBase
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
    /// 个人资源状态。
    /// </summary>
    public PersonalResourceStatus Status { get; set; }

    /// <summary>
    /// 公开申请审核状态。
    /// </summary>
    public PersonalResourceAuditStatus AuditStatus { get; set; }

    /// <summary>
    /// 按资源定义规范化后的属性值 JSON。
    /// </summary>
    [MaxLength(100000)]
    public required string ValuesJson { get; set; }

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
}
