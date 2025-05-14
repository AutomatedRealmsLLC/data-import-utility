using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// This class is used to map a value from one value to another.
/// </summary>
public class MapTransformation : ValueTransformationBase
{
    /// <summary>
    /// Static TypeId for this transformation.
    /// </summary>
    public static readonly string TypeIdString = "Core.MapTransformation";

    /// <summary>
    /// Gets or sets the dictionary of value mappings.
    /// </summary>
    public Dictionary<string, string> Mappings { get; set; } = [];

    /// <summary>
    /// Gets or sets the default value to use if no mapping is found.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapTransformation"/> class.
    /// </summary>
    public MapTransformation() : base(TypeIdString)
    {
    }

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
    public override bool IsEmpty => Mappings is null || !Mappings.Any();

    /// <inheritdoc />
    [JsonIgnore]
    public override Type OutputType => typeof(string); // Assuming mapped values are generally strings or converted to strings.

    /// <inheritdoc />
    public override Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult)
    {
        try
        {
            if (previousResult.WasFailure)
            {
                return Task.FromResult(previousResult);
            }

            TransformationResult checkedResult = TransformationResultHelpers.ErrorIfCollection(previousResult, OperationInvalidForCollectionsMessage);
            if (checkedResult.WasFailure)
            {
                return Task.FromResult(checkedResult);
            }

            if (Mappings is null || !Mappings.Any())
            {
                return Task.FromResult(checkedResult); // No mappings provided, return original (potentially error-checked) result
            }

            var currentInputValueString = checkedResult.CurrentValue?.ToString();

            if (currentInputValueString is not null && Mappings.TryGetValue(currentInputValueString, out var mappedValue))
            {
                return Task.FromResult(TransformationResult.Success(
                    originalValue: checkedResult.OriginalValue,
                    originalValueType: checkedResult.OriginalValueType,
                    currentValue: mappedValue,
                    currentValueType: mappedValue?.GetType() ?? typeof(string),
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
            return Task.FromResult(TransformationResult.Failure(
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
    public override async Task<TransformationResult> Transform(object? value, Type targetType)
    {
        var initialResult = TransformationResult.Success(
            originalValue: value,
            originalValueType: value?.GetType() ?? typeof(object),
            currentValue: value,
            currentValueType: value?.GetType() ?? typeof(object),
            appliedTransformations: [],
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: targetType
        );
        return await ApplyTransformationAsync(initialResult);
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = new MapTransformation();
        CloneBaseProperties(clone);
        clone.Mappings = new Dictionary<string, string>(Mappings);
        clone.DefaultValue = DefaultValue;
        return clone;
    }
}
