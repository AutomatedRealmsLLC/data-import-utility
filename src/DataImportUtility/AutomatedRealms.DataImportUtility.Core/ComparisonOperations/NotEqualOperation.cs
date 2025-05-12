using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces; // For ITransformationContext
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using System.Globalization; // For CultureInfo and NumberStyles

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if two values are not equal.
/// </summary>
public class NotEqualOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(NotEqualOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Does not equal";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if two values are not equal.";

    /// <summary>
    /// Gets or sets the order of this operation if it's part of an enumerated list of operations.
    /// </summary>
    public int EnumMemberOrder { get; set; }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is an ITransformationContext
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }
        if (RightOperand is null)
        {
            // For NotEqual, if Left is something and Right is not specified, it's arguably not equal.
            // However, consistent with EqualsOperation, let's require both operands.
            throw new InvalidOperationException($"{nameof(RightOperand)} must be set for {DisplayName} operation.");
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
            // If one is DateTime and the other isn't (or not parsable as such), they are not equal.
            // Or if targetFieldType is DateTime and parsing fails for either.
            return false;
        }

        // Attempt numeric comparison
        if (IsNumericType(leftValue) && IsNumericType(rightValue))
        {
            // Using double for comparison can lose precision. Prefer decimal for financial or exact comparisons.
            // For general cases, double is often used. Let's try to be robust.
            if (TryParseDecimal(leftValue, out var leftDecimal) && TryParseDecimal(rightValue, out var rightDecimal))
            {
                return leftDecimal == rightDecimal;
            }
            // Fallback to double if decimal parsing fails or if types are mixed (e.g. int and double string)
             if (TryParseDouble(leftValue, out var leftDouble) && TryParseDouble(rightValue, out var rightDouble))
            {
                // Using a small epsilon for floating-point comparison might be needed if precision issues are a concern.
                // For now, direct comparison.
                return leftDouble.Equals(rightDouble);
            }
            // If numeric types but cannot parse to a common comparable type, consider them not equal.
            return false;
        }
        
        // Fallback to string comparison (case-insensitive as per original likely intent for "Equals")
        // If one is numeric and the other is string (and not parsable as numeric), they are not equal by type.
        if ((IsNumericType(leftValue) && rightValue is string) || (leftValue is string && IsNumericType(rightValue)))
        {
             // If one is clearly numeric and the other is a string that wasn't parsed as numeric above,
             // they are not equal. (e.g. 5 vs "apple")
             // This check assumes TryParseDouble/Decimal would have handled cases like 5 vs "5.0"
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
        return double.TryParse(obj.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
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
        return decimal.TryParse(obj.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }


    /// <inheritdoc />
    public override ComparisonOperationBase Clone() // Ensure return type is ComparisonOperationBase
    {
        return new NotEqualOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
    }
}

// The NotEqualOperationExtensions class is no longer needed as its logic is integrated.
