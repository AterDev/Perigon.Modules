// ⚠️ dont delete this file unless you are absolutely sure about it.
using Perigon.AspNetCore.Abstraction;
namespace Entity;

/// <summary>
/// The tenant entity
/// </summary>
public sealed class Tenant : IEntityBase
{
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public Tenant() { Name = string.Empty; Domain = string.Empty; }
    [Key] public Guid Id { get; set; } = Guid.CreateVersion7();
    [AutoTimestamp] public DateTimeOffset CreatedTime { get; set; }
    [AutoTimestamp(onUpdate: true)] public DateTimeOffset UpdatedTime { get; set; }
    public bool IsDeleted { get; set; }
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? DbConnectionString { get; set; }

    [MaxLength(500)]
    public string? AnalysisConnectionString { get; set; }

    [MaxLength(200)]
    public required string Domain { get; set; }

    [JsonIgnore]
    public int TypeValue { get; set; } = (int)TenantType.Normal;

    [NotMapped]
    public TenantType Type { get => (TenantType)TypeValue; set => TypeValue = (int)value; }

    public bool Disabled { get; set; }
}

public enum TenantType
{
    /// <summary>
    /// Trial
    /// </summary>
    [Description("Trial")]
    Trial,

    /// <summary>
    /// Normal
    /// </summary>
    [Description("Normal")]
    Normal,

    /// <summary>
    /// Independent
    /// </summary>
    [Description("Independent")]
    Independent,
}
