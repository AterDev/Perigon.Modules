using System.Text.Json.Serialization;
using Perigon.AspNetCore.Options;

namespace SystemMod.Serialization;

[JsonSerializable(typeof(LoginSecurityPolicyOption))]
internal partial class SystemModJsonSerializerContext : JsonSerializerContext
{
}
