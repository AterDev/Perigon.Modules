namespace Entity.ResourceMod;

/// <summary>资源环境和分类的角色读取授权。</summary>
[Index(nameof(TenantId), nameof(RoleId), nameof(EnvironmentId), nameof(CategoryId), IsUnique = true)]
public class ResPermission : EntityBase
{
    /// <summary>角色 ID。</summary>
    public Guid RoleId { get; set; }
    /// <summary>环境 ID。</summary>
    public Guid EnvironmentId { get; set; }
    /// <summary>分类 ID。</summary>
    public Guid CategoryId { get; set; }
    /// <summary>授权对应的环境。</summary>
    [ForeignKey(nameof(EnvironmentId))]
    public ResEnvironment Environment { get; set; } = null!;
    /// <summary>授权对应的分类。</summary>
    [ForeignKey(nameof(CategoryId))]
    public ResCategory Category { get; set; } = null!;
}
