using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ApiService.OpenApi;

/// <summary>Describes enum members using metadata produced during the Web project build.</summary>
public sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string ResourceName = "Perigon.EnumMetadata.enum-meta.generated.xml";
    private static readonly IReadOnlyDictionary<(string Assembly, string Type), EnumInfo> Metadata = LoadMetadata();

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        // Flags may have combinations; string enums may use serializer-specific names.
        if (schema.Type != JsonSchemaType.Integer)
        {
            return Task.CompletedTask;
        }
        var type = context.JsonTypeInfo.Type;
        var assembly = type.Assembly.GetName().Name;
        if (!type.IsEnum || assembly is null || type.FullName is null ||
            !Metadata.TryGetValue((assembly, type.FullName), out var info) || info.Flags || info.Members.Count == 0)
        {
            return Task.CompletedTask;
        }

        var branches = info.Members.Select(member => (IOpenApiSchema)new OpenApiSchema
        {
            Title = member.Name,
            Description = member.Description,
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["const"] = new NumericConstExtension(member.Value),
            },
        }).ToList();
        if (info.Members.Select(member => member.Value).Distinct(StringComparer.Ordinal).Count() == info.Members.Count)
        {
            schema.OneOf = branches;
        }
        else
        {
            // Aliases may share a value and must not invalidate oneOf.
            schema.AnyOf = branches;
        }
        return Task.CompletedTask;
    }

    private static IReadOnlyDictionary<(string Assembly, string Type), EnumInfo> LoadMetadata()
    {
        using var stream = typeof(EnumSchemaTransformer).Assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException("Embedded enum metadata was not generated during the Web project build.");
        }

        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        });
        var document = XDocument.Load(reader);
        var result = new Dictionary<(string Assembly, string Type), EnumInfo>();
        foreach (var item in document.Root?.Elements("enum") ?? [])
        {
            var assembly = (string?)item.Attribute("assembly") ?? throw new InvalidDataException("Enum assembly is missing.");
            var type = (string?)item.Attribute("type") ?? throw new InvalidDataException("Enum type is missing.");
            var flags = (bool?)item.Attribute("flags") ?? false;
            var members = item.Elements("member").Select(member =>
            {
                var name = (string?)member.Attribute("name") ?? throw new InvalidDataException("Enum member name is missing.");
                var value = (string?)member.Attribute("value") ?? throw new InvalidDataException("Enum member value is missing.");
                if (!long.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _) &&
                    !ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out _))
                {
                    throw new InvalidDataException("Enum member value is not an integer.");
                }
                return new EnumMember(name, (string?)member.Attribute("description") ?? name, value);
            }).ToArray();
            result.Add((assembly, type), new EnumInfo(flags, members));
        }
        return result;
    }

    // Microsoft.OpenApi 3.x models Const as string; write the validated numeric JSON literal.
    private sealed class NumericConstExtension(string value) : IOpenApiExtension
    {
        public void Write(IOpenApiWriter writer, OpenApiSpecVersion version) => writer.WriteRaw(value);
    }

    private sealed record EnumInfo(bool Flags, IReadOnlyList<EnumMember> Members);
    private sealed record EnumMember(string Name, string Description, string Value);
}
