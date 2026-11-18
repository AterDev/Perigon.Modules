namespace SystemMod.Models.SysDataScopeGroupDtos;

/// <see cref="Entity.SystemMod.SysDataScopeGroup"/>
public class SysDataScopeGroupUpdateDto
{
    [StringLength(30)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool? IsEnabled { get; set; }
}
