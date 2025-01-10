using System.Text.Json;
using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;

namespace DataImportUtility.CustomConverters;

/// <summary>
/// The custom JSON Converter for <see cref="MappingRuleBase" />.
/// </summary>
public class MappingRuleBaseConverter : JsonConverter<MappingRuleBase>
{
    private static readonly string _opNamePropCamelCase = nameof(MappingRuleBase.EnumMemberName).ToCamelCase()!;

    /// <inheritdoc />
    public override MappingRuleBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty(_opNamePropCamelCase, out JsonElement opNameElement) || root.TryGetProperty(nameof(MappingRuleBase.EnumMemberName), out opNameElement))
        {
            var operationName = opNameElement.GetString();
            var operationType = Enum.TryParse<MappingRuleType>(operationName, out var vt) ? vt : throw new JsonException($"Failed to parse {nameof(MappingRuleType)} from Name property value: '{operationName}'.");
            return JsonSerializer.Deserialize(root.GetRawText(), operationType.GetClassType()!, options) as MappingRuleBase
                ?? throw new JsonException($"Failed to deserialize {nameof(MappingRuleBase)}.");
        }

        throw new JsonException($"{nameof(MappingRuleBase.EnumMemberName)} property not found.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, MappingRuleBase value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}
