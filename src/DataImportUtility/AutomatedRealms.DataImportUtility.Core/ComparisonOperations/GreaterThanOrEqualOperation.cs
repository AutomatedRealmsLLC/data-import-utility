using AutomatedRealms.DataImportUtility.Abstractions; // For ITransformationContext, ComparisonOperationBase
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult
using AutomatedRealms.DataImportUtility.Abstractions.Interfaces; // Ensure ITransformationContext is known
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is greater than or equal to another value.
/// </summary>
public class GreaterThanOrEqualOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(GreaterThanOrEqualOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Greater than or equal to";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is greater than or equal to another value.";

    // Removed EnumMemberOrder property

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is a TransformationResult, which implements ITransformationContext
    {
        if (LeftOperand is null || RightOperand is null)
        {
            // TODO: Log this failure.
            return false; 
        }

        // This should now call MappingRuleBase.Apply(ITransformationContext context)
        TransformationResult? leftOpResult = await LeftOperand.Apply(contextResult);
        
        if (leftOpResult is null || leftOpResult.WasFailure)
        {
            // TODO: Log leftOpResult?.ErrorMessage or null operand
            return false;
        }

        TransformationResult? rightOpResult = await RightOperand.Apply(contextResult);

        if (rightOpResult is null || rightOpResult.WasFailure)
        {
            // TODO: Log rightOpResult?.ErrorMessage or null operand
            return false;
        }
        
        return leftOpResult.GreaterThanOrEqual(rightOpResult); // Extension method
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        // Base Clone() handles operands and MemberwiseClone.
        return base.Clone();
    }
}

/// <summary>
/// Extension methods for the GreaterThanOrEqual operation.
/// </summary>
public static class GreaterThanOrEqualOperationExtensions
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
    /// Checks if the left result's current value is greater than or equal to the right result's current value.
    /// </summary>
    /// <param name="leftResult">The transformation result of the left operand.</param>
    /// <param name="rightResult">The transformation result of the right operand.</param>
    /// <returns>True if the left value is greater than or equal to the right value; otherwise, false.</returns>
    public static bool GreaterThanOrEqual(this TransformationResult leftResult, TransformationResult rightResult)
    {
        object? leftVal = leftResult.CurrentValue;
        object? rightVal = rightResult.CurrentValue;

        if (leftVal == null && rightVal == null) return true;
        if (leftVal == null || rightVal == null) return false;

        if (IsNumeric(leftVal) && IsNumeric(rightVal))
        {
            try
            {
                decimal dLeft = Convert.ToDecimal(leftVal, CultureInfo.InvariantCulture);
                decimal dRight = Convert.ToDecimal(rightVal, CultureInfo.InvariantCulture);
                return dLeft >= dRight;
            }
            catch (OverflowException)
            {
                try
                {
                    double dblLeft = Convert.ToDouble(leftVal, CultureInfo.InvariantCulture);
                    double dblRight = Convert.ToDouble(rightVal, CultureInfo.InvariantCulture);
                    return dblLeft >= dblRight;
                }
                catch { /* Fall through */ }
            }
            catch (FormatException) { /* Fall through */ }
        }

        if (CanConvertToDateTime(leftVal, out var leftDate) && CanConvertToDateTime(rightVal, out var rightDate))
        {
            return leftDate >= rightDate;
        }

        var sLeft = leftVal.ToString();
        var sRight = rightVal.ToString();
        if (sLeft == null || sRight == null) return false;
        
        return string.Compare(sLeft, sRight, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
