namespace Entity.NotificationMod;

[Index(nameof(RuleId), nameof(ReceiverId), IsUnique = true)]
public class NotificationRuleReceiver : EntityBase
{
    public Guid RuleId { get; set; }
    public Guid ReceiverId { get; set; }
}
