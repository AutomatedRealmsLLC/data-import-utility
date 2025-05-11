using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Helpers; // Required for TransformationResultHelpers
using AbstractionsModels = AutomatedRealms.DataImportUtility.Abstractions.Models;

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
    public override async Task<AbstractionsModels.TransformationResult> ApplyTransformationAsync(AbstractionsModels.TransformationResult previousResult)
    {
        await Task.CompletedTask; // Simulate async operation if no true async work
        try
        {
            if (previousResult.WasFailure)
            {
                return previousResult;
            }
            // The Interpolate extension method is expected to handle the logic
            return previousResult.Interpolate(TransformationDetail);
        }
        catch (Exception ex)
        {
            // Corrected Failure call
            return AbstractionsModels.TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: previousResult.CurrentValueType, // Or OutputType if more appropriate before failure
                errorMessage: ex.Message,
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
        await Task.CompletedTask; // Simulate async operation
        var initialResult = AbstractionsModels.TransformationResult.Success(
            originalValue: value, 
            originalValueType: value?.GetType() ?? typeof(object), 
            currentValue: value, 
            currentValueType: value?.GetType() ?? typeof(object),
            record: null, // Assuming no DataRow context in this direct Transform call
            tableDefinition: null // Assuming no TableDefinition context here
            );
        
        try
        {
            if (initialResult.WasFailure) 
            {
                return initialResult;
            }
            // The Interpolate extension method is expected to handle the logic
            return initialResult.Interpolate(TransformationDetail);
        }
        catch (Exception ex)
        {
            // Corrected Failure call
            return AbstractionsModels.TransformationResult.Failure(
                originalValue: initialResult.OriginalValue,
                targetType: targetType, // targetType from method signature
                errorMessage: ex.Message,
                originalValueType: initialResult.OriginalValueType,
                currentValueType: null,
                appliedTransformations: initialResult.AppliedTransformations,
                record: initialResult.Record,
                tableDefinition: initialResult.TableDefinition);
        }
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = (InterpolateTransformation)MemberwiseClone();
        clone.TransformationDetail = TransformationDetail;
        return clone;
    }
}

/// <summary>
/// The extension methods for the <see cref="InterpolateTransformation" />.
/// </summary>
public static class InterpolateTransformationExtensions
{
    /// <summary>
    /// Interpolates the value.
    /// </summary>
    /// <param name="result">The result of the previous transformation, containing the CurrentValue to be interpolated.</param>
    /// <param name="interpolateDetail">The interpolation pattern to use.</param>
    /// <returns>The result of the transformation.</returns>
    public static AbstractionsModels.TransformationResult Interpolate(this AbstractionsModels.TransformationResult result, string? interpolateDetail)
    {
        string? pattern = interpolateDetail; // pattern is nullable
        
        if (string.IsNullOrWhiteSpace(pattern))
        {
            // No transformation pattern to apply, return current value as string or original error.
            return result.WasFailure ? result : AbstractionsModels.TransformationResult.Success(
                originalValue: result.OriginalValue, 
                originalValueType: result.OriginalValueType, 
                currentValue: result.CurrentValue?.ToString(), // Ensure current value is string
                currentValueType: typeof(string),              // Type is string
                appliedTransformations: result.AppliedTransformations,
                record: result.Record, 
                tableDefinition: result.TableDefinition);
        }

        if (result.WasFailure) // If previous steps failed, propagate the error.
        {
            return result;
        }

        // Using TransformationResultHelpers extension methods correctly with 'result'
        if (TransformationResultHelpers.IsNullOrNullValue(result)) // Pass the whole result
        {
            // If current value in result is null/empty, replace placeholders with empty string.
        }

        var valuesToUse = TransformationResultHelpers.ResultValueAsArray(result); // Pass the whole result

        string? interpolatedValue = pattern; // Initialize with the pattern, nullable

        if (valuesToUse != null) 
        {
            for (var i = 0; i < valuesToUse.Length; i++)
            {
                interpolatedValue = interpolatedValue?.Replace($"${{{i}}}", valuesToUse[i]?.ToString() ?? string.Empty);
            }
        }
        else // if valuesToUse is null (e.g. result.CurrentValue was null and not array-like)
        {
            // This case might mean the pattern itself is returned if it had no placeholders,
            // or placeholders are effectively replaced with empty if not caught by the loop.
            // The loop with valuesToUse[i]?.ToString() ?? string.Empty handles nulls in the array.
            // If valuesToUse is null, the loop doesn't run.
            // Consider if pattern should be cleared of placeholders if CurrentValue is fundamentally null.
            // For now, if valuesToUse is null, interpolatedValue remains the original pattern.
            // A more robust approach for ${i} replacement when valuesToUse is null might be needed
            // if the pattern should be "emptied" of its placeholders.
            // However, ResultValueAsArray(result_with_null_current_value) usually yields string[]{null}, so valuesToUse wouldn't be null.
        }

        return AbstractionsModels.TransformationResult.Success(
            originalValue: result.OriginalValue, 
            originalValueType: result.OriginalValueType, 
            currentValue: interpolatedValue, 
            currentValueType: typeof(string), // Output is always string
            appliedTransformations: result.AppliedTransformations,
            record: result.Record, 
            tableDefinition: result.TableDefinition);
    }
}
