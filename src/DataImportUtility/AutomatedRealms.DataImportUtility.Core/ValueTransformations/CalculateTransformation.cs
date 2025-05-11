using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult
using AutomatedRealms.DataImportUtility.Core.Helpers;     // For StringHelpers.TryGetPlaceholderMatches and TransformationResultHelpers.ResultValueAsArray
using AutomatedRealms.DataImportUtility.Core.Compatibility; // For MathCompatibility
using Jace;
using System; // For Math.Clamp, Exception, Task, ArgumentException, Type
using System.Linq; // For .Select
using System.Threading.Tasks; // For Task

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// This class is used to calculate the value of a field based on the values of other fields.
/// </summary>
public class CalculateTransformation : ValueTransformationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(CalculateTransformation);

    /// <summary>
    /// The error message when the calculation syntax provided could not be parsed.
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
    [JsonIgnore]
    public override bool IsEmpty => string.IsNullOrWhiteSpace(TransformationDetail);

    /// <inheritdoc />
    [JsonIgnore]
    public override Type OutputType => typeof(decimal);

    /// <inheritdoc />
    public override async Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult)
    {
        try
        {
            // The Calculate extension method modifies the passed-in result and returns it.
            TransformationResult calculatedResult = previousResult.Calculate(TransformationDetail, DecimalPlaces);
            return await Task.FromResult(calculatedResult); // Keep async nature if Calculate becomes async later
        }
        catch (Exception ex)
        {
            return previousResult with
            {
                ErrorMessage = ex switch
                {
                    ParseException _ => InvalidFormatMessage, // Jace.ParseException
                    _ => $"{ex.GetType().FullName} - {ex.Message}" // Use FullName for clarity
                },
                CurrentValueType = previousResult.CurrentValueType, // Preserve type on error
                CurrentValue = previousResult.CurrentValue // Preserve value on error
            };
        }
    }

    /// <inheritdoc />
    public override Task<TransformationResult> Transform(object? value, Type targetType)
    {
        var initialResult = TransformationResult.Success(
            originalValue: value,
            originalValueType: value?.GetType() ?? typeof(object),
            currentValue: value, // Initial value before transformation
            currentValueType: value?.GetType() ?? typeof(object)
        );

        try
        {
            // The Calculate extension method (defined below) modifies the passed-in result and returns it.
            TransformationResult calculatedResult = initialResult.Calculate(TransformationDetail, DecimalPlaces);
            return Task.FromResult(calculatedResult);
        }
        catch (Exception ex)
        {
            return Task.FromResult(initialResult with
            {
                ErrorMessage = ex switch
                {
                    ParseException _ => InvalidFormatMessage, // Jace.ParseException
                    _ => $"{ex.GetType().FullName} - {ex.Message}" // Use FullName for clarity
                }
            });
        }
    }
}

/// <summary>
/// The extension methods for the <see cref="CalculateTransformation" /> to be used with the scripting engine.
/// </summary>
public static class CalculateOperationExtensions
{
    private readonly static CalculationEngine _calculationEngine = new();
    /// <summary>
    /// Calculates the value of a field based on the values of other fields.
    /// </summary>
    /// <param name="result">The result of the previous transformation.</param>
    /// <param name="transformationDetail">
    /// The transformation detail that contains the calculation to perform.
    /// </param>
    /// <param name="decimalPlaces">
    /// The number of decimal places to round the result to.
    /// </param>
    /// <returns>The calculated value.</returns>
    /// <remarks>
    /// This value must be between -1 and 15 for the <see cref="Math.Round(double)"/> method. A
    /// value of -1 will not perform any rounding and use the value returned from the calculation
    /// engine.<br/>
    /// <br/>
    /// If the input value is null or empty, the placeholders for the transformation detail will default
    /// to 0. This will also happen for any placeholders that are not found in the input value.
    /// </remarks>
    public static TransformationResult Calculate(this TransformationResult result, string? transformationDetail, int decimalPlaces)
    {
        var resultValueText = transformationDetail;
        if (string.IsNullOrWhiteSpace(resultValueText))
        {
            return result with { CurrentValue = string.Empty, CurrentValueType = typeof(decimal) };
        }

        // Use CurrentValue from the result for calculations
        var currentInputValue = result.CurrentValue?.ToString();
        if (string.IsNullOrWhiteSpace(currentInputValue))
        {
            // If CurrentValue is null/empty, but we have a transformationDetail (formula),
            // we might still proceed if the formula doesn't rely on ${index} placeholders
            // or if those placeholders are intended to default to 0.
            // However, the existing logic for replacing ${index} relies on valuesToUse from CurrentValue.
            // For now, let's assume if CurrentValue is empty, we can't proceed with indexed placeholders.
            // If the formula is static (e.g. "2+2"), it could still work.
        }

        var valuesToUse = result.ResultValueAsArray(); // This helper likely uses CurrentValue

        if (valuesToUse is not null)
        {
            // Replace the transformation detail placeholders with the actual values
            foreach (var (index, val) in valuesToUse.Select((val, index) => (index, val)))
            {
                resultValueText = resultValueText!.Replace($"${{{index}}}", val);
            }
        }

        // If there are any placeholders left, replace them with "0"
        if (resultValueText.TryGetPlaceholderMatches(out var placeholderMatches)) // From Core.Helpers.StringHelpers
        {
            foreach (Match match in placeholderMatches)
            {
                resultValueText = resultValueText!.Replace(match.Value, "0");
            }
        }

#if !NETCOREAPP2_0_OR_GREATER
        decimalPlaces = MathCompatibility.Clamp(-1, decimalPlaces, 15); // From Core.Compatibility
#else
        decimalPlaces = Math.Clamp(-1, decimalPlaces, 15); // From System
#endif

        var calculatedVal = _calculationEngine.Calculate(resultValueText);
        if (decimalPlaces >= 0)
        {
            calculatedVal = Math.Round(
                calculatedVal,
                decimalPlaces
            );
        }

        return result with
        {
            CurrentValue = calculatedVal.ToString($"{(decimalPlaces >= 0 ? $"F{decimalPlaces}" : null)}"),
            CurrentValueType = typeof(decimal),
        };
    }
}
