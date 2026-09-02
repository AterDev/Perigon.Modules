namespace ResourceMod.Models.PersonalResourceDtos;

/// <summary>
/// 个人资源查询筛选条件。
/// </summary>
public class PersonalResourceFilterDto : FilterBase
{
    /// <summary>
    /// 按个人资源状态筛选。
    /// </summary>
    public PersonalResourceStatus? Status { get; set; }

    /// <summary>
    /// 按审核状态筛选。
    /// </summary>
    public PersonalResourceAuditStatus? AuditStatus { get; set; }
}
