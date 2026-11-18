using System.Buffers;
using ServiceDefaults;

namespace UnitTest;

public sealed class BinaryStringArrayCacheSerializerTests
{
    [Test]
    public async Task RoundTrip_PreservesNullEmptyUnicodeAndNullItems()
    {
        var serializer = new BinaryStringArrayCacheSerializer();
        string?[][] cases = [null!, Array.Empty<string?>(), new string?[] { "", "中文", "😀", null }];

        foreach (string?[] expected in cases)
        {
            var buffer = new ArrayBufferWriter<byte>();
            serializer.Serialize(expected!, buffer);
            string[]? actual = serializer.Deserialize(new ReadOnlySequence<byte>(buffer.WrittenMemory));

            if (expected is null)
            {
                await Assert.That(actual).IsNull();
            }
            else
            {
                await Assert.That(actual).IsEquivalentTo(expected);
            }
        }
    }

    [Test]
    public async Task Deserialize_RejectsTruncatedPayload()
    {
        var serializer = new BinaryStringArrayCacheSerializer();
        await Assert.That(() => serializer.Deserialize(new ReadOnlySequence<byte>([1, 0, 0])))
            .Throws<InvalidDataException>();
    }
}
