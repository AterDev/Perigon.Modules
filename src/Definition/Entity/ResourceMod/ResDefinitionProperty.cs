namespace Entity.ResourceMod;

[Index(nameof(DefinitionId), nameof(Name), IsUnique = true)]
public class ResDefinitionProperty : EntityBase
{
    [MaxLength(60)]
    public required string Name { get; set; }
    public ResValueType ValueType { get; set; }
    public bool IsRequired { get; set; }
    public int MaxLength { get; set; } = 200;
    public int Sort { get; set; }
    public Guid DefinitionId { get; set; }
    [ForeignKey(nameof(DefinitionId))]
    public ResDefinition Definition { get; set; } = null!;
    public ICollection<ResValue> Values { get; set; } = [];
}

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
