namespace Entity.ResourceMod;

/// <summary>
/// 用户资源的属性值。值和属性快照独立保存，避免资源定义变更影响历史提交。
/// </summary>
public class UserResValue : EntityBase
{
    /// <summary>
    /// 用户资源 ID。
    /// </summary>
    public Guid UserResourceId { get; set; }

    /// <summary>
    /// 资源定义属性 ID。
    /// </summary>
    public Guid DefinitionPropertyId { get; set; }

    /// <summary>
    /// 规范化后的属性值。
    /// </summary>
    [StringLength(1000)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 保存时的属性名称快照。
    /// </summary>
    [StringLength(60)]
    public string PropertyNameSnapshot { get; set; } = string.Empty;

    /// <summary>保存时的属性值类型快照。</summary>
    public ResValueType ValueTypeSnapshot { get; set; }

    /// <summary>
    /// 所属用户资源。
    /// </summary>
    [ForeignKey(nameof(UserResourceId))]
    public UserResource UserResource { get; set; } = null!;

    /// <summary>
    /// 所属资源定义属性。
    /// </summary>
    [ForeignKey(nameof(DefinitionPropertyId))]
    public ResDefinitionProperty DefinitionProperty { get; set; } = null!;
}
