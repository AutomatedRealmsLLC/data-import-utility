using System.Globalization; // For CultureInfo and NumberStyles
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult and ITransformationContext

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is not in a set of values.
/// </summary>
public class NotInOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.NotIn";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is not in";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is not in a set of values (case-insensitive for strings by default).";

    /// <summary>
    /// The set of rules providing values to check against.
    /// </summary>
    public IEnumerable<MappingRuleBase>? Values { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotInOperation"/> class.
    /// </summary>
    public NotInOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(MappingRuleBase leftOperand, MappingRuleBase? rightOperand, MappingRuleBase? secondaryRightOperand)
    {
        this.LeftOperand = leftOperand ?? throw new ArgumentNullException(nameof(leftOperand), $"Left operand cannot be null for {DisplayName}.");
        // 'Values' collection is used by this operation instead of RightOperand, LowLimit, or HighLimit from the base.
        // These base properties are explicitly not set or used by NotInOperation's core logic.
        // It's assumed 'Values' is populated via deserialization or direct assignment.
        this.RightOperand = null; 
        this.LowLimit = null;
        this.HighLimit = null;
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        ITransformationContext? context = contextResult as ITransformationContext;
        if (context == null)
        {
            if (contextResult is ITransformationContext directContext)
            {
                context = directContext;
            }
            else
            {
                throw new InvalidOperationException($"The provided contextResult (type: {contextResult?.GetType().FullName}) could not be interpreted as an {nameof(ITransformationContext)} for {DisplayName} operation.");
            }
        }

        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be configured for {DisplayName} operation.");
        }

        if (Values is null || !Values.Any())
        {
            return true; // If there are no values to check against, the LeftOperand cannot be "in" the set.
        }

        var leftResult = await LeftOperand.Apply(context);

        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }

        object? leftValue = leftResult.CurrentValue;

        foreach (var valueRule in Values)
        {
            if (valueRule == null) continue;

            var valueRuleResult = await valueRule.Apply(context);
            if (valueRuleResult == null || valueRuleResult.WasFailure)
            {
                throw new InvalidOperationException($"Failed to evaluate a value in {nameof(Values)} for {DisplayName} operation: {valueRuleResult?.ErrorMessage ?? "Result was null."}");
            }

            // Using TargetFieldType from the contextResult if available, otherwise null.
            // This assumes TransformationResult might have this property, or ITransformationContext does.
            // If not, the AreValuesEqual method needs to be robust to a null targetFieldType.
            Type? targetFieldType = (context as TransformationResult)?.TargetFieldType; 

            if (AreValuesEqual(leftValue, valueRuleResult.CurrentValue, targetFieldType))
            {
                return false; // Left value is IN the set, so NOT IN is false.
            }
        }

        return true; // Left value was not found in any of the values, so NOT IN is true.
    }

    private static bool AreValuesEqual(object? leftValue, object? rightValue, Type? targetFieldType)
    {
        if (leftValue == null && rightValue == null)
        {
            return true;
        }
        if (leftValue == null || rightValue == null)
        {
            // If one is null and the other is an empty string, consider them equal for "In" checks.
            if ((leftValue == null && rightValue is string rs && string.IsNullOrEmpty(rs)) ||
                (rightValue == null && leftValue is string ls && string.IsNullOrEmpty(ls)))
            {
                return true;
            }
            return false;
        }

        // Attempt type-aware comparison first
        if (targetFieldType != null)
        {
            try
            {
                var convertedLeft = ConvertToType(leftValue, targetFieldType);
                var convertedRight = ConvertToType(rightValue, targetFieldType);
                if (convertedLeft != null && convertedRight != null && convertedLeft.Equals(convertedRight)) return true;
                // Fallback if conversion fails or types are tricky
            }
            catch { /* Conversion failed, fallback to string or other comparisons */ }
        }

        if (leftValue is DateTime || rightValue is DateTime || (targetFieldType == typeof(DateTime)))
        {
            if (TryParseDateTime(leftValue, out var leftDate) && TryParseDateTime(rightValue, out var rightDate))
            {
                return leftDate.Equals(rightDate);
            }
            // If one is DateTime and other isn't parseable as such, they aren't equal in this context.
            return false;
        }
        
        bool leftIsNumeric = IsNumericType(leftValue);
        bool rightIsNumeric = IsNumericType(rightValue);

        if (leftIsNumeric && rightIsNumeric)
        {
            // Try decimal first for precision
            if (TryParseDecimal(leftValue, out var leftDecimal) && TryParseDecimal(rightValue, out var rightDecimal))
            {
                return leftDecimal == rightDecimal;
            }
            // Fallback to double if decimal parsing fails (e.g. scientific notation not handled by decimal.Parse by default)
            if (TryParseDouble(leftValue, out var leftDouble) && TryParseDouble(rightValue, out var rightDouble))
            {
                // Using a small epsilon for double comparison might be needed for calculated values,
                // but for direct "In" list checks, exact equality or string representation is often intended.
                return leftDouble.Equals(rightDouble);
            }
            // If they are numeric but cannot be parsed consistently to decimal or double, fallback to string.
        }
        else if (leftIsNumeric || rightIsNumeric) // One is numeric, the other is not (and not DateTime)
        {
            // If comparing a number to a non-numeric string (e.g., 5 vs "apple"), they are not equal.
            // If comparing a number to a string that *looks* like a number (e.g. 5 vs "5.0"),
            // the numeric parsing above should handle it if both were treated as numbers.
            // If one is definitively numeric and the other is not (and not parseable as such), consider not equal.
            // However, the problem statement often implies string comparison as a fallback.
            // Let's try to parse the string one as a number if the other is a number.
            if (leftIsNumeric && rightValue is string rStr)
            {
                if (TryParseDecimal(rStr, out var rDec) && TryParseDecimal(leftValue, out var lDec)) return lDec == rDec;
                if (TryParseDouble(rStr, out var rDbl) && TryParseDouble(leftValue, out var lDbl)) return lDbl.Equals(rDbl);
            }
            else if (rightIsNumeric && leftValue is string lStr)
            {
                if (TryParseDecimal(lStr, out var lDec) && TryParseDecimal(rightValue, out var rDec)) return lDec == rDec;
                if (TryParseDouble(lStr, out var lDbl) && TryParseDouble(rightValue, out var rDbl)) return lDbl.Equals(rDbl);
            }
            // If one is numeric and the other is a string not representing that number, they are not equal.
            // Fall through to string comparison, which might equate "5" and 5.0 if both become "5" or "5.0".
        }

        // Fallback to case-insensitive string comparison
        return string.Equals(leftValue.ToString(), rightValue.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static object? ConvertToType(object? value, Type targetType)
    {
        if (value == null) return null;
        if (targetType == null) return value;

        if (value.GetType() == targetType) return value;

        try
        {
            if (targetType.IsEnum) return Enum.Parse(targetType, value.ToString()!, true);
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            // Try specific parsing for common types if ChangeType fails
            if (targetType == typeof(DateTime))
            {
                if (TryParseDateTime(value, out var dt)) return dt;
            }
            else if (targetType == typeof(decimal))
            {
                if (TryParseDecimal(value, out var dec)) return dec;
            }
            else if (targetType == typeof(double))
            {
                if (TryParseDouble(value, out var dbl)) return dbl;
            }
            // Add other specific parsers if needed
            return null; // Or throw, or return original value
        }
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
        if (obj is DateTimeOffset dto) { result = dto.DateTime; return true; }
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
    public override ComparisonOperationBase Clone()
    {
        var clone = new NotInOperation();
        if (this.LeftOperand != null)
        {
            clone.LeftOperand = (MappingRuleBase)this.LeftOperand.Clone();
        }
        // RightOperand from base is not used by NotInOperation.
        if (this.Values != null)
        {
            clone.Values = this.Values.Select(v => (MappingRuleBase)v.Clone()).ToList();
        }
        return clone;
    }
}
