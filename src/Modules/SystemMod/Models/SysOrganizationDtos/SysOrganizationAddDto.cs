namespace SystemMod.Models.SysOrganizationDtos;

/// <summary>
/// 创建组织。
/// </summary>
public sealed class SysOrganizationAddDto
{
    /// <summary>
    /// 组织名称。
    /// </summary>
    [StringLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 外部系统中的组织标识。
    /// </summary>
    [StringLength(40)]
    public string? ExternalId { get; set; }

    /// <summary>
    /// 父组织。为空时创建根组织。
    /// </summary>
    public Guid? ParentId { get; set; }
}
