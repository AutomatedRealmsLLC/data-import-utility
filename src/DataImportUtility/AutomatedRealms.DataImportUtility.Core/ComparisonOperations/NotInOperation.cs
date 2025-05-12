using System.Globalization; // For CultureInfo and NumberStyles
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is not in a set of values.
/// </summary>
public class NotInOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(NotInOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is not in";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is not in a set of values (case-insensitive for strings).";

    /// <summary>
    /// Gets or sets the order of this operation if it's part of an enumerated list of operations.
    /// </summary>
    public int EnumMemberOrder { get; set; }

    /// <summary>
    /// The set of rules providing values to check against.
    /// </summary>
    public IEnumerable<MappingRuleBase>? Values { get; set; }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is an ITransformationContext
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }

        if (Values is null || !Values.Any())
        {
            // If there are no values to check against, the LeftOperand cannot be "in" the set.
            // Therefore, "NotIn" should be true.
            return true;
        }

        var leftResult = await LeftOperand.Apply(contextResult);

        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }

        object? leftValue = leftResult.CurrentValue;

        foreach (var valueRule in Values)
        {
            if (valueRule == null) continue; // Skip null rules in the list

            var valueRuleResult = await valueRule.Apply(contextResult);
            if (valueRuleResult == null || valueRuleResult.WasFailure)
            {
                // If any value rule in the list fails to evaluate, it's an issue.
                // Depending on desired behavior, could throw, log, or treat as non-match.
                // For now, let's throw, as it indicates a configuration or data problem.
                throw new InvalidOperationException($"Failed to evaluate a value in {nameof(Values)} for {DisplayName} operation: {valueRuleResult?.ErrorMessage ?? "Result was null."}");
            }

            if (AreValuesEqual(leftValue, valueRuleResult.CurrentValue, contextResult.TargetFieldType))
            {
                return false; // Left value is IN the set, so NOT IN is false.
            }
        }

        return true; // Left value was not found in any of the values, so NOT IN is true.
    }

    // Using the same AreValuesEqual helper from EqualsOperation/NotEqualOperation for consistency.
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

        if (targetFieldType == typeof(DateTime) || leftValue is DateTime || rightValue is DateTime)
        {
            if (TryParseDateTime(leftValue, out var leftDate) && TryParseDateTime(rightValue, out var rightDate))
            {
                return leftDate.Equals(rightDate);
            }
            return false;
        }

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
        return value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal;
    }

    private static bool TryParseDateTime(object? obj, out DateTime result)
    {
        result = default;
        if (obj == null) return false;
        if (obj is DateTime dt) { result = dt; return true; }
        return DateTime.TryParse(obj.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out result);
    }

    private static bool TryParseDouble(object? obj, out double result)
    {
        result = default;
        if (obj == null) return false;
        if (obj is double d) { result = d; return true; }
        if (obj is IConvertible convertible) { try { result = convertible.ToDouble(CultureInfo.InvariantCulture); return true; } catch { /* Fall through */ } }
        return double.TryParse(obj.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseDecimal(object? obj, out decimal result)
    {
        result = default;
        if (obj == null) return false;
        if (obj is decimal dec) { result = dec; return true; }
        if (obj is IConvertible convertible) { try { result = convertible.ToDecimal(CultureInfo.InvariantCulture); return true; } catch { /* Fall through */ } }
        return decimal.TryParse(obj.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone() // Ensure return type is ComparisonOperationBase
    {
        return new NotInOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            Values = Values?.Select(v => v.Clone()).ToList(), // Ensure correct cast for cloned values
            EnumMemberOrder = EnumMemberOrder
        };
    }
}
