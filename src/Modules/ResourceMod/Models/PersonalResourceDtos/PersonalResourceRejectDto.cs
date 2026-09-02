namespace ResourceMod.Models.PersonalResourceDtos;

/// <summary>
/// 公开申请驳回请求结构。
/// </summary>
public class PersonalResourceRejectDto
{
    /// <summary>
    /// 驳回原因。
    /// </summary>
    [MaxLength(500)]
    public string? ReviewComment { get; set; }
}
