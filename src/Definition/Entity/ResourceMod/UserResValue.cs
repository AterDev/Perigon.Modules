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
    [MaxLength(1000)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 保存时的属性名称快照。
    /// </summary>
    [MaxLength(60)]
    public string PropertyNameSnapshot { get; set; } = string.Empty;

    /// <summary>
    /// 保存时的属性值类型快照的整数存储值。
    /// </summary>
    [JsonIgnore]
    [Column("ValueTypeSnapshot")]
    public int ValueTypeSnapshotValue { get; set; }
    /// <summary>保存时的属性值类型快照。</summary>
    [NotMapped]
    public ResValueType ValueTypeSnapshot
    {
        get => (ResValueType)ValueTypeSnapshotValue;
        set => ValueTypeSnapshotValue = (int)value;
    }

    /// <summary>
    /// 所属用户资源。
    /// </summary>
    [ForeignKey(nameof(UserResourceId))]
    [NotMapped]
    public UserResource UserResource { get; set; } = null!;

    /// <summary>
    /// 所属资源定义属性。
    /// </summary>
    [ForeignKey(nameof(DefinitionPropertyId))]
    [NotMapped]
    public ResDefinitionProperty DefinitionProperty { get; set; } = null!;
}
