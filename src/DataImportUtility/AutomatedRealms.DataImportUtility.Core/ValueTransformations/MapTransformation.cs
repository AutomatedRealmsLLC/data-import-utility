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
    public override Task<AbstractionsModels.TransformationResult> ApplyTransformationAsync(AbstractionsModels.TransformationResult previousResult) // Removed async
    {
        try
        {
            if (previousResult.WasFailure)
            {
                return Task.FromResult(previousResult);
            }

            AbstractionsModels.TransformationResult checkedResult = TransformationResultHelpers.ErrorIfCollection(previousResult, ValueTransformationBase.OperationInvalidForCollectionsMessage);
            if (checkedResult.WasFailure)
            {
                return Task.FromResult(checkedResult);
            }

            if (ValueMappings == null || !ValueMappings.Any())
            { 
                return Task.FromResult(checkedResult); // No mappings provided, return original (potentially error-checked) result
            }

            var currentInputValueString = checkedResult.CurrentValue?.ToString();

            var mappedEntry = ValueMappings.FirstOrDefault(x => string.Equals(x.FromValue, currentInputValueString, StringComparison.Ordinal)); // Consider StringComparison option

            if (mappedEntry != null)
            {
                return Task.FromResult(AbstractionsModels.TransformationResult.Success(
                    originalValue: checkedResult.OriginalValue,
                    originalValueType: checkedResult.OriginalValueType,
                    currentValue: mappedEntry.ToValue, 
                    currentValueType: mappedEntry.ToValue?.GetType() ?? typeof(string),
                    appliedTransformations: checkedResult.AppliedTransformations,
                    record: checkedResult.Record,
                    tableDefinition: checkedResult.TableDefinition,
                    sourceRecordContext: checkedResult.SourceRecordContext,
                    targetFieldType: checkedResult.TargetFieldType
                ));
            }

            return Task.FromResult(checkedResult); // No match found, return original (error-checked) result
        }
        catch (Exception ex)
        {
            return Task.FromResult(AbstractionsModels.TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: OutputType, 
                errorMessage: ex.Message,
                originalValueType: previousResult.OriginalValueType,
                currentValueType: null, 
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                explicitTargetFieldType: previousResult.TargetFieldType
            ));
        }
    }

    /// <inheritdoc />
    public override async Task<AbstractionsModels.TransformationResult> Transform(object? value, Type targetType)
    {
        var initialResult = AbstractionsModels.TransformationResult.Success(
            originalValue: value,
            originalValueType: value?.GetType() ?? typeof(object),
            currentValue: value,
            currentValueType: value?.GetType() ?? typeof(object),
            appliedTransformations: new List<string>(),
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: targetType
        );
        // ApplyTransformationAsync now returns Task<TransformationResult>, so it can be awaited or returned directly if the signature matches.
        // Since Transform is async, we await it.
        return await ApplyTransformationAsync(initialResult);
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
