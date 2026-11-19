namespace Entity.ResourceMod;

/// <summary>资源定义中的单个属性配置。</summary>
public class ResDefinitionProperty : EntityBase
{
    /// <summary>属性名称。</summary>
    [StringLength(60)]
    public string Name { get; set; } = string.Empty;
    /// <summary>属性值类型。</summary>
    [Column("ValueType")]
    public ResValueType ValueType { get; set; }
    /// <summary>是否必填。</summary>
    public bool IsRequired { get; set; }
    /// <summary>属性值最大长度。</summary>
    public int MaxLength { get; set; } = 200;
    /// <summary>显示排序。排序属于定义与属性的关联，属性本身不持久化该值。</summary>
    [NotMapped]
    public int Sort { get; set; }
    [JsonIgnore]
    [NotMapped]
    public ICollection<ResDefinitionPropertyMap> DefinitionMaps { get; set; } = [];
    /// <summary>使用此属性的资源值。</summary>
    [JsonIgnore]
    [NotMapped]
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
