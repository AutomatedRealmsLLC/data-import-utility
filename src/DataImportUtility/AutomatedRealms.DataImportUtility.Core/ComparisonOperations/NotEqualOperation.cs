using System.Globalization; // For CultureInfo and NumberStyles
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult and MappingRuleBase

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if two values are not equal.
/// </summary>
public class NotEqualOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.NotEqual";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Does not equal";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if two values are not equal.";

    /// <summary>
    /// Initializes a new instance of the <see cref="NotEqualOperation"/> class.
    /// </summary>
    public NotEqualOperation() : base(TypeIdString)
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
            throw new ArgumentNullException(nameof(rightOperand), $"Right operand must be provided for {TypeIdString}.");
        }
        // secondaryRightOperand is not used by NotEqualOperation, base class handles it (sets HighLimit which is fine)
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null) // Should be caught by ConfigureOperands, but defensive check.
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }
        if (RightOperand is null) // Should be caught by ConfigureOperands, but defensive check.
        {
            throw new InvalidOperationException($"{nameof(RightOperand)} must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);
        var rightResult = await RightOperand.Apply(contextResult);

        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }
        if (rightResult == null || rightResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult?.ErrorMessage ?? "Result was null."}");
        }

        return !AreValuesEqual(leftResult.CurrentValue, rightResult.CurrentValue, contextResult.TargetFieldType);
    }

    private static bool AreValuesEqual(object? leftValue, object? rightValue, Type? targetFieldType)
    {
        if (leftValue == null && rightValue == null)
        {
            return true;
        }
        if (leftValue == null || rightValue == null)
        {
            return false;
        }

        // Attempt DateTime comparison first if applicable
        if (targetFieldType == typeof(DateTime) || leftValue is DateTime || rightValue is DateTime)
        {
            if (TryParseDateTime(leftValue, out var leftDate) && TryParseDateTime(rightValue, out var rightDate))
            {
                return leftDate.Equals(rightDate);
            }
            return false;
        }

        // Attempt numeric comparison
        if (IsNumericType(leftValue) && IsNumericType(rightValue))
        {
            if (TryParseDecimal(leftValue, out var leftDecimal) && TryParseDecimal(rightValue, out var rightDecimal))
            {
                return leftDecimal == rightDecimal;
            }
            if (TryParseDouble(leftValue, out var leftDouble) && TryParseDouble(rightValue, out var rightDouble))
            {
                return leftDouble.Equals(rightDouble);
            }
            return false;
        }

        if ((IsNumericType(leftValue) && rightValue is string) || (leftValue is string && IsNumericType(rightValue)))
        {
            return false;
        }

        return string.Equals(leftValue.ToString(), rightValue.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNumericType(object? value)
    {
        if (value == null) return false;
        return value is sbyte
            || value is byte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong
            || value is float
            || value is double
            || value is decimal;
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
            catch { /* Fall through */ }
        }
        return double.TryParse(obj.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseDecimal(object? obj, out decimal result)
    {
        result = default;
        if (obj == null) return false;
        if (obj is decimal dec) { result = dec; return true; }
        if (obj is IConvertible convertible)
        {
            try { result = convertible.ToDecimal(CultureInfo.InvariantCulture); return true; }
            catch { /* Fall through */ }
        }
        return decimal.TryParse(obj.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        // Base clone handles LeftOperand and RightOperand.
        // No NotEqualOperation-specific properties to clone after removing EnumMemberOrder.
        return (NotEqualOperation)base.Clone();
    }
}

// The NotEqualOperationExtensions class is no longer needed as its logic is integrated.
