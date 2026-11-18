using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Perigon.AspNetCore.Utils;

namespace ApiService.OpenApi;

/// <summary>Adds the legacy x-enumData extension from static enum metadata.</summary>
public sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (EnumOpenApiMetadata.TryGet(context.JsonTypeInfo.Type, out var items))
        {
            schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            schema.Extensions["x-enumData"] = new EnumDataExtension(items);
        }

        return Task.CompletedTask;
    }

    private sealed class EnumDataExtension(IReadOnlyList<EnumDictionary> items) : IOpenApiExtension
    {
        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteStartArray();
            foreach (var item in items)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("name"); writer.WriteValue(item.Name);
                writer.WritePropertyName("value"); writer.WriteValue(item.Value);
                writer.WritePropertyName("description"); writer.WriteValue(item.Description);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}

internal static class EnumOpenApiMetadata
{
    public static bool TryGet(Type type, out IReadOnlyList<EnumDictionary> items) =>
        GeneratedEnumOpenApiMetadata.Values.TryGetValue(type, out items!);
}
