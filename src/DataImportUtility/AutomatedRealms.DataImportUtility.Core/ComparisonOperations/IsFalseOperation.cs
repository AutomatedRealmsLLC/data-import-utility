using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is false.
/// </summary>
public class IsFalseOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(IsFalseOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is false";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is false.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);

        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }

        return leftResult.IsFalse();
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return new IsFalseOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone()
        };
    }
}

/// <summary>
/// Extension methods for the IsFalse operation.
/// </summary>
public static class IsFalseOperationExtensions
{
    /// <summary>
    /// Checks if the result's current value is considered false.
    /// </summary>
    /// <param name="result">The transformation result to check.</param>
    /// <returns>True if the result's current value is false; otherwise, false.</returns>
    /// <remarks>
    /// A value is considered false if it is null, an empty string,
    /// a boolean false, the number 0, or the string "false" (case-insensitive).
    /// </remarks>
    public static bool IsFalse(this TransformationResult result)
    {
        if (result.CurrentValue == null) return true;
        if (result.CurrentValue is bool boolVal) return !boolVal;
        
        var stringVal = result.CurrentValue.ToString();
        if (string.IsNullOrEmpty(stringVal)) return true;
        if (string.Equals(stringVal, "false", StringComparison.OrdinalIgnoreCase)) return true;
        if (stringVal == "0") return true;
        if (result.CurrentValue is IConvertible convertible) {
            try {
                return convertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture) == 0;
            } catch {
                // Not a number, or not convertible to double
            }
        }

        return false;
    }
}
