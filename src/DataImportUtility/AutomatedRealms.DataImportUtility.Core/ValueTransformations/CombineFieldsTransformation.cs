using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using AutomatedRealms.DataImportUtility.Abstractions;
// Alias for Abstractions.Models to avoid ambiguity with any potential Core.Models.TransformationResult
using AbstractionsModels = AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.Helpers; // For TransformationResultHelpers extensions

// Assuming FieldTransformation will be in Core.Models. This using is specific.
// If FieldTransformation moves, this will need to change.
// For now, we also assume Core.Models does NOT have its own TransformationResult,
// or if it does, the alias AbstractionsModels.TransformationResult will ensure we use the correct one.
using CoreModels = AutomatedRealms.DataImportUtility.Core.Models;

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// Combines the transformed values of multiple fields into a single value.
/// </summary>
public partial class CombineFieldsTransformation : ValueTransformationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(CombineFieldsTransformation);

    /// <summary>
    /// The error message when the number of values to combine does not match the number of field mappings.
    /// </summary>
    public const string ValueTransformationCountMismatchmessage = "The number of values to combine does not match the number of field mappings.";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Combine Fields";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Combines the transformed values of multiple fields into a single value.";

    /// <summary>
    /// The field transformations to use to combine the values.
    /// This property is used for configuration and validation but not directly in ApplyTransformationAsync logic here.
    /// The actual values to combine are expected in previousResult.CurrentValue.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<CoreModels.FieldTransformation> SourceFieldTransforms { get; set; } = [];

    /// <inheritdoc />
    public override Type OutputType => typeof(string);

    /// <inheritdoc />
    public override bool IsEmpty => (SourceFieldTransforms == null || !SourceFieldTransforms.Any()) && string.IsNullOrWhiteSpace(TransformationDetail);

    /// <inheritdoc />
    public override async Task<AbstractionsModels.TransformationResult> ApplyTransformationAsync(AbstractionsModels.TransformationResult previousResult)
    {
        await Task.CompletedTask; // Simulate async if no true async work needed before logic

        if (previousResult.WasFailure)
        {
            return previousResult;
        }

        string? pattern = this.TransformationDetail;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return AbstractionsModels.TransformationResult.Success(
                originalValue: previousResult.OriginalValue,
                originalValueType: previousResult.OriginalValueType,
                currentValue: pattern, // pattern is null or whitespace
                currentValueType: typeof(string),
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition);
        }

        try
        {
            // Corrected: Pass previousResult directly to the helper method.
            object?[]? valuesAsObjects = TransformationResultHelpers.ResultValueAsArray(previousResult);
            string?[] valuesToInterpolate = valuesAsObjects?.Select(v => v?.ToString()).ToArray() ?? [];

            string? finalCombinedValue = pattern;
            for (var i = 0; i < valuesToInterpolate.Length; i++)
            {
                if (finalCombinedValue == null) break; // Should not happen if pattern is not null
                finalCombinedValue = finalCombinedValue.Replace($"${{{i}}}", valuesToInterpolate[i] ?? string.Empty);
            }

            return AbstractionsModels.TransformationResult.Success(
                originalValue: previousResult.OriginalValue,
                originalValueType: previousResult.OriginalValueType,
                currentValue: finalCombinedValue,
                currentValueType: typeof(string),
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition);
        }
        catch (Exception ex)
        {
            return AbstractionsModels.TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: OutputType, // Target type for this transformation
                errorMessage: $"Error in CombineFields transformation: {ex.Message}",
                originalValueType: previousResult.OriginalValueType,
                currentValueType: null, // Value is null due to failure
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition);
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
            record: null, // No DataRow context in this direct Transform call
            tableDefinition: null // No TableDefinition context here
        );
        return await ApplyTransformationAsync(initialResult);
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        // Create a shallow copy for simple properties.
        var clone = (CombineFieldsTransformation)this.MemberwiseClone();
        // Deep copy SourceFieldTransforms if it's not null, assuming FieldTransformation has a Clone method.
        clone.SourceFieldTransforms = this.SourceFieldTransforms?.Select(ft => ft.Clone() as CoreModels.FieldTransformation ?? throw new InvalidOperationException("Cloned FieldTransformation is null or not of expected type.")).ToList() ?? [];
        return clone;
    }
}

/*
// The original extension methods are complex and involve `FieldTransformation` which itself needs refactoring.
// The logic for `TransformationResult.CombineFields(string? operationDetail)` has been integrated into the `Transform` method above.
// The `IEnumerable<FieldTransformation>.CombineFields` extension is a higher-level orchestration.
// It would typically:
// 1. Iterate through data rows or a set of primary inputs.
// 2. For each primary input, apply a series of `FieldTransformation` instances.
//    Each `FieldTransformation` might have its own chain of `ValueTransformationBase` instances.
// 3. Collect the results from these `FieldTransformation` applications.
// 4. Then, use the `CombineFieldsTransformation` (now the instance method `Transform`) to combine these collected results.
// This orchestration logic does not belong inside a single `ValueTransformationBase` implementation.
// It should be part of a data processing pipeline or a service that uses these transformations.
// For now, these extensions will be commented out. They need to be moved and redesigned
// as part of the overall refactoring of data processing logic.

// public static class CombineFieldsTransformationExtensions
// {
// This method orchestrated the application of multiple FieldTransformation objects
// and then combined their results. This is beyond the scope of a single ValueTransformationBase.
// public static async Task<IEnumerable<AbstractionsModels.TransformationResult>> CombineFields(
//     this IEnumerable<CoreModels.FieldTransformation> fieldTransforms, // FieldTransformation needs refactoring
//     string? transformationDetail // This is the pattern, now part of CombineFieldsTransformation instance
// Implicitly, this method also dealt with a "current row" or "current context"
// from which FieldTransformation instances would draw their initial values.
// )
// {
// ... original logic involving:
// - transformationDetail.TryGetPlaceholderMatches (now in Core.Helpers.StringHelpers)
// - Iterating over dataTable.Rows
// - Calling fieldTransform.Apply(curRow) (FieldTransformation.Apply needs refactoring)
// - Serializing results and then calling the other CombineFields extension.

// This logic needs to be re-thought.
// A possible approach:
// 1. A service takes a collection of "source values" (e.g., from a data row).
// 2. It applies the relevant FieldTransformation to each source value.
// 3. The results (which are strings or can be converted to strings) are collected.
// 4. These collected strings are then passed (perhaps as a JSON array string)
//    to the CombineFieldsTransformation.Transform method.

// throw new NotImplementedException("This extension method needs to be refactored into a higher-level processing service.");
// }

// The logic of this extension method is now primarily within CombineFieldsTransformation.Transform
// public static AbstractionsModels.TransformationResult CombineFields(this AbstractionsModels.TransformationResult result, string? operationDetail)
// {
//     // ... original logic ...
// }
// }
*/
