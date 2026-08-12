namespace Entity.ResourceMod;

/// <summary>按环境、分类和定义组织的资源实例。</summary>
[Index(nameof(EnvironmentId))]
[Index(nameof(CategoryId))]
[Index(nameof(GroupId))]
[Index(nameof(DefinitionId))]
public class Resource : EntityBase
{
    /// <summary>环境 ID。</summary>
    public Guid EnvironmentId { get; set; }
    /// <summary>分类 ID。</summary>
    public Guid CategoryId { get; set; }
    /// <summary>可选的分组 ID。</summary>
    public Guid? GroupId { get; set; }
    /// <summary>资源定义 ID。</summary>
    public Guid DefinitionId { get; set; }
    /// <summary>资源关联的标签名称列表。</summary>
    public List<string> TagNames { get; set; } = [];
    /// <summary>资源所属环境。</summary>
    [ForeignKey(nameof(EnvironmentId))]
    public ResEnvironment Environment { get; set; } = null!;
    /// <summary>资源所属分类。</summary>
    [ForeignKey(nameof(CategoryId))]
    public ResCategory Category { get; set; } = null!;
    /// <summary>资源所属分组。</summary>
    [ForeignKey(nameof(GroupId))]
    public ResGroup? Group { get; set; }
    /// <summary>资源使用的定义。</summary>
    [ForeignKey(nameof(DefinitionId))]
    public ResDefinition Definition { get; set; } = null!;
    /// <summary>资源属性值。</summary>
    public ICollection<ResValue> Values { get; set; } = [];
}
