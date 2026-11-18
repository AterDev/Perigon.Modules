using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using Microsoft.Extensions.Caching.Hybrid;

namespace ServiceDefaults;

/// <summary>
/// Stores string arrays in a compact length-prefixed UTF-8 format for HybridCache.
/// This avoids the reflection-based JSON fallback when a distributed cache is enabled.
/// </summary>
public sealed class BinaryStringArrayCacheSerializer : IHybridCacheSerializer<string[]>
{
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public string[] Deserialize(ReadOnlySequence<byte> source)
    {
        byte[] bytes = source.ToArray();
        int offset = 0;
        int count = ReadInt32(bytes, ref offset);
        if (count == -1)
        {
            return null!;
        }

        if (count < 0 || count > (bytes.Length - offset) / sizeof(int))
        {
            throw new InvalidDataException("The cached string array contains an invalid item count.");
        }

        string[] values = new string[count];
        for (int index = 0; index < count; index++)
        {
            int length = ReadInt32(bytes, ref offset);
            if (length == -1)
            {
                values[index] = null!;
                continue;
            }

            if (length < 0 || length > bytes.Length - offset)
            {
                throw new InvalidDataException("The cached string array contains an invalid item length.");
            }

            values[index] = Utf8.GetString(bytes, offset, length);
            offset += length;
        }

        if (offset != bytes.Length)
        {
            throw new InvalidDataException("The cached string array contains trailing data.");
        }

        return values;
    }

    public void Serialize(string[] value, IBufferWriter<byte> target)
    {
        if (value is null)
        {
            WriteInt32(target, -1);
            return;
        }

        WriteInt32(target, value.Length);
        foreach (string? item in value)
        {
            if (item is null)
            {
                WriteInt32(target, -1);
                continue;
            }

            byte[] bytes = Utf8.GetBytes(item);
            WriteInt32(target, bytes.Length);
            target.Write(bytes);
        }
    }

    private static int ReadInt32(byte[] bytes, ref int offset)
    {
        if (bytes.Length - offset < sizeof(int))
        {
            throw new InvalidDataException("The cached string array is truncated.");
        }

        int value = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset, sizeof(int)));
        offset += sizeof(int);
        return value;
    }

    private static void WriteInt32(IBufferWriter<byte> target, int value)
    {
        Span<byte> destination = target.GetSpan(sizeof(int));
        BinaryPrimitives.WriteInt32LittleEndian(destination, value);
        target.Advance(sizeof(int));
    }
}

