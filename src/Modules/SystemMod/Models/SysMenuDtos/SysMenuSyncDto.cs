namespace SystemMod.Models.SysMenuDtos;
/// <summary>
/// <inheritdoc cref="SysMenu"/>
/// </summary>
public class SysMenuSyncDto
{
    public required string Name { get; set; }
    public required string AccessCode { get; set; }
    public int MenuType { get; set; }
    public int? Sort { get; set; }
    public string? Icon { get; set; }
    public List<SysMenuSyncDto> Children { get; set; } = [];
}
