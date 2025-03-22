using System.Text.Json.Serialization;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.ValueTransformations;

/// <summary>
/// A transformation operation that combines the extracted values into a single value.
/// </summary>
public class InterpolateTransformation : ValueTransformationBase
{
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
    public override Task<TransformationResult> Apply(TransformationResult result)
    {
        try
        {
            return Task.FromResult(result.Interpolate(TransformationDetail));
        }
        catch (Exception ex)
        {
            return Task.FromResult(result with { ErrorMessage = ex.Message });
        }
    }

    /// <inheritdoc />
    public override string GenerateSyntax()
    {
        // Since we are using a custom syntax, we don't need to generate anything
        return TransformationDetail ?? string.Empty;
    }
}

/// <summary>
/// The extension methods for the <see cref="InterpolateTransformation" /> to be used with the scripting engine.
/// </summary>
public static class InterpolateTransformationExtensions
{
    /// <summary>
    /// Interpolates the value.
    /// </summary>
    /// <param name="result">The result of the previous transformation.</param>
    /// <param name="interpolateDetail">The interpolation pattern to use.</param>
    /// <returns>
    /// The result of the transformation.
    /// </returns>
    /// <remarks>
    /// Use syntax like `${0}` to interpolate the values that are the result of a previous transformation operation.
    /// </remarks>
    public static TransformationResult Interpolate(this TransformationResult result, string? interpolateDetail)
    {
        var resultValue = interpolateDetail;
        if (string.IsNullOrWhiteSpace(resultValue) || result.IsNullOrNullValue())
        {
            // No transformation to apply, so just return the input interpolation pattern
            return result with { Value = resultValue, CurrentValueType = typeof(string) };
        }
        var valuesToUse = result.ResultValueAsArray();

        if (valuesToUse is null)
        {
            return result with { Value = resultValue, CurrentValueType = typeof(string) };
        }

        // Apply the interpolation
        // Since we aren't using the normal format string syntax (e.g. {0}), we need to do this manually
        // We are using the syntax ${0} to indicate the value to interpolate
        for (var i = 0; i < valuesToUse.Length; i++)
        {
            resultValue = resultValue.Replace($"${{{i}}}", valuesToUse[i] ?? string.Empty);
        }

        return result with { Value = resultValue, CurrentValueType = typeof(string) };
    }
}