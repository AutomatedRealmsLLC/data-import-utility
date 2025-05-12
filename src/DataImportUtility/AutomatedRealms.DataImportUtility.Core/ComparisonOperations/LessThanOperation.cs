using System.Globalization; // For CultureInfo
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For non-generic TransformationResult

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is less than another value.
/// </summary>
public class LessThanOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(LessThanOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Less than";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is less than another value.";

    // Removed EnumMemberOrder property as it's not in ComparisonOperationBase

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // Changed to non-generic TransformationResult
    {
        if (LeftOperand is null || RightOperand is null)
        {
            // TODO: Log this failure
            return false;
        }

        // Apply should now resolve to MappingRuleBase.Apply(ITransformationContext context)
        var leftOpResult = await LeftOperand.Apply(contextResult);
        if (leftOpResult is null || leftOpResult.WasFailure)
        {
            // TODO: Log leftOpResult?.ErrorMessage or null operand
            return false;
        }

        var rightOpResult = await RightOperand.Apply(contextResult);
        if (rightOpResult is null || rightOpResult.WasFailure)
        {
            // TODO: Log rightOpResult?.ErrorMessage or null operand
            return false;
        }

        return leftOpResult.LessThan(rightOpResult); // Extension method on non-generic TransformationResult
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone() // Changed return type to ComparisonOperationBase
    {
        return base.Clone(); // Utilize base class cloning
    }
}

/// <summary>
/// Extension methods for the LessThan operation.
/// </summary>
public static class LessThanOperationExtensions
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
    /// Checks if the left result is less than the right result.
    /// </summary>
    /// <param name="leftResult">The result of the left operand (non-generic TransformationResult).</param>
    /// <param name="rightResult">The result of the right operand (non-generic TransformationResult).</param>
    /// <returns>True if the left result is less than the right result; otherwise, false.</returns>
    public static bool LessThan(this TransformationResult leftResult, TransformationResult rightResult)
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
                return dLeft < dRight;
            }
            catch (OverflowException)
            {
                try
                {
                    double dblLeft = Convert.ToDouble(leftVal, CultureInfo.InvariantCulture);
                    double dblRight = Convert.ToDouble(rightVal, CultureInfo.InvariantCulture);
                    return dblLeft < dblRight;
                }
                catch { /* Fall through */ }
            }
            catch (FormatException) { /* Fall through */ }
        }

        if (CanConvertToDateTime(leftVal, out var leftDate) && CanConvertToDateTime(rightVal, out var rightDate))
        {
            return leftDate < rightDate;
        }

        var sLeft = leftVal.ToString();
        var sRight = rightVal.ToString();
        if (sLeft == null || sRight == null) return false;
        return string.Compare(sLeft, sRight, StringComparison.OrdinalIgnoreCase) < 0;
    }
}
