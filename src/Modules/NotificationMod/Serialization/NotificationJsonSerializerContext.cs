using NotificationMod.Models.WebhookDtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotificationMod.Serialization;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(WebhookResultDto))]
public partial class NotificationJsonSerializerContext : JsonSerializerContext
{
}
