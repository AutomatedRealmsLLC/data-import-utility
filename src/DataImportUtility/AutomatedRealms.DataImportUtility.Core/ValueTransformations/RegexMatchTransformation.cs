using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers; // Required for List<string> in TransformationResult
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// Represents a transformation operation that matches a value against a regular expression and returns the set of matches.
/// </summary>
public class RegexMatchTransformation : ValueTransformationBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 6;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(RegexMatchTransformation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Regex Match";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Regex";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Matches a value against a regular expression and returns the set of matches.";

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => string.IsNullOrWhiteSpace(TransformationDetail);

    /// <inheritdoc />
    [JsonIgnore]
    public override Type OutputType => typeof(string); // Output can be a single string or a JSON string of an array

    /// <inheritdoc />
    public override Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult)
    {
        try
        {
            if (previousResult.WasFailure)
            {
                return Task.FromResult(previousResult);
            }

            TransformationResult checkedResult = TransformationResultHelpers.ErrorIfCollection(previousResult, ValueTransformationBase.OperationInvalidForCollectionsMessage);
            if (checkedResult.WasFailure)
            {
                return Task.FromResult(checkedResult);
            }

            var currentInputValue = checkedResult.CurrentValue?.ToString();
            string? regExPattern = TransformationDetail;

            if (string.IsNullOrWhiteSpace(regExPattern))
            {
                return Task.FromResult(TransformationResult.Success(
                    originalValue: checkedResult.OriginalValue,
                    originalValueType: checkedResult.OriginalValueType,
                    currentValue: currentInputValue ?? string.Empty,
                    currentValueType: typeof(string),
                    appliedTransformations: checkedResult.AppliedTransformations,
                    record: checkedResult.Record,
                    tableDefinition: checkedResult.TableDefinition,
                    sourceRecordContext: checkedResult.SourceRecordContext,
                    targetFieldType: checkedResult.TargetFieldType
                ));
            }

            if (string.IsNullOrEmpty(currentInputValue))
            {
                return Task.FromResult(TransformationResult.Success(
                    originalValue: checkedResult.OriginalValue,
                    originalValueType: checkedResult.OriginalValueType,
                    currentValue: string.Empty,
                    currentValueType: typeof(string),
                    appliedTransformations: checkedResult.AppliedTransformations,
                    record: checkedResult.Record,
                    tableDefinition: checkedResult.TableDefinition,
                    sourceRecordContext: checkedResult.SourceRecordContext,
                    targetFieldType: checkedResult.TargetFieldType
                ));
            }

            Regex regEx;
            try
            {
                regEx = new Regex(regExPattern);
            }
            catch (ArgumentException ex) // Catch Regex pattern compilation errors
            {
                return Task.FromResult(TransformationResult.Failure(
                    originalValue: checkedResult.OriginalValue,
                    targetType: OutputType,
                    errorMessage: $"Invalid regex pattern: {ex.Message}",
                    originalValueType: checkedResult.OriginalValueType,
                    currentValueType: typeof(string), // Still string type, but failed.
                    appliedTransformations: checkedResult.AppliedTransformations,
                    record: checkedResult.Record,
                    tableDefinition: checkedResult.TableDefinition,
                    sourceRecordContext: checkedResult.SourceRecordContext,
                    explicitTargetFieldType: checkedResult.TargetFieldType
                ));
            }

            var matches = regEx.Matches(currentInputValue) // currentInputValue is not null here due to earlier check
                .OfType<Match>()
                .Select(match => match.Value)
                .ToArray();

            object? finalValue;
            if (matches.Length == 0)
            {
                finalValue = string.Empty;
            }
            else if (matches.Length == 1)
            {
                finalValue = matches[0];
            }
            else // Multiple matches
            {
                finalValue = JsonSerializer.Serialize(matches);
            }

            return Task.FromResult(TransformationResult.Success(
                originalValue: checkedResult.OriginalValue,
                originalValueType: checkedResult.OriginalValueType,
                currentValue: finalValue,
                currentValueType: typeof(string), // Output is always string (or JSON string)
                appliedTransformations: checkedResult.AppliedTransformations,
                record: checkedResult.Record,
                tableDefinition: checkedResult.TableDefinition,
                sourceRecordContext: checkedResult.SourceRecordContext,
                targetFieldType: checkedResult.TargetFieldType
            ));
        }
        catch (Exception ex) // Catch any other unexpected errors
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
            appliedTransformations: new List<string>(), // Initialize with empty list
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: targetType
        );

        // ApplyTransformationAsync is non-async in its core logic but returns Task, so await it.
        return await ApplyTransformationAsync(initialResult);
    }

    /// <summary>
    /// Generates the syntax for the transformation detail.
    /// This is not an override from ValueTransformationBase.
    /// </summary>
    /// <remarks>
    /// Currently, this method just returns the TransformationDetail.
    /// It could be expanded to build a regex pattern if a more complex UI/configuration is introduced.
    /// </remarks>
    public string GenerateSyntax()
    {
        return TransformationDetail ?? string.Empty;
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = (RegexMatchTransformation)MemberwiseClone();
        clone.TransformationDetail = this.TransformationDetail;
        return clone;
    }
}
