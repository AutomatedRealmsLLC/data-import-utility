using System.Globalization;
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is greater than another value.
/// </summary>
public class GreaterThanOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.GreaterThan";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Greater than";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is greater than another value.";

    /// <summary>
    /// Initializes a new instance of the <see cref="GreaterThanOperation"/> class.
    /// </summary>
    public GreaterThanOperation() : base(TypeIdString) // Pass TypeIdString to base constructor
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(
        MappingRuleBase leftOperand,
        MappingRuleBase? rightOperand,
        MappingRuleBase? secondaryRightOperand)
    {
        base.ConfigureOperands(leftOperand, rightOperand, secondaryRightOperand); // Calls base to set LeftOperand and RightOperand

        if (this.LeftOperand is null) // Validation after base call
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand must be provided for {TypeIdString}.");
        }
        if (this.RightOperand is null) // Validation after base call
        {
            // For GreaterThan, RightOperand is the value to compare against (like LowLimit in base, but conceptually the primary comparison value)
            throw new ArgumentNullException(nameof(rightOperand), $"Right operand (value to compare against) must be provided for {TypeIdString}.");
        }
        // secondaryRightOperand is not used by GreaterThanOperation.
        // Base ConfigureOperands sets HighLimit to secondaryRightOperand, which is fine as it won't be used.
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null || RightOperand is null) // Should be caught by ConfigureOperands
        {
            throw new InvalidOperationException($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }

        TransformationResult? leftOpResult = await LeftOperand.Apply(contextResult);
        if (leftOpResult is null || leftOpResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftOpResult?.ErrorMessage ?? "Result was null."}");
        }

        TransformationResult? rightOpResult = await RightOperand.Apply(contextResult);
        if (rightOpResult is null || rightOpResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightOpResult?.ErrorMessage ?? "Result was null."}");
        }

        return leftOpResult.GreaterThan(rightOpResult); // Extension method on TransformationResult
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        // Base Clone() handles LeftOperand, RightOperand and MemberwiseClone.
        return (GreaterThanOperation)base.Clone();
    }
}

/// <summary>
/// Extension methods for the GreaterThan operation.
/// </summary>
public static class GreaterThanOperationExtensions
{
    private static bool IsNumeric(object? value)
    {
        if (value == null) return false;
        return value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint ||
               value is long || value is ulong || value is float || value is double || value is decimal;
    }

    private static bool CanConvertToDateTime(object? value, out DateTime result)
    {
        result = default;
        if (value == null) return false;
        if (value is DateTime dt)
        {
            result = dt;
            return true;
        }
        if (value is DateTimeOffset dto)
        {
            result = dto.UtcDateTime;
            return true;
        }
        var stringValue = value.ToString();
        if (string.IsNullOrEmpty(stringValue)) return false;
        return DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out result);
    }

    /// <summary>
    /// Checks if the left result's current value is greater than the right result's current value.
    /// </summary>
    /// <param name="leftResult">The transformation result of the left operand.</param>
    /// <param name="rightResult">The transformation result of the right operand.</param>
    /// <returns>True if the left value is greater than the right value; otherwise, false.</returns>
    public static bool GreaterThan(this TransformationResult leftResult, TransformationResult rightResult)
    {
        object? leftVal = leftResult.CurrentValue;
        object? rightVal = rightResult.CurrentValue;

        if (leftVal == null || rightVal == null) { return false; }

        if (IsNumeric(leftVal) && IsNumeric(rightVal))
        {
            try
            {
                decimal dLeft = Convert.ToDecimal(leftVal, CultureInfo.InvariantCulture);
                decimal dRight = Convert.ToDecimal(rightVal, CultureInfo.InvariantCulture);
                return dLeft > dRight;
            }
            catch (OverflowException)
            {
                try
                {
                    double dblLeft = Convert.ToDouble(leftVal, CultureInfo.InvariantCulture);
                    double dblRight = Convert.ToDouble(rightVal, CultureInfo.InvariantCulture);
                    return dblLeft > dblRight;
                }
                catch { /* Fall through */ }
            }
            catch (FormatException) { /* Fall through */ }
        }

        if (CanConvertToDateTime(leftVal, out var leftDate) && CanConvertToDateTime(rightVal, out var rightDate))
        {
            return leftDate > rightDate;
        }

        var sLeft = leftVal.ToString();
        var sRight = rightVal.ToString();
        if (sLeft == null || sRight == null) return false;
        return string.Compare(sLeft, sRight, StringComparison.OrdinalIgnoreCase) > 0;
    }
}
