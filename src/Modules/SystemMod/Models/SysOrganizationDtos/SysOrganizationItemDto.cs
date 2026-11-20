namespace SystemMod.Models.SysOrganizationDtos;

/// <summary>
/// 组织树节点。
/// </summary>
public sealed class SysOrganizationItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ExternalId { get; set; }

    public Guid? ParentId { get; set; }

    public List<SysOrganizationItemDto> Children { get; set; } = [];
}
