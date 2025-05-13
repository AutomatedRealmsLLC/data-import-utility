using System;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

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

    // EnumMemberName and EnumMemberOrder removed

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

        if (this.LeftOperand is null)
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand (value to check) must be provided for {TypeIdString}.");
        }
        if (this.RightOperand is null) // This is base.RightOperand, used as LowLimit
        {
            throw new ArgumentNullException(nameof(rightOperand), $"Right operand (low limit) must be provided for {TypeIdString}.");
        }
        if (this.HighLimit is null) // Changed from this.SecondaryRightOperand
        {
            throw new ArgumentNullException(nameof(secondaryRightOperand), $"Secondary right operand (high limit) must be provided for {TypeIdString}.");
        }
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        // Properties are inherited from ComparisonOperationBase
        if (this.LeftOperand is null || this.RightOperand is null || this.HighLimit is null) // Changed from this.SecondaryRightOperand
        {
            throw new InvalidOperationException($"All operands (value, low limit, high limit) must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }

        var leftResult = await this.LeftOperand.Apply(contextResult);
        var lowLimitResult = await this.RightOperand.Apply(contextResult); // RightOperand is LowLimit
        var highLimitResult = await this.HighLimit.Apply(contextResult); // Changed from this.SecondaryRightOperand

        if (leftResult == null)
        {
            throw new InvalidOperationException($"Applying {nameof(this.LeftOperand)} for {DisplayName} operation returned a null TransformationResult.");
        }
        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(this.LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        if (lowLimitResult == null)
        {
            throw new InvalidOperationException($"Applying low limit operand ({nameof(this.RightOperand)}) for {DisplayName} operation returned a null TransformationResult.");
        }
        if (lowLimitResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate low limit operand ({nameof(this.RightOperand)}) for {DisplayName} operation: {lowLimitResult.ErrorMessage}");
        }

        if (highLimitResult == null)
        {
            throw new InvalidOperationException($"Applying high limit operand ({nameof(this.HighLimit)}) for {DisplayName} operation returned a null TransformationResult."); // Changed from SecondaryRightOperand
        }
        if (highLimitResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate high limit operand ({nameof(this.HighLimit)}) for {DisplayName} operation: {highLimitResult.ErrorMessage}"); // Changed from SecondaryRightOperand
        }

        return !IsBetweenInternal(leftResult.CurrentValue, lowLimitResult.CurrentValue, highLimitResult.CurrentValue);
    }

    private static bool IsBetweenInternal(object? value, object? lowLimit, object? highLimit)
    {
        if (value == null || lowLimit == null || highLimit == null)
        {
            return false;
        }

        // Try numeric comparison
        if (IsNumeric(value) && IsNumeric(lowLimit) && IsNumeric(highLimit))
        {
            try
            {
                decimal valDecimal = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                decimal lowDecimal = Convert.ToDecimal(lowLimit, CultureInfo.InvariantCulture);
                decimal highDecimal = Convert.ToDecimal(highLimit, CultureInfo.InvariantCulture);
                if (lowDecimal > highDecimal) { decimal temp = lowDecimal; lowDecimal = highDecimal; highDecimal = temp; }
                return valDecimal >= lowDecimal && valDecimal <= highDecimal;
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                throw new InvalidOperationException($"Numeric comparison in {nameof(IsBetweenInternal)} failed. Value: '{value}', Low: '{lowLimit}', High: '{highLimit}'.", ex);
            }
        }

        // Try DateTime comparison
        if (CanConvertToDateTime(value, out DateTime valDt) &&
            CanConvertToDateTime(lowLimit, out DateTime lowDt) &&
            CanConvertToDateTime(highLimit, out DateTime highDt))
        {
            if (lowDt > highDt) { DateTime temp = lowDt; lowDt = highDt; highDt = temp; }
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

    private static bool IsNumeric(object? value)
    {
        if (value == null) return false;
        return value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint ||
               value is long || value is ulong || value is float || value is double || value is decimal;
    }

    private static bool CanConvertToDateTime(object? obj, out DateTime result)
    {
        result = default;
        if (obj == null) return false;
        if (obj is DateTime dt)
        {
            result = dt;
            return true;
        }
        if (obj is DateTimeOffset dto)
        {
            result = dto.UtcDateTime;
            return true;
        }
        var stringValue = obj.ToString();
        if (string.IsNullOrEmpty(stringValue)) return false;
        return DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out result);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return (NotBetweenOperation)base.Clone();
    }
}
