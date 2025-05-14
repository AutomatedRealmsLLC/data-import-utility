using AutomatedRealms.DataImportUtility.Abstractions.Services;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions.CustomConverters;

/// <summary>
/// Custom JSON Converter for <see cref="ComparisonOperationBase" />.
/// </summary>
public class ComparisonOperationBaseConverter : JsonConverter<ComparisonOperationBase>
{
    private readonly ITypeRegistryService _typeRegistryService;
    private const string TypeIdPropertyNameCamelCase = "typeId";
    private const string TypeIdPropertyNamePascalCase = "TypeId";

    /// <summary>
    /// Initializes a new instance of the <see cref="ComparisonOperationBaseConverter"/> class.
    /// </summary>
    /// <param name="typeRegistryService">The type registry service to resolve TypeIds.</param>
    /// <exception cref="ArgumentNullException">Thrown if typeRegistryService is null.</exception>
    public ComparisonOperationBaseConverter(ITypeRegistryService typeRegistryService)
    {
        _typeRegistryService = typeRegistryService;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty(TypeIdPropertyNameCamelCase, out JsonElement typeIdElement) &&
            !root.TryGetProperty(TypeIdPropertyNamePascalCase, out typeIdElement))
        {
            throw new JsonException($"Property '{TypeIdPropertyNameCamelCase}' or '{TypeIdPropertyNamePascalCase}' not found for deserializing {nameof(ComparisonOperationBase)}.");
        }

        var typeIdString = typeIdElement.GetString();
        if (string.IsNullOrWhiteSpace(typeIdString))
        {
            throw new JsonException($"TypeId value is null or empty for {nameof(ComparisonOperationBase)}.");
        }

        if (!_typeRegistryService.TryResolveType(typeIdString!, out Type? concreteType) || concreteType is null)
        {
            throw new JsonException($"Unable to resolve TypeId '{typeIdString}' to a registered type for {nameof(ComparisonOperationBase)}. Ensure the type is registered with ITypeRegistryService.");
        }

        if (!typeof(ComparisonOperationBase).IsAssignableFrom(concreteType))
        {
            throw new JsonException($"The resolved type '{concreteType.FullName}' for TypeId '{typeIdString}' does not inherit from {nameof(ComparisonOperationBase)}.");
        }

        var deserializedObject = JsonSerializer.Deserialize(root.GetRawText(), concreteType, options);

        return deserializedObject as ComparisonOperationBase
               ?? throw new JsonException($"Failed to deserialize {nameof(ComparisonOperationBase)} to concrete type '{concreteType.FullName}' using TypeId '{typeIdString}'. The deserialized object was null or not assignable.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ComparisonOperationBase value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
