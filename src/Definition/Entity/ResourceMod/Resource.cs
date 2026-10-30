namespace Entity.ResourceMod;

/// <summary>按环境、分类和定义组织的资源实例。</summary>
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
    // PostgreSQL text[] is materialized as string[] by Npgsql; keeping the
    // entity property array-shaped avoids an unsupported List<string> cast.
    public string[] TagNames { get; set; } = [];
    /// <summary>资源所属环境。</summary>
    [ForeignKey(nameof(EnvironmentId))]
    [NotMapped]
    public ResEnvironment Environment { get; set; } = null!;
    /// <summary>资源所属分类。</summary>
    [ForeignKey(nameof(CategoryId))]
    [NotMapped]
    public ResCategory Category { get; set; } = null!;
    /// <summary>资源所属分组。</summary>
    [ForeignKey(nameof(GroupId))]
    [NotMapped]
    public ResGroup? Group { get; set; }
    /// <summary>资源使用的定义。</summary>
    [ForeignKey(nameof(DefinitionId))]
    [NotMapped]
    public ResDefinition Definition { get; set; } = null!;
    /// <summary>资源属性值。</summary>
    [NotMapped]
    public ICollection<ResValue> Values { get; set; } = [];
}
