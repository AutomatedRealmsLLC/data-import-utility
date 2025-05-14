using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if two values are equal. Handles type conversions for common types.
/// </summary>
public class EqualsOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.Equals";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Equals";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if two values are equal, attempting type conversion if necessary.";

    /// <summary>
    /// Initializes a new instance of the <see cref="EqualsOperation"/> class.
    /// </summary>
    public EqualsOperation() : base(TypeIdString) // Pass TypeIdString to base constructor
    {
        // Operands (LeftOperand, RightOperand) are set via properties.
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // Changed back to TransformationResult
    {
        if (LeftOperand is null || RightOperand is null)
        {
            // Consider logging contextResult.AddLogMessage("Error: Operands not set for EqualsOperation.");
            throw new InvalidOperationException($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set for {nameof(EqualsOperation)}.");
        }

        // contextResult is a TransformationResult, which implements ITransformationContext.
        // So it can be passed to Apply methods that expect ITransformationContext.

        var leftOperandActualResult = await LeftOperand.Apply(contextResult); // Pass contextResult (as ITransformationContext)
        if (leftOperandActualResult is null || leftOperandActualResult.WasFailure)
        {
            // Consider logging
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftOperandActualResult?.ErrorMessage ?? "Result was null."}");
        }

        var rightOperandActualResult = await RightOperand.Apply(contextResult); // Pass contextResult (as ITransformationContext)
        if (rightOperandActualResult is null || rightOperandActualResult.WasFailure)
        {
            // Consider logging
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightOperandActualResult?.ErrorMessage ?? "Result was null."}");
        }

        return EqualsOperationExtensions.IsEqualTo(leftOperandActualResult, rightOperandActualResult);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return (EqualsOperation)base.Clone();
    }
}

/// <summary>
/// Extension methods for the IsEqualTo operation.
/// </summary>
public static class EqualsOperationExtensions
{
    /// <summary>
    /// Checks if the current values of two transformation results are equal.
    /// Attempts type-aware comparison for common types (numeric, DateTime, bool, string).
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="rightResult">The result of the right operand.</param>
    /// <returns>True if the values are considered equal; otherwise, false.</returns>
    public static bool IsEqualTo(this TransformationResult leftResult, TransformationResult rightResult)
    {
        var leftValue = leftResult.CurrentValue;
        var rightValue = rightResult.CurrentValue;

        // If both are null, they are equal.
        if (leftValue is null && rightValue is null)
        {
            return true;
        }
        // If one is null and the other isn't, they are not equal.
        if (leftValue is null || rightValue is null)
        {
            return false;
        }

        // Direct type equality and object.Equals check first
        if (leftValue.GetType() == rightValue.GetType())
        {
            return Equals(leftValue, rightValue);
        }

        // Attempt to convert and compare common types
        try
        {
            // Numeric comparison (decimal is a good common ground)
            if (leftValue.IsNumericType() && rightValue.IsNumericType())
            {
                return Convert.ToDecimal(leftValue) == Convert.ToDecimal(rightValue);
            }

            // DateTime comparison
            if ((leftValue is DateTime ld || leftValue.CanConvertToDateTime(out ld))
                && (rightValue is DateTime rd || rightValue.CanConvertToDateTime(out rd)))
            {
                return ld == rd;
                // If one is DateTime and the other isn't convertible, they aren't equal in this typed sense.
            }

            // Boolean comparison
            var leftIsBoolConvertible = TryConvertToBool(leftValue, out bool leftBoolValue);
            var rightIsBoolConvertible = TryConvertToBool(rightValue, out bool rightBoolValue);

            if (leftIsBoolConvertible && rightIsBoolConvertible)
            {
                return leftBoolValue == rightBoolValue;
            }
            // If only one is bool convertible, or neither are (and they aren't covered by numeric/date), they aren't equal in this typed sense.

        }
        catch (Exception)
        {
            // If conversion fails, fall through to string comparison
        }

        // Fallback to case-insensitive string comparison
        return string.Equals(leftValue.ToString(), rightValue.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryConvertToBool(object? value, out bool result)
    {
        result = false;
        if (value is null) return false;

        if (value is bool boolVal)
        {
            result = boolVal;
            return true;
        }
        if (value.IsNumericType())
        {
            try
            {
                result = Convert.ToInt32(value) != 0;
                return true;
            }
            catch { return false; }
        }
        if (value is string strVal)
        {
            if (bool.TryParse(strVal, out result))
            {
                return true;
            }
            // Allow common string representations of true/false like "1"/"0", "yes"/"no"
            if (strVal.Equals("1", StringComparison.OrdinalIgnoreCase) || strVal.Equals("true", StringComparison.OrdinalIgnoreCase) || strVal.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }
            if (strVal.Equals("0", StringComparison.OrdinalIgnoreCase) || strVal.Equals("false", StringComparison.OrdinalIgnoreCase) || strVal.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }
        }
        return false;
    }
}
