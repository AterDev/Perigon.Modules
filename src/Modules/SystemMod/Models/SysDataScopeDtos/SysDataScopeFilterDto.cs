namespace SystemMod.Models.SysDataScopeDtos;

/// <summary>
/// 数据权限范围查询条件。
/// </summary>
public class SysDataScopeFilterDto : FilterBase
{
    [MaxLength(60)]
    public string? Name { get; set; }

    [MaxLength(30)]
    public string? ResourceCode { get; set; }

    public DataScopeType? ScopeType { get; set; }

    public Guid? GroupId { get; set; }
}
