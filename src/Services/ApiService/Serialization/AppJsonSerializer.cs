using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Share.Serialization;

namespace ApiService.Serialization;

public sealed class AppJsonSerializer : IAppJsonSerializer
{
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, GetTypeInfo<T>());
    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize(json, GetTypeInfo<T>());

    private static JsonTypeInfo<T> GetTypeInfo<T>() =>
        AppSerializerContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
        ?? throw new InvalidOperationException($"AppSerializerContext does not contain JSON metadata for {typeof(T)}.");
}
