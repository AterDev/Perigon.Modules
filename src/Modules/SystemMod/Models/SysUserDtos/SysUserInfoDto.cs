namespace SystemMod.Models.SysUserDtos;

public class SysUserInfoDto
{
    public Guid Id
    {
        get; set;
    }
    public string Username { get; set; } = default!;
    public string[] Roles { get; set; } = default!;
    public List<SysMenu>? Menus
    {
        get; set;
    }
    public List<SysUserDataScopeGroupItemDto> DataScopeGroups { get; set; } = [];
}
