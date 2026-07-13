namespace Entity.ResourceMod;

public class ResTag : EntityBase
{
    [MaxLength(60)]
    public required string Name { get; set; }
    [MaxLength(20)]
    public required string Color { get; set; }
    [MaxLength(60)]
    public string? Icon { get; set; }
}
