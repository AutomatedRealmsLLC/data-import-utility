using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Jace;

using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.TransformOperations;

/// <summary>
/// This class is used to calculate the value of a field based on the values of other fields.
/// </summary>
public class CalculateOperation : ValueTransformationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(CalculateOperation);

    /// <summary>
    /// The error _message when the calculation syntax provided could not be parsed.
    /// </summary>
    public const string InvalidFormatMessage = "The calculation format is invalid.";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Perform Calculation";

    /// <inheritdoc />
    [JsonIgnore]
    public override string ShortName => "Calculate";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Calculate the value of a field based on the values of other fields.";

    /// <summary>
    /// The number of decimal places to round the result to.
    /// </summary>
    public int DecimalPlaces { get; set; }

    /// <inheritdoc />
    public override Task<TransformationResult> Apply(TransformationResult result)
    {
        try
        {
            return Task.FromResult(result.Calculate(OperationDetail, DecimalPlaces));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                ex switch
                {
                    ParseException _ => result with { ErrorMessage = InvalidFormatMessage },
                    _ => result with { ErrorMessage = $"{ex.GetType()} - {ex.Message}" },
                });
        }
    }
}

/// <summary>
/// The extension methods for the <see cref="CalculateOperation" /> to be used with the scripting engine.
/// </summary>
public static class CalculateOperationExtensions
{
    private readonly static CalculationEngine _calculationEngine = new();
    /// <summary>
    /// Calculates the value of a field based on the values of other fields.
    /// </summary>
    /// <param name="result">The result of the previous transformation.</param>
    /// <param name="operationDetail">
    /// The operation detail that contains the calculation to perform.
    /// </param>
    /// <param name="decimalPlaces">
    /// The number of decimal places to round the result to.
    /// </param>
    /// <returns>The calculated value.</returns>
    /// <remarks>
    /// If the input value is null or empty, the placeholders for the operation detail will default
    /// to 0. This will also happen for any placeholders that are not found in the input value.
    /// </remarks>
    public static TransformationResult Calculate(this TransformationResult result, string? operationDetail, int decimalPlaces)
    {
        var resultValue = operationDetail;
        if (string.IsNullOrWhiteSpace(resultValue))
        {
            return result with { Value = string.Empty };
        }

        if (string.IsNullOrWhiteSpace(result.Value))
        {
            return result;
        }

        var valuesToUse = result.ResultValueAsArray();

        if (valuesToUse is not null)
        {
            // Replace the operation detail placeholders with the actual values
            foreach (var (index, value) in valuesToUse.Select((value, index) => (index, value)))
            {
                resultValue = resultValue.Replace($"${{{index}}}", value);
            }
        }

        // If there are any placeholders left, replace them with "0"
        if (resultValue.TryGetPlaceholderMatches(out var placeholderMatches))
        {
            foreach (Match match in placeholderMatches)
            {
                resultValue = resultValue.Replace(match.Value, "0");
            }
        }

        return result with
        {
            Value = Math.Round(
                _calculationEngine.Calculate(resultValue),
                decimalPlaces
            ).ToString($"F{decimalPlaces}")
        };
    }
}

