using Microsoft.Extensions.Validation;

namespace NotificationMod.Models.NotificationRuleDtos;

#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public class NotificationRuleAddDto
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public Guid[] ReceiverIds { get; set; } = [];
    public string[] EventTypes { get; set; } = [];
    public string[] Statuses { get; set; } = [];
    public long[] ProjectIds { get; set; } = [];
    public bool Enabled { get; set; } = true;
}

