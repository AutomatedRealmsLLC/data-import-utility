using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Core.Helpers;

namespace AutomatedRealms.DataImportUtility.Core.CustomConverters;

/// <summary>
/// Custom JSON Converter for <see cref="ValueTransformationBase" />.
/// </summary>
public class ValueTransformationBaseConverter : JsonConverter<ValueTransformationBase>
{
    private static readonly string _opNamePropCamelCase = nameof(ValueTransformationBase.EnumMemberName).ToCamelCase()!;

    /// <inheritdoc />
    public override ValueTransformationBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty(_opNamePropCamelCase, out JsonElement opNameElement) || root.TryGetProperty(nameof(ValueTransformationBase.EnumMemberName), out opNameElement))
        {
            var operationName = opNameElement.GetString();
            var operationType = Enum.TryParse<ValueTransformationType>(operationName, out var vt) ? vt : throw new JsonException($"Failed to parse {nameof(ValueTransformationType)} from Name property value: '{operationName}'.");
            // Ensure GetClassType is accessible, potentially via Core.Helpers or Abstractions.Helpers
            return JsonSerializer.Deserialize(root.GetRawText(), operationType.GetClassType()!, options) as ValueTransformationBase
                ?? throw new JsonException($"Failed to deserialize {nameof(ValueTransformationBase)}.");
        }

        throw new JsonException($"{nameof(ValueTransformationBase.EnumMemberName)} property not found.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ValueTransformationBase value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}
