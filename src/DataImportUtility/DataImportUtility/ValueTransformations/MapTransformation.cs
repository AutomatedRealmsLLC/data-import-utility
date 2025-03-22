using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.ValueTransformations;

/// <summary>
/// This class is used to map a value from one value to another.
/// </summary>
public class MapTransformation : ValueTransformationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(MapTransformation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Map Values";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Map";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Map the value of the source field to a new value using a mapping list.";

    /// <inheritdoc />
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ValueMap> ValueMappings { get; set; } = [];

    /// <summary>
    /// The field name to map the value to.
    /// </summary>
    /// <remarks>
    /// If the field name is null or empty, the mapping will return the first match, if any.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? FieldName { get; set; }

    /// <inheritdoc />
    /// <remarks>
    /// If the mapping list is empty, does not contain the source field or value, 
    /// or the source value is null, the original value is returned. Also, if the
    /// <see cref="FieldName"/> is empty or null, the first match is returned 
    /// without regard to the field name.
    /// </remarks>
    public override Task<TransformationResult> Apply(TransformationResult result)
    {
        try
        {
            return Task.FromResult(result.Map(ValueMappings, FieldName));
        }
        catch (Exception ex)
        {
            return Task.FromResult(result with { ErrorMessage = ex.Message });
        }
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = (MapTransformation)MemberwiseClone();
        clone.ValueMappings = ValueMappings
            .Select(x => new ValueMap()
            {
                ImportedFieldName = x.ImportedFieldName,
                FromValue = x.FromValue,
                ToValue = x.ToValue
            }).ToList();
        return clone;
    }
}

/// <summary>
/// The extension methods for the <see cref="MapTransformation" /> to be used with the scripting engine.
/// </summary>
public static class MapTransformationExtensions
{
    /// <summary>
    /// Maps the value of the source field to a new value using a mapping list.
    /// </summary>
    /// <param name="result">The result of the previous transformation.</param>
    /// <param name="valueMappings">The mapping list.</param>
    /// <param name="fieldName">The field name to map the value to.</param>
    /// <returns>The mapped value.</returns>
    /// <remarks>
    /// If the mapping list is empty, does not contain the source field or value, 
    /// or the source value is null, the original value is returned. Also, if the
    /// <paramref name="fieldName"/> is empty or null, the first match is returned
    /// without regard to the field name.
    /// </remarks>
    public static TransformationResult Map(this TransformationResult result, List<ValueMap> valueMappings, string? fieldName)
    {
        // This cannot run on string collections
        if ((result = result.ErrorIfCollection(ValueTransformationBase.OperationInvalidForCollectionsMessage)).WasFailure)
        {
            return result;
        }

        // Check if the valueMappings is empty
        if (valueMappings is not { Count: >0 }) { return result; }

        var lookups = valueMappings.Where(x => string.IsNullOrWhiteSpace(fieldName) || x.ImportedFieldName == fieldName).ToList();

        if (lookups.Count == 0) { return result; }

        var lookupValue = string.IsNullOrWhiteSpace(result.Value) ? string.Empty : result.Value;
        var mappedValue = lookups.FirstOrDefault(x => x.FromValue == lookupValue);
        if (mappedValue is null) { return result; }

        return result with { Value = mappedValue.ToValue };
    }
}