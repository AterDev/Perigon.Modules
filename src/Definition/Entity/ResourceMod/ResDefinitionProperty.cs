namespace Entity.ResourceMod;

/// <summary>资源定义中的单个属性配置。</summary>
[Index(nameof(DefinitionId), nameof(Name), IsUnique = true)]
public class ResDefinitionProperty : EntityBase
{
    /// <summary>属性名称。</summary>
    [MaxLength(60)]
    public required string Name { get; set; }
    /// <summary>属性值类型。</summary>
    public ResValueType ValueType { get; set; }
    /// <summary>是否必填。</summary>
    public bool IsRequired { get; set; }
    /// <summary>属性值最大长度。</summary>
    public int MaxLength { get; set; } = 200;
    /// <summary>显示排序。</summary>
    public int Sort { get; set; }
    /// <summary>所属资源定义 ID。</summary>
    public Guid DefinitionId { get; set; }
    /// <summary>所属资源定义。</summary>
    [ForeignKey(nameof(DefinitionId))]
    public ResDefinition Definition { get; set; } = null!;
    /// <summary>使用此属性的资源值。</summary>
    public ICollection<ResValue> Values { get; set; } = [];
}

/// <summary>资源属性值类型。</summary>
public enum ResValueType
{
    [Description("字符串")]
    String,
    [Description("数字")]
    Number,
    [Description("布尔值")]
    Boolean,
    [Description("日期")]
    Date,
    [Description("URI")]
    Uri,
    [Description("IP 地址")]
    IPAddress,
}
