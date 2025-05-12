using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using System.Linq; // Added for Select
using System.Collections; // Added for IEnumerable
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Helpers; // Required for TransformationResultHelpers
using AbstractionsModels = AutomatedRealms.DataImportUtility.Abstractions.Models;
using System.Collections.Generic; // Added for List<string>

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// A transformation operation that combines the extracted values into a single value.
/// </summary>
public class InterpolateTransformation : ValueTransformationBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 4;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(InterpolateTransformation);

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

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => string.IsNullOrEmpty(TransformationDetail);

    /// <inheritdoc />
    [JsonIgnore]
    public override Type OutputType => typeof(string);

    /// <inheritdoc />
    public override Task<AbstractionsModels.TransformationResult> ApplyTransformationAsync(AbstractionsModels.TransformationResult previousResult)
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
                return Task.FromResult(AbstractionsModels.TransformationResult.Success(
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
                valuesToUse = Array.Empty<string?>();
            }
            else if (previousResult.CurrentValue is IEnumerable<string> stringEnumerable)
            {
                valuesToUse = stringEnumerable.ToArray();
            }
            else if (previousResult.CurrentValue is IEnumerable enumerableValue && !(previousResult.CurrentValue is string))
            {
                valuesToUse = enumerableValue.Cast<object>().Select(o => o?.ToString()).ToArray();
            }
            else
            {
                valuesToUse = new[] { previousResult.CurrentValue.ToString() };
            }
            
            // pattern is guaranteed not null or whitespace here due to the earlier check.
            string interpolatedValue = pattern!;

            for (var i = 0; i < valuesToUse.Length; i++)
            {
                // interpolatedValue is not null here as it's initialized from a non-null pattern
                interpolatedValue = interpolatedValue.Replace($"${{{i}}}", valuesToUse[i] ?? string.Empty);
            }
            // End of integrated logic

            return Task.FromResult(AbstractionsModels.TransformationResult.Success(
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
        
        return await ApplyTransformationAsync(initialResult); // ApplyTransformationAsync is now Task-based
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = (InterpolateTransformation)MemberwiseClone();
        clone.TransformationDetail = TransformationDetail;
        return clone;
    }
}
