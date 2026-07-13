namespace Entity.ResourceMod;

public class ResEnvironment : EntityBase
{
    [MaxLength(60)]
    public required string Name { get; set; }
    [MaxLength(60)]
    public string? Icon { get; set; }
    [MaxLength(20)]
    public required string Color { get; set; }
    public ICollection<Resource> Resources { get; set; } = [];
    public ICollection<ResPermission> Permissions { get; set; } = [];
}
