using System.Text.Json.Serialization;
using System.Collections.Generic; // For List<T>
using System.Linq; // For .Select, .ToList, .FirstOrDefault, .Where
using System.Threading.Tasks; // For Task
using System;
using AutomatedRealms.DataImportUtility.Abstractions;
using AbstractionsModels = AutomatedRealms.DataImportUtility.Abstractions.Models; // Alias for Abstractions.Models
using AutomatedRealms.DataImportUtility.Core.Helpers; 
using AutomatedRealms.DataImportUtility.Core.Models;   // For ValueMap

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations; 

/// <summary>
/// This class is used to map a value from one value to another.
/// </summary>
public class MapTransformation : ValueTransformationBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 5;

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
    [JsonIgnore]
    public override bool IsEmpty => ValueMappings == null || !ValueMappings.Any();

    /// <inheritdoc />
    [JsonIgnore]
    public override Type OutputType => typeof(string); // Assuming mapped values are generally strings or converted to strings.

    /// <summary>
    /// The list of value mappings to use.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ValueMap> ValueMappings { get; set; } = new(); 

    /// <summary>
    /// The field name to map the value to. (Currently not used by the Map extension logic, but kept for potential future use or if logic changes)
    /// </summary>
    /// <remarks>
    /// If the field name is null or empty, the mapping will return the first match, if any.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? FieldName { get; set; }

    /// <inheritdoc />
    public override async Task<AbstractionsModels.TransformationResult> ApplyTransformationAsync(AbstractionsModels.TransformationResult previousResult)
    {
        try
        {
            AbstractionsModels.TransformationResult transformedResult = previousResult.Map(ValueMappings, FieldName);
            return await Task.FromResult(transformedResult);
        }
        catch (Exception ex)
        {
            return previousResult with 
            { 
                ErrorMessage = ex.Message,
                CurrentValue = previousResult.CurrentValue,
                CurrentValueType = previousResult.CurrentValueType 
            };
        }
    }

    /// <inheritdoc />
    public override Task<AbstractionsModels.TransformationResult> Transform(object? value, Type targetType)
    {
        var initialResult = AbstractionsModels.TransformationResult.Success(
            originalValue: value,
            originalValueType: value?.GetType() ?? typeof(object),
            currentValue: value,
            currentValueType: value?.GetType() ?? typeof(object)
        );

        try
        {
            AbstractionsModels.TransformationResult transformedResult = initialResult.Map(ValueMappings, FieldName);
            return Task.FromResult(transformedResult);
        }
        catch (Exception ex)
        {
            return Task.FromResult(initialResult with { ErrorMessage = ex.Message });
        }
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = (MapTransformation)MemberwiseClone();
        clone.FieldName = this.FieldName; 
        // ValueMappings is a list of complex objects (ValueMap), requires deep cloning.
        clone.ValueMappings = this.ValueMappings.Select(x => x.Clone()).ToList(); // Assumes ValueMap has a Clone method
        // TransformationDetail is not directly used by MapTransformation properties but is part of base, clone if necessary.
        clone.TransformationDetail = this.TransformationDetail;
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
    /// <param name="result">The result of the previous transformation. <see cref="AbstractionsModels.TransformationResult.CurrentValue"/> is used as the value to map.</param>
    /// <param name="valueMappings">The mapping list. Each <see cref="ValueMap"/> contains a FromValue and ToValue.</param>
    /// <param name="fieldName">The field name to map the value to. (Currently, the extension logic primarily uses FromValue for matching, not fieldName from ValueMap model directly in filter).</param>
    /// <returns>A <see cref="AbstractionsModels.TransformationResult"/> with the <see cref="AbstractionsModels.TransformationResult.CurrentValue"/> updated to the mapped value if a match is found; otherwise, the original <see cref="AbstractionsModels.TransformationResult.CurrentValue"/>.</returns>
    /// <remarks>
    /// If the mapping list is empty, or no match is found for the <see cref="AbstractionsModels.TransformationResult.CurrentValue"/>, the original <paramref name="result"/> is returned unchanged.
    /// The comparison for mapping is typically case-sensitive. Consider adding options for case-insensitivity if needed.
    /// </remarks>
    public static AbstractionsModels.TransformationResult Map(this AbstractionsModels.TransformationResult result, List<ValueMap> valueMappings, string? fieldName)
    {
        AbstractionsModels.TransformationResult checkedResult = TransformationResultHelpers.ErrorIfCollection(result, ValueTransformationBase.OperationInvalidForCollectionsMessage);
        if (checkedResult.WasFailure)
        {
            return checkedResult;
        }

        if (valueMappings == null || !valueMappings.Any())
        { 
            return checkedResult; // No mappings provided, return original result
        }

        var currentInputValueString = checkedResult.CurrentValue?.ToString(); // Convert current value to string for comparison

        // The FieldName property on MapTransformation isn't directly used here to filter valueMappings by ValueMap.ImportedFieldName.
        // The existing logic seems to imply FieldName might be for a different purpose or future use.
        // The filtering `lookups.FirstOrDefault(x => x.FromValue == lookupValue)` directly uses the CurrentValue.
        // If `fieldName` from the parameters was intended to filter `valueMappings` based on `ValueMap.ImportedFieldName`, that logic would be: 
        // var relevantMappings = string.IsNullOrWhiteSpace(fieldName) ? valueMappings : valueMappings.Where(m => m.ImportedFieldName == fieldName).ToList();
        // For now, sticking to matching based on FromValue against CurrentValue.

        var mappedEntry = valueMappings.FirstOrDefault(x => x.FromValue == currentInputValueString);

        if (mappedEntry != null)
        {
            return checkedResult with 
            { 
                CurrentValue = mappedEntry.ToValue, 
                CurrentValueType = mappedEntry.ToValue?.GetType() ?? typeof(string) // Type of the mapped value
            };
        }

        return checkedResult; // No match found, return original result
    }
}
