using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Collections; // Added for IEnumerable
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// A transformation operation that combines the extracted values into a single value.
/// </summary>
public class InterpolateTransformation : ValueTransformationBase
{
    /// <summary>
    /// Static TypeId for this transformation.
    /// </summary>
    public static readonly string TypeIdString = "Core.InterpolateTransformation";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Interpolate";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Interpolate";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Combine the extracted values into a single value.";

    /// <summary>
    /// The interpolation pattern to use.
    /// </summary>
    /// <remarks>
    /// Use syntax like `${0}` to interpolate the values that are the result of a previous transformation operation.
    /// </remarks>
    public override string? TransformationDetail { get; set; } = "${0}";

    /// <summary>
    /// Initializes a new instance of the <see cref="InterpolateTransformation"/> class.
    /// </summary>
    public InterpolateTransformation() : base(TypeIdString) { }

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => string.IsNullOrEmpty(TransformationDetail);

    /// <inheritdoc />
    [JsonIgnore]
    public override Type OutputType => typeof(string);

    /// <inheritdoc />
    public override Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult)
    {
        try
        {
            if (previousResult.WasFailure)
            {
                return Task.FromResult(previousResult);
            }

            string? pattern = TransformationDetail;

            if (string.IsNullOrWhiteSpace(pattern))
            {
                return Task.FromResult(TransformationResult.Success(
                    originalValue: previousResult.OriginalValue,
                    originalValueType: previousResult.OriginalValueType,
                    currentValue: previousResult.CurrentValue?.ToString(),
                    currentValueType: typeof(string),
                    appliedTransformations: previousResult.AppliedTransformations,
                    record: previousResult.Record,
                    tableDefinition: previousResult.TableDefinition,
                    sourceRecordContext: previousResult.SourceRecordContext,
                    targetFieldType: previousResult.TargetFieldType
                ));
            }

            // Logic from InterpolateTransformationExtensions.Interpolate starts here
            string?[] valuesToUse;
            if (previousResult.CurrentValue == null)
            {
                valuesToUse = [];
            }
            else if (previousResult.CurrentValue is IEnumerable<string> stringEnumerable)
            {
                valuesToUse = [.. stringEnumerable];
            }
            else if (previousResult.CurrentValue is IEnumerable enumerableValue && previousResult.CurrentValue is not string)
            {
                valuesToUse = [.. enumerableValue.Cast<object>().Select(o => o?.ToString())];
            }
            else
            {
                valuesToUse = [previousResult.CurrentValue.ToString()];
            }

            // pattern is guaranteed not null or whitespace here due to the earlier check.
            string interpolatedValue = pattern!;

            for (var i = 0; i < valuesToUse.Length; i++)
            {
                // interpolatedValue is not null here as it's initialized from a non-null pattern
                interpolatedValue = interpolatedValue.Replace($"${{{i}}}", valuesToUse[i] ?? string.Empty);
            }
            // End of integrated logic

            return Task.FromResult(TransformationResult.Success(
                originalValue: previousResult.OriginalValue,
                originalValueType: previousResult.OriginalValueType,
                currentValue: interpolatedValue,
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

        return await ApplyTransformationAsync(initialResult); // ApplyTransformationAsync is now Task-based
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = new InterpolateTransformation()
        {
            TransformationDetail = this.TransformationDetail
        };
        // TypeId is set by the constructor
        return clone;
    }
}
