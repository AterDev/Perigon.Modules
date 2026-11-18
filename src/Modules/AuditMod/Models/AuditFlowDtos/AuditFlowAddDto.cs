using Microsoft.Extensions.Validation;

namespace AuditMod.Models.AuditFlowDtos;
#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public class AuditFlowAddDto
{
    [Required, StringLength(100)]
    public string FlowKey { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string SceneCode { get; set; } = string.Empty;
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public List<AuditFlowStepDto> Steps { get; set; } = [];
}
