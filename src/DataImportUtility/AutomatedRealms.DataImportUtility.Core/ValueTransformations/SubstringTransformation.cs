using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Correct TransformationResult
using AutomatedRealms.DataImportUtility.Core.Helpers;

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
    public override async Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult)
    {
        try
        {
            TransformationResult transformedResult = previousResult.Substring(StartIndex, MaxLength ?? int.MaxValue);
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
    public override Task<TransformationResult> Transform(object? value, Type targetType)
    {
        var initialResult = TransformationResult.Success(
            originalValue: value,
            originalValueType: value?.GetType() ?? typeof(object),
            currentValue: value, 
            currentValueType: value?.GetType() ?? typeof(object)
        );

        try
        {
            TransformationResult calculatedResult = initialResult.Substring(this.StartIndex, this.MaxLength ?? int.MaxValue);
            return Task.FromResult(calculatedResult);
        }
        catch (Exception ex)
        {
            return Task.FromResult(initialResult with { ErrorMessage = ex.Message });
        }
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
        clone.TransformationDetail = this.TransformationDetail; // Ensure TransformationDetail is also explicitly cloned
        return clone;
    }
}

/// <summary>
/// The extension methods for the <see cref="SubstringTransformation" /> to be used with the scripting engine.
/// </summary>
public static class SubstringTransformationExtensions
{
    /// <summary>
    /// Performs a Substring operation on the input value result.
    /// </summary>
    /// <param name="result">The result of the previous transformation.</param>
    /// <param name="startIndex">The starting index of the substring operation. 
    /// Use a negative start index to indicate an offset from the end of the string.</param>
    /// <param name="maxLength">The maximum length of the substring. 
    /// Use <see cref="int.MaxValue"/> to take characters to the end of the string. 
    /// Use a negative value to specify a length relative to the end of the string after the start index.</param>
    /// <returns>A <see cref="TransformationResult"/> containing the substring or an error message.</returns>
    public static TransformationResult Substring(this TransformationResult result, int startIndex = 0, int maxLength = int.MaxValue)
    {
        TransformationResult checkedResult = TransformationResultHelpers.ErrorIfCollection(result, ValueTransformationBase.OperationInvalidForCollectionsMessage);
        if (checkedResult.WasFailure)
        {
            return checkedResult;
        }

        var currentInputValue = checkedResult.CurrentValue?.ToString();

        if (string.IsNullOrWhiteSpace(currentInputValue))
        {
            return checkedResult with { CurrentValue = string.Empty, CurrentValueType = typeof(string) };
        }

        // If IsNullOrWhiteSpace is false, currentInputValue is not null.
        string originalValue = currentInputValue!; 
        int actualStartIndex = startIndex;

        if (actualStartIndex < 0)
        {
            actualStartIndex = originalValue.Length + actualStartIndex;
        }
        actualStartIndex = Math.Max(0, Math.Min(actualStartIndex, originalValue.Length));

        int actualLength = maxLength;
        if (maxLength == int.MaxValue)
        {
            actualLength = originalValue.Length - actualStartIndex;
        }
        else if (maxLength < 0)
        {
            actualLength = (originalValue.Length - actualStartIndex) + maxLength; 
        }
        actualLength = Math.Max(0, Math.Min(actualLength, originalValue.Length - actualStartIndex));
        
        string resultValue = originalValue.Substring(actualStartIndex, actualLength);

        return checkedResult with { CurrentValue = resultValue, CurrentValueType = typeof(string) };
    }
}
