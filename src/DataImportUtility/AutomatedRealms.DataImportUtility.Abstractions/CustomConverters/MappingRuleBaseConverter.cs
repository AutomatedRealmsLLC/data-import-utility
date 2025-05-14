using AutomatedRealms.DataImportUtility.Abstractions.Services;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions.CustomConverters;

/// <summary>
/// The custom JSON Converter for <see cref="MappingRuleBase" />.
/// </summary>
public class MappingRuleBaseConverter : JsonConverter<MappingRuleBase>
{
    private readonly ITypeRegistryService _typeRegistryService;
    private const string TypeIdPropertyNameCamelCase = "typeId";
    private const string TypeIdPropertyNamePascalCase = "TypeId";

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingRuleBaseConverter"/> class.
    /// </summary>
    /// <param name="typeRegistryService">The type registry service to resolve TypeIds.</param>
    /// <exception cref="ArgumentNullException">Thrown if typeRegistryService is null.</exception>
    public MappingRuleBaseConverter(ITypeRegistryService typeRegistryService)
    {
        _typeRegistryService = typeRegistryService;
    }

    /// <inheritdoc />
    public override MappingRuleBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty(TypeIdPropertyNameCamelCase, out JsonElement typeIdElement) &&
            !root.TryGetProperty(TypeIdPropertyNamePascalCase, out typeIdElement))
        {
            throw new JsonException($"Property '{TypeIdPropertyNameCamelCase}' or '{TypeIdPropertyNamePascalCase}' not found for deserializing {nameof(MappingRuleBase)}.");
        }

        var typeIdString = typeIdElement.GetString();
        if (string.IsNullOrWhiteSpace(typeIdString))
        {
            throw new JsonException($"TypeId value is null or empty for {nameof(MappingRuleBase)}.");
        }

        // typeIdString is guaranteed not to be null here due to the IsNullOrWhiteSpace check above.
        if (!_typeRegistryService.TryResolveType(typeIdString!, out Type? concreteType) || concreteType == null)
        {
            throw new JsonException($"Unable to resolve TypeId '{typeIdString}' to a registered type for {nameof(MappingRuleBase)}. Ensure the type is registered with ITypeRegistryService.");
        }

        if (!typeof(MappingRuleBase).IsAssignableFrom(concreteType))
        {
            throw new JsonException($"The resolved type '{concreteType.FullName}' for TypeId '{typeIdString}' does not inherit from {nameof(MappingRuleBase)}.");
        }

        var deserializedObject = JsonSerializer.Deserialize(root.GetRawText(), concreteType, options);

        return deserializedObject as MappingRuleBase
               ?? throw new JsonException($"Failed to deserialize {nameof(MappingRuleBase)} to concrete type '{concreteType.FullName}' using TypeId '{typeIdString}'. The deserialized object was null or not assignable.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, MappingRuleBase value, JsonSerializerOptions options)
    {
        // The TypeId property on the value object (e.g., CopyRule) will be serialized as part of its properties.
        // Ensure the concrete type's TypeId is correctly set before serialization.
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
