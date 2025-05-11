using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System; // For Exception, Type
using System.Linq; // For .OfType, .Select, .ToArray, .FirstOrDefault
using System.Threading.Tasks; // For Task
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult
using AutomatedRealms.DataImportUtility.Core.Helpers;

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
    public override async Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult)
    {
        try
        {
            // The RegexMatch extension method is defined below and operates on TransformationResult
            TransformationResult transformedResult = previousResult.RegexMatch(TransformationDetail);
            return await Task.FromResult(transformedResult);
        }
        catch (ArgumentException ex) // Catch specific Regex argument errors
        {
            return previousResult with 
            { 
                ErrorMessage = $"Regex pattern error: {ex.Message}",
                CurrentValue = previousResult.CurrentValue,
                CurrentValueType = previousResult.CurrentValueType 
            };
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
            TransformationResult transformedResult = initialResult.RegexMatch(TransformationDetail);
            return Task.FromResult(transformedResult);
        }
        catch (ArgumentException ex) // Catch specific Regex argument errors
        {
             return Task.FromResult(initialResult with { ErrorMessage = $"Regex pattern error: {ex.Message}" });
        }
        catch (Exception ex)
        {
            return Task.FromResult(initialResult with { ErrorMessage = ex.Message });
        }
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

/// <summary>
/// The extension methods for the <see cref="RegexMatchTransformation" /> to be used with the scripting engine.
/// </summary>
public static class RegexMatchTransformationExtensions
{
    /// <summary>
    /// Performs a Regex match on the input value result.
    /// </summary>
    /// <param name="result">The result of the previous transformation. <see cref="TransformationResult.CurrentValue"/> is used as input.</param>
    /// <param name="regExPattern">The regular expression pattern to match against. If null or empty, the behavior depends on the input value.</param>
    /// <returns>
    /// A <see cref="TransformationResult"/> where <see cref="TransformationResult.CurrentValue"/> contains:
    /// - A single string if one match is found.
    /// - A JSON serialized string array if multiple matches are found.
    /// - The original <see cref="TransformationResult.CurrentValue"/> (potentially serialized if it was an object not a string) if <paramref name="regExPattern"/> is null or whitespace.
    /// - An empty string if the input value is null or empty and <paramref name="regExPattern"/> is valid.
    /// The <see cref="TransformationResult.CurrentValueType"/> will be <see cref="string"/>.
    /// </returns>
    public static TransformationResult RegexMatch(this TransformationResult result, string? regExPattern)
    {
        TransformationResult checkedResult = TransformationResultHelpers.ErrorIfCollection(result, ValueTransformationBase.OperationInvalidForCollectionsMessage);
        if (checkedResult.WasFailure)
        {
            return checkedResult;
        }

        var currentInputValue = checkedResult.CurrentValue?.ToString();

        if (string.IsNullOrWhiteSpace(regExPattern))
        {
            // If no pattern, return current value. If it's an object, it might need serialization to fit string type,
            // or we decide CurrentValue can remain object. For now, let's assume it should be string.
            // If CurrentValue was already a string or null, ToString() handles it.
            // If it was some other object, ToString() might not be what we want.
            // However, Regex typically operates on strings.
            return checkedResult with 
            { 
                CurrentValue = currentInputValue ?? string.Empty, // Ensure it's a string
                CurrentValueType = typeof(string) 
            };
        }
        
        if (string.IsNullOrEmpty(currentInputValue)) // Check after pattern check, as pattern might be invalid
        {
             // If input is null/empty but pattern is valid, result is typically no matches (empty string or empty array)
            return checkedResult with { CurrentValue = string.Empty, CurrentValueType = typeof(string) };
        }

        try
        {
            var regEx = new Regex(regExPattern); // Can throw ArgumentException if pattern is invalid

            var matches = regEx.Matches(currentInputValue!) // currentInputValue is not null here due to earlier check
                .OfType<Match>()
                .Select(match => match.Value)
                .ToArray();

            if (matches.Length == 0)
            {
                return checkedResult with { CurrentValue = string.Empty, CurrentValueType = typeof(string) };
            }
            else if (matches.Length == 1)
            {
                return checkedResult with { CurrentValue = matches[0], CurrentValueType = typeof(string) };
            }
            else // Multiple matches
            {
                return checkedResult with 
                { 
                    CurrentValue = JsonSerializer.Serialize(matches), 
                    CurrentValueType = typeof(string) // JSON string
                };
            }
        }
        catch (ArgumentException ex) // Catch Regex pattern compilation errors
        {
            return checkedResult with { ErrorMessage = $"Invalid regex pattern: {ex.Message}", CurrentValueType = typeof(string) };
        }
    }
}
