using System.Text.Json;
using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.ValueTransformations;

/// <summary>
/// Represents a transformation operation that extracts a substring from a value.
/// </summary>
public class SubstringTransformation : ValueTransformationBase
{
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
    private int _startIndex;

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
    private int? _maxLength;

    /// <inheritdoc />
    public override Task<TransformationResult> Apply(TransformationResult result)
    {
        try
        {
            return Task.FromResult(result.Substring(StartIndex, MaxLength ?? int.MaxValue));
        }
        catch (Exception ex)
        {
            return Task.FromResult(result with { ErrorMessage = ex.Message });
        }
    }

    /// <inheritdoc />
    public override string GenerateSyntax()
    {
        TransformationDetail = JsonSerializer.Serialize(new { StartIndex, MaxLength });
        return TransformationDetail;
    }
}

/// <summary>
/// The extension methods for the <see cref="SubstringTransformation" /> to be used with the scripting engine.
/// </summary>
public static class SubstringTransformationExtensions
{
    /// <summary>
    /// Performs a Regex match on the input value result.
    /// </summary>
    /// <param name="result">The result of the previous transformation.</param>
    /// <param name="startIndex">The starting index of the substring operation.</param>
    /// <param name="maxLength">The length of the substring operation.</param>
    /// <returns>
    /// The set of matches.
    /// </returns>
    /// <remarks>
    /// Use a negative start index to indicate that we want the index at the end 
    /// of the string minus the absolute value of the number.<br />
    /// <br />
    /// Use a max length of int.MaxValue to indicate that we don't want to limit 
    /// the length of the substring.<br />
    /// <br />
    /// Use a negative max length to indicate that we want the length of the string 
    /// remaining after the start index minus the absolute value of the number.
    /// </remarks>
    public static TransformationResult Substring(this TransformationResult result, int startIndex = 0, int maxLength = int.MaxValue)
    {
        // This cannot run on string collections
        if ((result = result.ErrorIfCollection(ValueTransformationBase.OperationInvalidForCollectionsMessage)).WasFailure)
        {
            return result;
        }

        if (string.IsNullOrWhiteSpace(result.Value))
        {
            return result;
        }

#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
        // Negative start index means start X characters from the end of the string
        var resultValue = result.Value?.Substring(
            Math.Min(
                result.Value.Length,
                Math.Max(
                    (startIndex < 0 ? result.Value.Length : 0) + startIndex,
                    0
                )
            ));

        // Negative max length means take the length of the string minus the absolute value of the number
        resultValue = resultValue?.Substring(0,
            Math.Min(
                resultValue.Length,
                Math.Max(
                    (maxLength < 0 ? (result.Value?.Length ?? 0) : 0) + maxLength,
                    0
                )
            )
        );
#else
        // Negative start index means start X characters from the end of the string
        var resultValue = result.Value[
            Math.Min(
                result.Value.Length,
                Math.Max(
                    (startIndex < 0 ? result.Value.Length : 0) + startIndex,
                    0
                )
            )..];

        // Negative max length means take the length of the string minus the absolute value of the number
        resultValue = resultValue[
            ..Math.Min(
                resultValue.Length,
                Math.Max(
                    (maxLength < 0 ? result.Value.Length : 0) + maxLength,
                    0
                )
            )
        ];
#endif

        return result with { Value = resultValue };
    }
}