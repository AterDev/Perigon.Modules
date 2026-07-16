namespace Entity.ResourceMod;

[Index(nameof(EnvironmentId))]
[Index(nameof(CategoryId))]
[Index(nameof(GroupId))]
[Index(nameof(DefinitionId))]
public class Resource : EntityBase
{
    public Guid EnvironmentId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid DefinitionId { get; set; }
    public List<string> TagNames { get; set; } = [];
    [ForeignKey(nameof(EnvironmentId))]
    public ResEnvironment Environment { get; set; } = null!;
    [ForeignKey(nameof(CategoryId))]
    public ResCategory Category { get; set; } = null!;
    [ForeignKey(nameof(GroupId))]
    public ResGroup? Group { get; set; }
    [ForeignKey(nameof(DefinitionId))]
    public ResDefinition Definition { get; set; } = null!;
    public ICollection<ResValue> Values { get; set; } = [];
}
