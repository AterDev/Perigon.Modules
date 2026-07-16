namespace ResourceMod.Models.ResGroupDtos;

public class ResGroupUpdateDto
{
    [MaxLength(60)] public required string Name { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(60)] public string? Icon { get; set; }
    [MaxLength(20)] public required string Color { get; set; }
    public Guid CategoryId { get; set; }
}
