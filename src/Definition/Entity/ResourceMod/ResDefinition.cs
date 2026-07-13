namespace Entity.ResourceMod;

public class ResDefinition : EntityBase
{
    [MaxLength(60)]
    public required string Name { get; set; }
    [MaxLength(60)]
    public string? Icon { get; set; }
    public ICollection<ResDefinitionProperty> Properties { get; set; } = [];
    public ICollection<Resource> Resources { get; set; } = [];
}
