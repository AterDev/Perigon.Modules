using System.Text.Json.Serialization;

namespace SystemMod.Models.SysOrganizationDtos;

/// <summary>
/// 修改组织。
/// </summary>
public sealed class SysOrganizationUpdateDto
{
    /// <summary>
    /// 组织名称。
    /// </summary>
    [StringLength(100)]
    public string? Name { get; set; }

    /// <summary>
    /// 外部系统中的组织标识。传 null 可清空。
    /// </summary>
    [StringLength(40)]
    public string? ExternalId
    {
        get;
        set
        {
            field = value;
            ExternalIdSpecified = true;
        }
    }

    /// <summary>
    /// 新的父组织。传 null 可移动到根组织。
    /// </summary>
    public Guid? ParentId
    {
        get;
        set
        {
            field = value;
            ParentIdSpecified = true;
        }
    }

    [JsonIgnore]
    public bool ExternalIdSpecified { get; private set; }

    [JsonIgnore]
    public bool ParentIdSpecified { get; private set; }
}
