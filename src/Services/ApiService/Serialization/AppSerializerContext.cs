using System.Text.Json.Serialization;
using Perigon.AspNetCore.Options;

namespace ApiService.Serialization;

[JsonSerializable(typeof(LoginSecurityPolicyOption))]
public partial class AppSerializerContext : JsonSerializerContext { }
