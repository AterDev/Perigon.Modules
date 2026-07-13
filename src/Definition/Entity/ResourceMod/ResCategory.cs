namespace Entity.ResourceMod;

[Index(nameof(TenantId), nameof(CatalogCode), IsUnique = true)]
public class ResCategory : EntityBase
{
    [MaxLength(60)]
    public required string Name { get; set; }
    [MaxLength(60)]
    public required string CatalogCode { get; set; }
    [MaxLength(60)]
    public string? Icon { get; set; }
    [MaxLength(20)]
    public required string Color { get; set; }
    public ICollection<ResGroup> Groups { get; set; } = [];
    public ICollection<Resource> Resources { get; set; } = [];
    public ICollection<ResPermission> Permissions { get; set; } = [];
}
