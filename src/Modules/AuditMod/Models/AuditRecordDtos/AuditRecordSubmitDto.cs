using Microsoft.Extensions.Validation;

namespace AuditMod.Models.AuditRecordDtos;

#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public sealed class AuditRecordSubmitDto
{
    [Required, StringLength(100)]
    public string SceneCode { get; set; } = string.Empty;
    [Required, StringLength(200)]
    public string BizId { get; set; } = string.Empty;
    [Required, StringLength(2000)]
    public string ApplyReason { get; set; } = string.Empty;
    public string? FormSnapshotJson { get; set; }
}
