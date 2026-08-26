namespace ResourceMod.Models.ResPermissionDtos;

/// <summary>
/// 资源权限替换请求结构。
/// </summary>
/// <inheritdoc cref="ResPermission"/>
public class ResPermissionUpdateDto
{
    /// <summary>
    /// 资源环境 ID。
    /// </summary>
    public Guid EnvironmentId { get; set; }

    /// <summary>
    /// 资源分类 ID。
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// 允许查看该环境和分类资源的角色 ID 列表；提交时会去重并整体替换原授权。
    /// </summary>
    public List<Guid> RoleIds { get; set; } = [];
}
