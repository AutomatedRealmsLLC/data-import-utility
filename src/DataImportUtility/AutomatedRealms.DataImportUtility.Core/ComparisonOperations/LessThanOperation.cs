using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Globalization;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is less than another value.
/// </summary>
public class LessThanOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.LessThan";

    // EnumMemberName removed

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Less than";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is less than another value.";

    /// <summary>
    /// Initializes a new instance of the <see cref="LessThanOperation"/> class.
    /// </summary>
    public LessThanOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(
        MappingRuleBase leftOperand,
        MappingRuleBase? rightOperand,
        MappingRuleBase? secondaryRightOperand) // Not used by LessThanOperation
    {
        base.ConfigureOperands(leftOperand, rightOperand, null); // secondaryRightOperand is not used

        if (this.LeftOperand is null)
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand must be provided for {TypeIdString}.");
        }
        if (this.RightOperand is null)
        {
            throw new ArgumentNullException(nameof(rightOperand), $"Right operand must be provided for {TypeIdString}.");
        }
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null || RightOperand is null) // Should be caught by ConfigureOperands
        {
            throw new InvalidOperationException($"Both LeftOperand and RightOperand must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }

        var leftOpResult = await LeftOperand.Apply(contextResult);
        if (leftOpResult == null)
        {
            throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned a null TransformationResult.");
        }
        if (leftOpResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftOpResult.ErrorMessage}");
        }

        var rightOpResult = await RightOperand.Apply(contextResult);
        if (rightOpResult == null)
        {
            throw new InvalidOperationException($"Applying {nameof(RightOperand)} for {DisplayName} operation returned a null TransformationResult.");
        }
        if (rightOpResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightOpResult.ErrorMessage}");
        }

        object? leftVal = leftOpResult.CurrentValue;
        object? rightVal = rightOpResult.CurrentValue;

        if (leftVal == null || rightVal == null)
        {
            // Consistent with how other operations handle nulls when comparison isn't meaningful.
            // For 'less than', if either is null, the comparison is generally false unless specific null semantics are defined.
            return false;
        }

        // Try numeric comparison first
        if (IsNumeric(leftVal) && IsNumeric(rightVal))
        {
            try
            {
                // Convert to decimal for comparison to handle various numeric types consistently.
                decimal leftDecimal = Convert.ToDecimal(leftVal, CultureInfo.InvariantCulture);
                decimal rightDecimal = Convert.ToDecimal(rightVal, CultureInfo.InvariantCulture);
                return leftDecimal < rightDecimal;
            }
            catch (OverflowException ex)
            {
                throw new InvalidOperationException($"Numeric comparison in {DisplayName} failed due to an overflow. Left: '{leftVal}', Right: '{rightVal}'.", ex);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException($"Numeric comparison in {DisplayName} failed due to a format error. Left: '{leftVal}', Right: '{rightVal}'.", ex);
            }
        }

        // Try DateTime comparison
        if (CanConvertToDateTime(leftVal, out DateTime leftDate) && CanConvertToDateTime(rightVal, out DateTime rightDate))
        {
            return leftDate < rightDate;
        }

        // Fallback to string comparison if types are not directly comparable as numbers or dates
        // This is a common fallback but might not always be the desired behavior for all mixed types.
        // Consider if specific type mismatch errors should be thrown instead for certain scenarios.
        var leftString = Convert.ToString(leftVal, CultureInfo.InvariantCulture);
        var rightString = Convert.ToString(rightVal, CultureInfo.InvariantCulture);

        // Note: String comparison is culture-sensitive. Using Ordinal for a consistent, byte-by-byte comparison.
        // If culture-specific linguistic comparison is needed, CultureInfo.CurrentCulture or a specific culture should be used.
        return string.Compare(leftString, rightString, StringComparison.Ordinal) < 0;
    }

    private static bool IsNumeric(object? value)
    {
        return value != null && (value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint ||
               value is long || value is ulong || value is float || value is double || value is decimal);
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
            result = dto.UtcDateTime; // Ensure comparison in UTC
            return true;
        }
        var stringValue = value.ToString();
        if (string.IsNullOrEmpty(stringValue)) return false;
        // Attempt to parse with InvariantCulture and assume UTC if no offset is present.
        return DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out result);
    }


    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return (LessThanOperation)base.Clone();
    }
}
