using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Globalization;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is less than or equal to another value.
/// </summary>
public class LessThanOrEqualOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.LessThanOrEqual";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Less than or equal to";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is less than or equal to another value.";

    /// <summary>
    /// Initializes a new instance of the <see cref="LessThanOrEqualOperation"/> class.
    /// </summary>
    public LessThanOrEqualOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(
        MappingRuleBase leftOperand,
        MappingRuleBase? rightOperand,
        MappingRuleBase? secondaryRightOperand) // Not used by LessThanOrEqualOperation
    {
        base.ConfigureOperands(leftOperand, rightOperand, null); // secondaryRightOperand is not used

        if (LeftOperand is null)
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand must be provided for {TypeIdString}.");
        }
        if (RightOperand is null)
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

        var leftOpResult = await LeftOperand.Apply(contextResult) ?? throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned a null TransformationResult.");

        if (leftOpResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftOpResult.ErrorMessage}");
        }

        var rightOpResult = await RightOperand.Apply(contextResult) ?? throw new InvalidOperationException($"Applying {nameof(RightOperand)} for {DisplayName} operation returned a null TransformationResult.");

        if (rightOpResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightOpResult.ErrorMessage}");
        }

        var leftVal = leftOpResult.CurrentValue;
        var rightVal = rightOpResult.CurrentValue;

        if (leftVal is null && rightVal is null) return true; // null is considered equal to null
        if (leftVal is null || rightVal is null) return false; // if one is null and the other isn't, they are not equal, and less/greater is not applicable.

        // Try numeric comparison first
        if (leftVal.IsNumericType() && rightVal.IsNumericType())
        {
            try
            {
                // Convert to decimal for comparison to handle various numeric types consistently.
                var leftDecimal = Convert.ToDecimal(leftVal, CultureInfo.InvariantCulture);
                var rightDecimal = Convert.ToDecimal(rightVal, CultureInfo.InvariantCulture);
                return leftDecimal <= rightDecimal;
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
        if (leftVal.CanConvertToDateTime(out DateTime leftDate) && rightVal.CanConvertToDateTime(out DateTime rightDate))
        {
            return leftDate <= rightDate;
        }

        // Fallback to string comparison
        var leftString = Convert.ToString(leftVal, CultureInfo.InvariantCulture);
        var rightString = Convert.ToString(rightVal, CultureInfo.InvariantCulture);

        return string.Compare(leftString, rightString, StringComparison.Ordinal) <= 0;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return (LessThanOrEqualOperation)base.Clone();
    }
}
