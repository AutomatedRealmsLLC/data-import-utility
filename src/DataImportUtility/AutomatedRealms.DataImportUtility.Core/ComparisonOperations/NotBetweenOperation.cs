using System.Globalization;
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is not between two other values.
/// </summary>
public class NotBetweenOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(NotBetweenOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Not between";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is not between two other values (inclusive of limits).";

    /// <summary>
    /// Gets or sets the order of this operation if it's part of an enumerated list of operations.
    /// </summary>
    public int EnumMemberOrder { get; set; }

    /// <summary>
    /// The rule providing the low limit (inclusive) for the comparison.
    /// </summary>
    public new MappingRuleBase? LowLimit { get; set; }

    /// <summary>
    /// The rule providing the high limit (inclusive) for the comparison.
    /// </summary>
    public new MappingRuleBase? HighLimit { get; set; }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is an ITransformationContext
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }
        if (this.LowLimit is null)
        {
            throw new InvalidOperationException($"{nameof(this.LowLimit)} must be set for {DisplayName} operation.");
        }
        if (this.HighLimit is null)
        {
            throw new InvalidOperationException($"{nameof(this.HighLimit)} must be set for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);
        var lowLimitResult = await this.LowLimit.Apply(contextResult);
        var highLimitResult = await this.HighLimit.Apply(contextResult);

        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }
        if (lowLimitResult == null || lowLimitResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(this.LowLimit)} for {DisplayName} operation: {lowLimitResult?.ErrorMessage ?? "Result was null."}");
        }
        if (highLimitResult == null || highLimitResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(this.HighLimit)} for {DisplayName} operation: {highLimitResult?.ErrorMessage ?? "Result was null."}");
        }

        return !IsBetweenInternal(leftResult.CurrentValue, lowLimitResult.CurrentValue, highLimitResult.CurrentValue, contextResult.TargetFieldType);
    }

    private static bool IsBetweenInternal(object? value, object? lowLimit, object? highLimit, Type? targetFieldType)
    {
        if (value == null || lowLimit == null || highLimit == null)
        {
            return false;
        }

        // Try DateTime
        if (value is DateTime || lowLimit is DateTime || highLimit is DateTime || targetFieldType == typeof(DateTime))
        {
            if (TryParseDateTime(value, out var valDt) &&
                TryParseDateTime(lowLimit, out var lowDt) &&
                TryParseDateTime(highLimit, out var highDt))
            {
                return valDt >= lowDt && valDt <= highDt;
            }
            return false;
        }

        // Try Numeric (double for flexibility, could refine to long/decimal if needed)
        if (value is IConvertible && lowLimit is IConvertible && highLimit is IConvertible)
        {
            if (TryParseDouble(value, out var valNum) &&
                TryParseDouble(lowLimit, out var lowNum) &&
                TryParseDouble(highLimit, out var highNum))
            {
                return valNum >= lowNum && valNum <= highNum;
            }
        }

        // Fallback to string comparison
        var valStr = value.ToString();
        var lowStr = lowLimit.ToString();
        var highStr = highLimit.ToString();

        if (valStr != null && lowStr != null && highStr != null)
        {
            if (string.Compare(lowStr, highStr, StringComparison.Ordinal) > 0)
            {
                return string.Compare(valStr, lowStr, StringComparison.Ordinal) >= 0 &&
                       string.Compare(valStr, highStr, StringComparison.Ordinal) <= 0;
            }
            return string.Compare(valStr, lowStr, StringComparison.Ordinal) >= 0 &&
                   string.Compare(valStr, highStr, StringComparison.Ordinal) <= 0;
        }

        return false;
    }

    private static bool TryParseDateTime(object? obj, out DateTime result)
    {
        result = default;
        if (obj == null) return false;
        if (obj is DateTime dt)
        {
            result = dt;
            return true;
        }
        return DateTime.TryParse(obj.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out result);
    }

    private static bool TryParseDouble(object? obj, out double result)
    {
        result = default;
        if (obj == null) return false;
        if (obj is double d) { result = d; return true; }
        if (obj is IConvertible convertible)
        {
            try { result = convertible.ToDouble(CultureInfo.InvariantCulture); return true; }
            catch { }
        }
        return double.TryParse(obj.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return new NotBetweenOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            LowLimit = this.LowLimit?.Clone(),
            HighLimit = this.HighLimit?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
    }
}
