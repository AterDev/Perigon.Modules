namespace Entity.ResourceMod;

public class ResGroup : EntityBase
{
    [MaxLength(60)]
    public required string Name { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    [MaxLength(60)]
    public string? Icon { get; set; }
    [MaxLength(20)]
    public required string Color { get; set; }
    public Guid CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public ResCategory Category { get; set; } = null!;
    public ICollection<Resource> Resources { get; set; } = [];
}
