using System.Collections; // Added for IEnumerable
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers; // Added for CultureInfo
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult
using AutomatedRealms.DataImportUtility.Core.Compatibility; // For MathCompatibility

using Jace;

namespace AutomatedRealms.DataImportUtility.Core.ValueTransformations;

/// <summary>
/// This class is used to calculate the value of a field based on the values of other fields.
/// </summary>
public class CalculateTransformation : ValueTransformationBase
{
    /// <summary>
    /// Static TypeId for this transformation.
    /// </summary>
    public static readonly string TypeIdString = "Core.CalculateTransformation";

    private static readonly CalculationEngine _calculationEngine = new(CultureInfo.InvariantCulture);

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

    /// <summary>
    /// Initializes a new instance of the <see cref="CalculateTransformation"/> class.
    /// </summary>
    public CalculateTransformation() : base(TypeIdString) { }

    /// <inheritdoc />
    public override Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult)
    {
        try
        {
            string? resultValueText = TransformationDetail;
            if (string.IsNullOrWhiteSpace(resultValueText))
            {
                return Task.FromResult(TransformationResult.Success(
                    originalValue: previousResult.OriginalValue,
                    originalValueType: previousResult.OriginalValueType,
                    currentValue: string.Empty,
                    currentValueType: typeof(decimal),
                    appliedTransformations: previousResult.AppliedTransformations,
                    record: previousResult.Record,
                    tableDefinition: previousResult.TableDefinition,
                    sourceRecordContext: previousResult.SourceRecordContext,
                    targetFieldType: previousResult.TargetFieldType
                ));
            }

            string[] valuesToUse;
            if (previousResult.CurrentValue == null)
            {
                valuesToUse = [];
            }
            else if (previousResult.CurrentValue is IEnumerable<string> stringEnumerable)
            {
                valuesToUse = [.. stringEnumerable];
            }
            else if (previousResult.CurrentValue is IEnumerable enumerableValue && !(previousResult.CurrentValue is string))
            {
                valuesToUse = [.. enumerableValue.Cast<object>().Select(o => o?.ToString() ?? "0")];
            }
            else // Single object (including string if not caught by IEnumerable<string>)
            {
                valuesToUse = [previousResult.CurrentValue.ToString() ?? "0"];
            }

            // resultValueText is guaranteed non-null here due to the earlier IsNullOrWhiteSpace check.
            string currentFormula = resultValueText!;
            if (valuesToUse.Any())
            {
                for (int i = 0; i < valuesToUse.Length; i++)
                {
                    string valToReplace = valuesToUse[i] ?? "0"; // Default nulls to "0"
                    currentFormula = currentFormula.Replace($"${{{i}}}", valToReplace);
                }
            }

            if (StringHelpers.TryGetPlaceholderMatches(currentFormula, out var placeholderMatches))
            {
                foreach (Match match in placeholderMatches.Cast<Match>())
                {
                    currentFormula = currentFormula.Replace(match.Value, "0");
                }
            }

            int clampedDecimalPlaces = DecimalPlaces;
#if !NETCOREAPP2_0_OR_GREATER
            clampedDecimalPlaces = MathCompatibility.Clamp(-1, clampedDecimalPlaces, 15);
#else
            clampedDecimalPlaces = Math.Clamp(clampedDecimalPlaces, -1, 15);
#endif

            double calculatedVal = _calculationEngine.Calculate(currentFormula);
            if (clampedDecimalPlaces >= 0)
            {
                calculatedVal = Math.Round(calculatedVal, clampedDecimalPlaces, MidpointRounding.AwayFromZero);
            }

            string finalValueString = calculatedVal.ToString(clampedDecimalPlaces >= 0 ? $"F{clampedDecimalPlaces}" : "G", CultureInfo.InvariantCulture);

            return Task.FromResult(TransformationResult.Success(
                originalValue: previousResult.OriginalValue,
                originalValueType: previousResult.OriginalValueType,
                currentValue: finalValueString,
                currentValueType: typeof(decimal),
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                targetFieldType: previousResult.TargetFieldType
            ));
        }
        catch (Exception ex)
        {
            string errorMessage = ex switch
            {
                ParseException _ => InvalidFormatMessage,
                _ => $"{ex.GetType().FullName} - {ex.Message}"
            };
            return Task.FromResult(TransformationResult.Failure(
                originalValue: previousResult.OriginalValue,
                targetType: previousResult.TargetFieldType, // Using TargetFieldType from context
                errorMessage: errorMessage,
                originalValueType: previousResult.OriginalValueType,
                currentValueType: previousResult.CurrentValueType, // Preserve current type info if any, or it will be null
                appliedTransformations: previousResult.AppliedTransformations,
                record: previousResult.Record,
                tableDefinition: previousResult.TableDefinition,
                sourceRecordContext: previousResult.SourceRecordContext,
                explicitTargetFieldType: previousResult.TargetFieldType
            ));
        }
    }

    /// <inheritdoc />
    public override async Task<TransformationResult> Transform(object? value, Type targetType) // targetType is not used here, but part of base signature
    {
        // Create an initial TransformationResult where 'value' is both original and current.
        // Context is null as this is the start of a transformation sequence for this specific call.
        var initialContext = TransformationResult.Success(
            originalValue: value,
            originalValueType: value?.GetType() ?? typeof(object),
            currentValue: value,
            currentValueType: value?.GetType() ?? typeof(object),
            appliedTransformations: new List<string>(),
            record: null, // No DataRow context for an isolated Transform call
            tableDefinition: null, // No TableDefinition context
            sourceRecordContext: null, // No SourceRecordContext
            targetFieldType: targetType // targetType is the intended target type for this transformation
        );
        return await ApplyTransformationAsync(initialContext);
    }

    /// <inheritdoc />
    public override ValueTransformationBase Clone()
    {
        var clone = new CalculateTransformation
        {
            DecimalPlaces = this.DecimalPlaces
        };
        // Clones TransformationDetail, TypeId
        this.CloneBaseProperties(clone);
        return clone;
    }
}
