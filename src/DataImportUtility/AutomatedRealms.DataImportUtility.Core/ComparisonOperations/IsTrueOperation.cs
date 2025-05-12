using System.Globalization; // Added for CultureInfo.InvariantCulture
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Added for TransformationResult

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is true.
/// </summary>
public class IsTrueOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(IsTrueOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is true";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is true.";

    /// <summary>
    /// Gets or sets the order of this operation if it's part of an enumerated list of operations.
    /// </summary>
    public int EnumMemberOrder { get; set; }

    /// <summary>
    /// Evaluates whether the left operand's value is considered true.
    /// A value is considered true if it's a boolean true, a non-zero number, or the string "true" (case-insensitive).
    /// Null, empty/whitespace strings, boolean false, zero, or other string values are considered false.
    /// </summary>
    /// <param name="contextResult">The transformation context.</param>
    /// <returns>True if the value is considered true, otherwise false.</returns>
    public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is an ITransformationContext
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"LeftOperand must be set for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);

        if (leftResult == null)
        {
            throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned null unexpectedly.");
        }

        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        object? value = leftResult.CurrentValue;

        if (value is null)
        {
            return false; // Null is not true
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false; // Empty or whitespace string is not true
            }
            if (bool.TryParse(stringValue, out var parsedBool))
            {
                return parsedBool;
            }
            // Check for "true" case-insensitively, though bool.TryParse usually handles this.
            // Adding explicit check for "true" for robustness if TryParse behavior varies or for clarity.
            if (string.Equals(stringValue, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            // Check for numeric interpretations
            if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var numberValue))
            {
                return numberValue != 0;
            }
            return false; // String is not recognizably true
        }

        // Handle numeric types directly (e.g., int, double, decimal)
        if (value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal)
        {
            // Convert to double for a common check against zero.
            // This avoids a complex switch or multiple if-else statements for each numeric type.
            // Using Convert.ToDouble for simplicity, assuming standard numeric conversions.
            try
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture) != 0;
            }
            catch (OverflowException)
            {
                // If the number is too large to fit in a double but is non-zero, it's true.
                // This case is rare for typical data but included for completeness.
                // A more robust check might involve type-specific comparisons if precision is critical.
                return true; // Or handle based on specific requirements for extreme values.
            }
            catch (InvalidCastException)
            {
                // Should not happen if type checks are correct, but as a fallback.
                return false;
            }
        }

        // For any other type, it's not considered true in this context.
        return false;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return new IsTrueOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
    }
}
