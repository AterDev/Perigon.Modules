using Microsoft.Extensions.Validation;

namespace AuditMod.Models.AuditRecordDtos;
#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public sealed class AuditRecordWithdrawDto { [StringLength(2000)] public string? Reason { get; set; } }
