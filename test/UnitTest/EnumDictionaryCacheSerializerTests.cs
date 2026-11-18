using System.Buffers;
using Perigon.AspNetCore.Utils;
using ServiceDefaults;

namespace UnitTest;

public sealed class EnumDictionaryCacheSerializerTests
{
    [Test]
    public async Task BinarySerializer_RoundTripsAllFields()
    {
        var source = new Dictionary<string, List<EnumDictionary>>(StringComparer.Ordinal)
        {
            ["AuditRecordStatus"] =
            [
                new EnumDictionary { Name = "Approved", Description = "已通过", Value = 2 },
                new EnumDictionary { Name = "Rejected", Description = "驳回", Value = -1 },
            ],
            ["Empty"] = [],
        };
        var serializer = new EnumDictionaryCacheSerializer();
        var buffer = new ArrayBufferWriter<byte>();

        serializer.Serialize(source, buffer);
        Dictionary<string, List<EnumDictionary>> actual = serializer.Deserialize(
            new ReadOnlySequence<byte>(buffer.WrittenMemory));

        await Assert.That(actual.Keys).IsEquivalentTo(source.Keys);
        await Assert.That(actual["AuditRecordStatus"]).Count().IsEqualTo(2);
        await Assert.That(actual["AuditRecordStatus"][0].Name).IsEqualTo("Approved");
        await Assert.That(actual["AuditRecordStatus"][0].Description).IsEqualTo("已通过");
        await Assert.That(actual["AuditRecordStatus"][0].Value).IsEqualTo(2);
        await Assert.That(actual["AuditRecordStatus"][1].Value).IsEqualTo(-1);
        await Assert.That(actual["Empty"]).IsEmpty();
    }
}
