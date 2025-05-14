using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is between two other values (inclusive).
/// </summary>
public class BetweenOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.Between";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Between";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is between two other values (inclusive).";

    /// <summary>
    /// Initializes a new instance of the <see cref="BetweenOperation"/> class.
    /// </summary>
    public BetweenOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // Signature matches base class
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {nameof(BetweenOperation)}.");
        }

        if (base.LowLimit is null || base.HighLimit is null)
        {
            throw new InvalidOperationException($"Base {nameof(LowLimit)} and base {nameof(HighLimit)} must be set for {nameof(BetweenOperation)}.");
        }

        var leftOperandActualResult = await LeftOperand.Apply(contextResult);
        if (leftOperandActualResult == null || leftOperandActualResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftOperandActualResult?.ErrorMessage ?? "Result was null."}");
        }

        var lowLimitActualResult = await base.LowLimit.Apply(contextResult);
        if (lowLimitActualResult == null || lowLimitActualResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LowLimit)} for {DisplayName} operation: {lowLimitActualResult?.ErrorMessage ?? "Result was null."}");
        }

        var highLimitActualResult = await base.HighLimit.Apply(contextResult);
        return highLimitActualResult == null || highLimitActualResult.WasFailure
            ? throw new InvalidOperationException($"Failed to evaluate {nameof(HighLimit)} for {DisplayName} operation: {highLimitActualResult?.ErrorMessage ?? "Result was null."}")
            : BetweenOperationExtensions.Between(leftOperandActualResult, lowLimitActualResult, highLimitActualResult);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = (BetweenOperation)base.Clone();
        return clone;
    }
}

/// <summary>
/// Extension methods for the Between operation.
/// </summary>
public static class BetweenOperationExtensions
{
    /// <summary>
    /// Checks if the left result is between the low and high limits (inclusive).
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="lowLimitInclusive">The low limit (inclusive).</param>
    /// <param name="highLimitInclusive">The high limit (inclusive).</param>
    /// <returns>True if the left result is between the low and high limits (inclusive); otherwise, false.</returns>
    public static bool Between(this TransformationResult leftResult, TransformationResult lowLimitInclusive, TransformationResult highLimitInclusive)
    {
        object? leftCurrentValue = leftResult.CurrentValue;
        object? lowCurrentValue = lowLimitInclusive.CurrentValue;
        object? highCurrentValue = highLimitInclusive.CurrentValue;

        if (leftCurrentValue is null) { return false; }

        if (lowCurrentValue is null || highCurrentValue is null) { return false; }

        // Attempt numeric comparison first
        if (leftCurrentValue.IsNumericType() || lowCurrentValue.IsNumericType() || highCurrentValue.IsNumericType())
        {
            try
            {
                var val = Convert.ToDecimal(leftCurrentValue);
                var low = Convert.ToDecimal(lowCurrentValue);
                var high = Convert.ToDecimal(highCurrentValue);
                return val >= low && val <= high;
            }
            catch (Exception) { /* Fall through */ }
        }

        // Attempt DateTime comparison
        if (leftCurrentValue is DateTime || lowCurrentValue is DateTime || highCurrentValue is DateTime)
        {
            try
            {
                var dateVal = Convert.ToDateTime(leftCurrentValue);
                var dateLow = Convert.ToDateTime(lowCurrentValue);
                var dateHigh = Convert.ToDateTime(highCurrentValue);
                return dateVal >= dateLow && dateVal <= dateHigh;
            }
            catch (Exception) { /* Fall through */ }
        }

        // Fallback to string comparison
        string? leftStr = leftCurrentValue.ToString();
        string? lowStr = lowCurrentValue.ToString();
        string? highStr = highCurrentValue.ToString();

        return leftStr is not null
            && string.Compare(leftStr, lowStr, StringComparison.Ordinal) >= 0
            && string.Compare(leftStr, highStr, StringComparison.Ordinal) <= 0;
    }
}
