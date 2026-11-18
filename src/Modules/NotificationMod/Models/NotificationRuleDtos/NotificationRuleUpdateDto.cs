using Microsoft.Extensions.Validation;

namespace NotificationMod.Models.NotificationRuleDtos;

#pragma warning disable ASP0029 // This request root lives in a cross-assembly module.
[ValidatableType]
#pragma warning restore ASP0029
public class NotificationRuleUpdateDto : NotificationRuleAddDto { }

