using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataImportUtility.CustomConverters;


/// <summary>
/// A JSON converter for serializing and deserializing <see cref="ImmutableList{T}"/> objects.
/// Converts between <see cref="ImmutableList{T}"/> and JSON arrays during serialization/deserialization.
/// </summary>
/// <typeparam name="T">The type of elements in the immutable list.</typeparam>
public class ImmutableListConverter<T> : JsonConverter<ImmutableList<T>>
{
    /// <summary>
    /// Reads and converts JSON to an <see cref="ImmutableList{T}"/>.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>An <see cref="ImmutableList{T}"/> containing the deserialized elements, or an empty list if the JSON is null.</returns>
    public override ImmutableList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
        return list?.ToImmutableList() ?? [];
    }

    /// <summary>
    /// Writes an <see cref="ImmutableList{T}"/> as JSON.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The <see cref="ImmutableList{T}"/> to serialize.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void Write(Utf8JsonWriter writer, ImmutableList<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToList(), options);
    }
}