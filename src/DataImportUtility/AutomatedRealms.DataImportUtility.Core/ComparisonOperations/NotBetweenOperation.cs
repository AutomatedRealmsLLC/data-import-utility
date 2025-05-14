using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Globalization;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is not between two other values.
/// </summary>
public class NotBetweenOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.NotBetween";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Not between";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is not between two other values (inclusive of limits).";

    /// <summary>
    /// Initializes a new instance of the <see cref="NotBetweenOperation"/> class.
    /// </summary>
    public NotBetweenOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(
        MappingRuleBase leftOperand,
        MappingRuleBase? rightOperand, // This will be LowLimit (maps to base.RightOperand)
        MappingRuleBase? secondaryRightOperand) // This will be HighLimit (maps to base.HighLimit)
    {
        base.ConfigureOperands(leftOperand, rightOperand, secondaryRightOperand);

        if (LeftOperand is null)
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand (value to check) must be provided for {TypeIdString}.");
        }
        if (RightOperand is null) // This is base.RightOperand, used as LowLimit
        {
            throw new ArgumentNullException(nameof(rightOperand), $"Right operand (low limit) must be provided for {TypeIdString}.");
        }
        if (HighLimit is null) // Changed from SecondaryRightOperand
        {
            throw new ArgumentNullException(nameof(secondaryRightOperand), $"Secondary right operand (high limit) must be provided for {TypeIdString}.");
        }
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        // Properties are inherited from ComparisonOperationBase
        if (LeftOperand is null || RightOperand is null || HighLimit is null) // Changed from SecondaryRightOperand
        {
            throw new InvalidOperationException($"All operands (value, low limit, high limit) must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);
        var lowLimitResult = await RightOperand.Apply(contextResult); // RightOperand is LowLimit
        var highLimitResult = await HighLimit.Apply(contextResult); // Changed from SecondaryRightOperand

        if (leftResult is null)
        {
            throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned a null TransformationResult.");
        }
        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        if (lowLimitResult is null)
        {
            throw new InvalidOperationException($"Applying low limit operand ({nameof(RightOperand)}) for {DisplayName} operation returned a null TransformationResult.");
        }
        if (lowLimitResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate low limit operand ({nameof(RightOperand)}) for {DisplayName} operation: {lowLimitResult.ErrorMessage}");
        }

        if (highLimitResult is null)
        {
            throw new InvalidOperationException($"Applying high limit operand ({nameof(HighLimit)}) for {DisplayName} operation returned a null TransformationResult."); // Changed from SecondaryRightOperand
        }
        if (highLimitResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate high limit operand ({nameof(HighLimit)}) for {DisplayName} operation: {highLimitResult.ErrorMessage}"); // Changed from SecondaryRightOperand
        }

        return !IsBetweenInternal(leftResult.CurrentValue, lowLimitResult.CurrentValue, highLimitResult.CurrentValue);
    }

    private static bool IsBetweenInternal(object? value, object? lowLimit, object? highLimit)
    {
        if (value is null || lowLimit is null || highLimit is null)
        {
            return false;
        }

        // Try numeric comparison
        if (value.IsNumericType() && lowLimit.IsNumericType() && highLimit.IsNumericType())
        {
            try
            {
                var valDecimal = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                var lowDecimal = Convert.ToDecimal(lowLimit, CultureInfo.InvariantCulture);
                var highDecimal = Convert.ToDecimal(highLimit, CultureInfo.InvariantCulture);
                if (lowDecimal > highDecimal)
                {
                    (highDecimal, lowDecimal) = (lowDecimal, highDecimal);
                }
                return valDecimal >= lowDecimal && valDecimal <= highDecimal;
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                throw new InvalidOperationException($"Numeric comparison in {nameof(IsBetweenInternal)} failed. Value: '{value}', Low: '{lowLimit}', High: '{highLimit}'.", ex);
            }
        }

        // Try DateTime comparison
        if (value.CanConvertToDateTime(out DateTime valDt) &&
            lowLimit.CanConvertToDateTime(out DateTime lowDt) &&
            highLimit.CanConvertToDateTime(out DateTime highDt))
        {
            if (lowDt > highDt)
            {
                (highDt, lowDt) = (lowDt, highDt);
            }
            return valDt >= lowDt && valDt <= highDt;
        }

        // Fallback to string comparison
        var valStr = Convert.ToString(value, CultureInfo.InvariantCulture);
        var lowStr = Convert.ToString(lowLimit, CultureInfo.InvariantCulture);
        var highStr = Convert.ToString(highLimit, CultureInfo.InvariantCulture);

        if (string.Compare(lowStr, highStr, StringComparison.Ordinal) > 0)
        {
            (lowStr, highStr) = (highStr, lowStr);
        }
        return string.Compare(valStr, lowStr, StringComparison.Ordinal) >= 0 &&
               string.Compare(valStr, highStr, StringComparison.Ordinal) <= 0;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return (NotBetweenOperation)base.Clone();
    }
}
