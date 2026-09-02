namespace ResourceMod.Models.UserResourceDtos;

/// <summary>
/// 公开申请驳回结构。
/// </summary>
public class UserResourceRejectDto
{
    /// <summary>
    /// 驳回原因。
    /// </summary>
    [MaxLength(500)]
    public string? ReviewComment { get; set; }
}
