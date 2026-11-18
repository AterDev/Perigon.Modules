using System.Text.Json.Serialization;
using Perigon.AspNetCore.Models;

namespace ServiceDefaults.Middleware;

[JsonSerializable(typeof(ErrorResult))]
internal partial class MiddlewareJsonContext : JsonSerializerContext;
