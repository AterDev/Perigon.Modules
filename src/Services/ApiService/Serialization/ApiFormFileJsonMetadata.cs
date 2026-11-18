using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace ApiService;

// ASP.NET Core OpenAPI recognizes IFormFile as a binary multipart part, but
// still requests JsonTypeInfo before it applies that schema fix-up. Supplying
// metadata here keeps the OpenAPI path AOT-safe; the generated OpenAPI schema
// is still binary rather than a JSON object.
[JsonSerializable(typeof(IFormFile))]
partial class ApiJsonSerializerContext
{
}
