using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Collections;
using System.Text.Json.Serialization;
// Alias for Abstractions.Models to avoid ambiguity with any potential Core.Models.TransformationResult

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// Combines the transformed values of multiple fields into a single value.
/// </summary>
public partial class CombineFieldsTransformation : ValueTransformationBase
{
    /// <summary>
    /// Static TypeId for this transformation.
    /// </summary>
    public static readonly string TypeIdString = "Core.CombineFieldsTransformation";

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
    public List<FieldTransformation> SourceFieldTransforms { get; set; } = [];

    /// <inheritdoc />
    public override Type OutputType => typeof(string);

    /// <inheritdoc />
    public override bool IsEmpty => (SourceFieldTransforms == null || !SourceFieldTransforms.Any()) && string.IsNullOrWhiteSpace(TransformationDetail);

    /// <summary>
    /// Initializes a new instance of the <see cref="CombineFieldsTransformation"/> class.
    /// </summary>
    public CombineFieldsTransformation() : base(TypeIdString) { }

    /// <inheritdoc />
    public override Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult)
    {
        if (previousResult.WasFailure)
        {
            return Task.FromResult(previousResult);
        }

        string? pattern = this.TransformationDetail;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return Task.FromResult(TransformationResult.Success(
                originalValue: previousResult.OriginalValue,
                originalValueType: previousResult.OriginalValueType,
                currentValue: pattern, // pattern is null or whitespace
                currentValueType: typeof(string),
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                targetFieldType: previousResult.TargetFieldType
            ));
        }

        try
        {
            string?[] valuesToInterpolate;
            if (previousResult.CurrentValue == null)
            {
                valuesToInterpolate = [];
            }
            else if (previousResult.CurrentValue is IEnumerable<string> stringEnumerable)
            {
                valuesToInterpolate = [.. stringEnumerable];
            }
            else if (previousResult.CurrentValue is IEnumerable enumerableValue && previousResult.CurrentValue is not string)
            {
                valuesToInterpolate = [.. enumerableValue.Cast<object>().Select(o => o?.ToString() ?? string.Empty)];
            }
            else
            {
                valuesToInterpolate = [previousResult.CurrentValue.ToString() ?? string.Empty];
            }

            // pattern is guaranteed not null here due to the IsNullOrWhiteSpace check above.
            string finalCombinedValue = pattern!;
            for (var i = 0; i < valuesToInterpolate.Length; i++)
            {
                // finalCombinedValue is not null here as it's initialized from a non-null pattern
                finalCombinedValue = finalCombinedValue.Replace($"${{{i}}}", valuesToInterpolate[i] ?? string.Empty);
            }

            return Task.FromResult(TransformationResult.Success(
                originalValue: previousResult.OriginalValue,
                originalValueType: previousResult.OriginalValueType,
                currentValue: finalCombinedValue,
                currentValueType: typeof(string),
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                targetFieldType: previousResult.TargetFieldType
            ));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: OutputType, // Target type for this transformation
                errorMessage: $"Error in CombineFields transformation: {ex.Message}",
                originalValueType: previousResult.OriginalValueType,
                currentValueType: null, // Value is null due to failure
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                explicitTargetFieldType: previousResult.TargetFieldType // Use TargetFieldType from context
            ));
        }
    }

    /// <inheritdoc />
    public override async Task<TransformationResult> Transform(object? value, Type targetType)
    {
        // 'value' for CombineFields is expected to be an IEnumerable of the pre-transformed values to combine.
        var initialResult = TransformationResult.Success(
            originalValue: value, // This might be the collection itself, or null if not applicable directly
            originalValueType: value?.GetType() ?? typeof(object),
            currentValue: value, // This is the collection of values to be combined
            currentValueType: value?.GetType() ?? typeof(object),
            appliedTransformations: [],
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: targetType
        );
        // ApplyTransformationAsync expects previousResult.CurrentValue to be the list/array of items.
        return await ApplyTransformationAsync(initialResult);
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = new CombineFieldsTransformation
        {
            // Deep copy SourceFieldTransforms if it's not null, assuming FieldTransformation has a Clone method.
            SourceFieldTransforms = this.SourceFieldTransforms?.Select(t => t.Clone()).ToList() ?? []
        };
        // Clones TransformationDetail, TypeId
        this.CloneBaseProperties(clone);
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
// public static async Task<IEnumerable<TransformationResult>> CombineFields(
//     this IEnumerable<FieldTransformation> fieldTransforms, // FieldTransformation needs refactoring
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
// public static TransformationResult CombineFields(this TransformationResult result, string? operationDetail)
// {
//     // ... original logic ...
// }
// }
*/
