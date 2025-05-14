using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Globalization;
using System.Text.Json.Serialization;

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

        if (LeftOperand is null) // Validation after base call
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand must be provided for {TypeIdString}.");
        }
        if (RightOperand is null) // Validation after base call
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

        if (leftResult is null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }
        return rightResult is null || rightResult.WasFailure
            ? throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult?.ErrorMessage ?? "Result was null."}")
            : !AreValuesEqual(leftResult.CurrentValue, rightResult.CurrentValue, contextResult.TargetFieldType);
    }

    private static bool AreValuesEqual(object? leftValue, object? rightValue, Type? targetFieldType)
    {
        if (leftValue is null && rightValue is null)
        {
            return true;
        }
        if (leftValue is null || rightValue is null)
        {
            return false;
        }

        // Attempt DateTime comparison first if applicable
        if (targetFieldType == typeof(DateTime) || leftValue is DateTime || rightValue is DateTime)
        {
            return TryParseDateTime(leftValue, out var leftDate)
                && TryParseDateTime(rightValue, out var rightDate)
                && leftDate.Equals(rightDate);
        }

        // Attempt numeric comparison
        if (leftValue.IsNumericType() && rightValue.IsNumericType())
        {
            if (TryParseDecimal(leftValue, out var leftDecimal) && TryParseDecimal(rightValue, out var rightDecimal))
            {
                return leftDecimal == rightDecimal;
            }
            return TryParseDouble(leftValue, out var leftDouble)
                && TryParseDouble(rightValue, out var rightDouble)
                && leftDouble.Equals(rightDouble);
        }

        return (!leftValue.IsNumericType() || rightValue is not string)
            && (leftValue is not string || !rightValue.IsNumericType())
            && string.Equals(leftValue.ToString(), rightValue.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseDateTime(object? obj, out DateTime result)
    {
        result = default;
        if (obj is null) return false;
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
        if (obj is null) return false;
        if (obj is double d) { result = d; return true; }
        if (obj is IConvertible convertible)
        {
            try
            {
                result = convertible.ToDouble(CultureInfo.InvariantCulture);
                return true;
            }
            catch { /* Fall through */ }
        }
        return double.TryParse(obj.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseDecimal(object? obj, out decimal result)
    {
        result = default;
        if (obj is null) return false;
        if (obj is decimal dec) { result = dec; return true; }
        if (obj is IConvertible convertible)
        {
            try
            {
                result = convertible.ToDecimal(CultureInfo.InvariantCulture);
                return true;
            }
            catch { /* Fall through */ }
        }
        return decimal.TryParse(obj.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
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
