using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Perigon.AspNetCore.Utils;

namespace UnitTest;

public sealed partial class JsonHelperTests
{
    [Test]
    public async Task AddOrUpdateJsonNode_WithTypeInfo_SerializesNestedValue()
    {
        JsonObject root = new();
        var value = new JsonHelperValue { Name = "中文", Count = 2 };

        JsonHelper.AddOrUpdateJsonNode(
            root,
            "metadata.value",
            value,
            JsonHelperJsonContext.Default.JsonHelperValue);

        JsonObject metadata = (JsonObject)root["metadata"]!;
        JsonObject serialized = (JsonObject)metadata["value"]!;
        await Assert.That(serialized["Name"]!.GetValue<string>()).IsEqualTo("中文");
        await Assert.That(serialized["Count"]!.GetValue<int>()).IsEqualTo(2);
    }

    [JsonSerializable(typeof(JsonHelperValue))]
    private partial class JsonHelperJsonContext : JsonSerializerContext
    {
    }

    private sealed partial class JsonHelperValue
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
