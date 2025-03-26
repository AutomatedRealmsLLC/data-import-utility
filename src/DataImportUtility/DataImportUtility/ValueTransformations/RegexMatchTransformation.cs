using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.ValueTransformations;

/// <summary>
/// Represents a transformation operation that matches a value against a regular expression and returns the set of matches.
/// </summary>
public class RegexMatchTransformation : ValueTransformationBase
{
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
    public override Task<TransformationResult> Apply(TransformationResult result)
    {
        try
        {
            return Task.FromResult(result.RegexMatch(TransformationDetail));
        }
        catch (Exception ex)
        {
            return Task.FromResult(result with { ErrorMessage = ex.Message });
        }
    }

    /// <inheritdoc />
    public override string GenerateSyntax()
    {
        // TODO: Consider if we are going to have a RegexMatch syntax builder
        // if so, we can use the TransformationDetail to store the regex pattern generated
        // by the builder property.

        return TransformationDetail ?? string.Empty;
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
    /// <param name="result">The result of the previous transformation.</param>
    /// <param name="regExPattern">The regular expression pattern to match against.</param>
    /// <returns>
    /// The set of matches.
    /// </returns>
    public static TransformationResult RegexMatch(this TransformationResult result, string? regExPattern)
    {
        if (string.IsNullOrWhiteSpace(regExPattern))
        {
            return result with
            {
                Value = result.Value is null
                    ? null
                    : JsonSerializer.Serialize(new[] { result.Value })
            };
        }

        // This cannot run on string collections
        if ((result = result.ErrorIfCollection(ValueTransformationBase.OperationInvalidForCollectionsMessage)).WasFailure)
        {
            return result;
        }

        var regEx = new Regex(regExPattern);

        var newValArray = regEx.Matches(result.Value ?? string.Empty)
            .OfType<Match>()
            .Select(match => match.Value)
            .ToArray();

        return result with
        {
            Value = newValArray.Length <= 1
                ? (newValArray.FirstOrDefault() ?? string.Empty)
                : JsonSerializer.Serialize(newValArray)
        };
    }
}
