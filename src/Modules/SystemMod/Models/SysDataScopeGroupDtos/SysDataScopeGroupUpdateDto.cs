namespace SystemMod.Models.SysDataScopeGroupDtos;

/// <see cref="Entity.SystemMod.SysDataScopeGroup"/>
public class SysDataScopeGroupUpdateDto
{
    [MaxLength(30)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool? IsEnabled { get; set; }
}
