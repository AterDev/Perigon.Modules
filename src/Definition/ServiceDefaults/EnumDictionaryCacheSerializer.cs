using System.Buffers;
using System.IO;
using Microsoft.Extensions.Caching.Hybrid;
using Perigon.AspNetCore.Utils;

namespace ServiceDefaults;

/// <summary>
/// AOT-safe binary serializer for the enum catalog cache.
/// </summary>
/// <remarks>
/// This deliberately does not use JSON. The format is private to the cache and is
/// versioned so an incompatible payload is rejected instead of being misread.
/// </remarks>
public sealed class EnumDictionaryCacheSerializer : IHybridCacheSerializer<Dictionary<string, List<EnumDictionary>>>
{
    private const int Magic = 0x50454731; // PEG1
    private const int MaxEntries = 100_000;
    private const int MaxStringBytes = 1_048_576;

    public Dictionary<string, List<EnumDictionary>> Deserialize(ReadOnlySequence<byte> source)
    {
        using var stream = new MemoryStream(source.ToArray(), writable: false);
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: false);
        if (reader.ReadInt32() != Magic)
            throw new InvalidDataException("Unknown enum catalog cache format.");

        int groupCount = ReadCount(reader);
        var result = new Dictionary<string, List<EnumDictionary>>(groupCount, StringComparer.Ordinal);
        for (var i = 0; i < groupCount; i++)
        {
            string key = ReadString(reader);
            int valueCount = ReadCount(reader);
            var values = new List<EnumDictionary>(valueCount);
            for (var j = 0; j < valueCount; j++)
            {
                values.Add(new EnumDictionary
                {
                    Name = ReadString(reader),
                    Description = ReadString(reader),
                    Value = reader.ReadInt32(),
                });
            }

            if (!result.TryAdd(key, values))
                throw new InvalidDataException("Duplicate enum catalog key.");
        }

        if (stream.Position != stream.Length)
            throw new InvalidDataException("Unexpected data after enum catalog cache payload.");
        return result;
    }

    public void Serialize(Dictionary<string, List<EnumDictionary>> value, IBufferWriter<byte> target)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Magic);
            WriteCount(writer, value.Count);
            foreach ((string key, List<EnumDictionary> entries) in value)
            {
                WriteString(writer, key);
                WriteCount(writer, entries.Count);
                foreach (EnumDictionary entry in entries)
                {
                    WriteString(writer, entry.Name);
                    WriteString(writer, entry.Description);
                    writer.Write(entry.Value);
                }
            }
        }
        target.Write(stream.GetBuffer().AsSpan(0, checked((int)stream.Length)));
    }

    private static int ReadCount(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > MaxEntries)
            throw new InvalidDataException("Invalid enum catalog entry count.");
        return count;
    }

    private static void WriteCount(BinaryWriter writer, int count)
    {
        if (count < 0 || count > MaxEntries)
            throw new InvalidDataException("Invalid enum catalog entry count.");
        writer.Write(count);
    }

    private static string ReadString(BinaryReader reader)
    {
        int byteCount = reader.ReadInt32();
        if (byteCount < 0 || byteCount > MaxStringBytes)
            throw new InvalidDataException("Invalid enum catalog string length.");
        byte[] bytes = reader.ReadBytes(byteCount);
        if (bytes.Length != byteCount)
            throw new EndOfStreamException();
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
        if (bytes.Length > MaxStringBytes)
            throw new InvalidDataException("Enum catalog string is too long.");
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }
}


