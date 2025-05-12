using System.Text.Json;
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers; // Required for List<string> in TransformationResult
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// Represents a transformation operation that extracts a substring from a value.
/// </summary>
public class SubstringTransformation : ValueTransformationBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 7;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(SubstringTransformation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName => "Substring";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Substring";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description => "Extracts a substring from a value given a starting character index and/or maximum length of characters to return.";

    private int _startIndex;
    /// <summary>
    /// The starting index of the substring operation
    /// </summary>
    /// <remarks>
    /// Use a negative number to indicate that we want to start from the end of the string minus the absolute value of the number.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int StartIndex
    {
        get => _startIndex;
        set
        {
            if (_startIndex == value) { return; }
            _startIndex = value;
            GenerateSyntax();
        }
    }

    private int? _maxLength;
    /// <summary>
    /// The length of the substring operation
    /// </summary>
    /// <remarks>
    /// Use a max length of int.MaxValue to indicate that we don't want to limit the length of the substring.
    /// Use a negative number to indicate that we want the length of the string minus the absolute value of the number.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? MaxLength
    {
        get => _maxLength;
        set
        {
            if (_maxLength == value) { return; }
            _maxLength = value;
            GenerateSyntax();
        }
    }

    /// <inheritdoc />
    [JsonIgnore]
    public override bool IsEmpty => this.StartIndex == 0 && (this.MaxLength == null || this.MaxLength == int.MaxValue);

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

            TransformationResult checkedResult = TransformationResultHelpers.ErrorIfCollection(previousResult, ValueTransformationBase.OperationInvalidForCollectionsMessage);
            if (checkedResult.WasFailure)
            {
                return Task.FromResult(checkedResult);
            }

            var currentInputValue = checkedResult.CurrentValue?.ToString();

            if (string.IsNullOrWhiteSpace(currentInputValue))
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

            // If IsNullOrWhiteSpace is false, currentInputValue is not null.
            string originalValueString = currentInputValue!;
            int localStartIndex = this.StartIndex;
            int localMaxLength = this.MaxLength ?? int.MaxValue;

            int actualStartIndex = localStartIndex;
            if (actualStartIndex < 0)
            {
                actualStartIndex = originalValueString.Length + actualStartIndex;
            }
            actualStartIndex = Math.Max(0, Math.Min(actualStartIndex, originalValueString.Length));

            int actualLength = localMaxLength;
            if (localMaxLength == int.MaxValue)
            {
                actualLength = originalValueString.Length - actualStartIndex;
            }
            else if (localMaxLength < 0)
            {
                actualLength = (originalValueString.Length - actualStartIndex) + localMaxLength;
            }
            actualLength = Math.Max(0, Math.Min(actualLength, originalValueString.Length - actualStartIndex));

            string resultValue = originalValueString.Substring(actualStartIndex, actualLength);

            return Task.FromResult(TransformationResult.Success(
                originalValue: checkedResult.OriginalValue,
                originalValueType: checkedResult.OriginalValueType,
                currentValue: resultValue,
                currentValueType: typeof(string),
                appliedTransformations: checkedResult.AppliedTransformations,
                record: checkedResult.Record,
                tableDefinition: checkedResult.TableDefinition,
                sourceRecordContext: checkedResult.SourceRecordContext,
                targetFieldType: checkedResult.TargetFieldType
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
            appliedTransformations: new List<string>(),
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: targetType
        );

        return await ApplyTransformationAsync(initialResult);
    }

    /// <summary>
    /// Generates the syntax for the transformation detail. This is not an override.
    /// </summary>
    public string GenerateSyntax()
    {
        TransformationDetail = JsonSerializer.Serialize(new { StartIndex, MaxLength });
        return TransformationDetail;
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = (SubstringTransformation)MemberwiseClone();
        clone.StartIndex = this.StartIndex;
        clone.MaxLength = this.MaxLength;
        // TransformationDetail is generated by StartIndex/MaxLength setters, 
        // so cloning them and calling GenerateSyntax (which happens in setters) is sufficient.
        // Explicitly setting it ensures it's correct if setters are bypassed or logic changes.
        clone.TransformationDetail = this.TransformationDetail;
        return clone;
    }
}
