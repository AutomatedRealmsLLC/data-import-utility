using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Globalization;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is greater than another value.
/// </summary>
public class GreaterThanOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.GreaterThan";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Greater than";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is greater than another value.";

    /// <summary>
    /// Initializes a new instance of the <see cref="GreaterThanOperation"/> class.
    /// </summary>
    public GreaterThanOperation() : base(TypeIdString) // Pass TypeIdString to base constructor
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
            // For GreaterThan, RightOperand is the value to compare against (like LowLimit in base, but conceptually the primary comparison value)
            throw new ArgumentNullException(nameof(rightOperand), $"Right operand (value to compare against) must be provided for {TypeIdString}.");
        }
        // secondaryRightOperand is not used by GreaterThanOperation.
        // Base ConfigureOperands sets HighLimit to secondaryRightOperand, which is fine as it won't be used.
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null || RightOperand is null) // Should be caught by ConfigureOperands
        {
            throw new InvalidOperationException($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }

        var leftOpResult = await LeftOperand.Apply(contextResult);
        if (leftOpResult is null || leftOpResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftOpResult?.ErrorMessage ?? "Result was null."}");
        }

        var rightOpResult = await RightOperand.Apply(contextResult);
        if (rightOpResult is null || rightOpResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightOpResult?.ErrorMessage ?? "Result was null."}");
        }

        return leftOpResult.GreaterThan(rightOpResult); // Extension method on TransformationResult
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        // Base Clone() handles LeftOperand, RightOperand and MemberwiseClone.
        return (GreaterThanOperation)base.Clone();
    }
}

/// <summary>
/// Extension methods for the GreaterThan operation.
/// </summary>
public static class GreaterThanOperationExtensions
{
    /// <summary>
    /// Checks if the left result's current value is greater than the right result's current value.
    /// </summary>
    /// <param name="leftResult">The transformation result of the left operand.</param>
    /// <param name="rightResult">The transformation result of the right operand.</param>
    /// <returns>True if the left value is greater than the right value; otherwise, false.</returns>
    public static bool GreaterThan(this TransformationResult leftResult, TransformationResult rightResult)
    {
        var leftVal = leftResult.CurrentValue;
        var rightVal = rightResult.CurrentValue;

        if (leftVal is null || rightVal is null) { return false; }

        if (leftVal.IsNumericType() && rightVal.IsNumericType())
        {
            try
            {
                var dLeft = Convert.ToDecimal(leftVal, CultureInfo.InvariantCulture);
                var dRight = Convert.ToDecimal(rightVal, CultureInfo.InvariantCulture);
                return dLeft > dRight;
            }
            catch (OverflowException)
            {
                try
                {
                    var dblLeft = Convert.ToDouble(leftVal, CultureInfo.InvariantCulture);
                    var dblRight = Convert.ToDouble(rightVal, CultureInfo.InvariantCulture);
                    return dblLeft > dblRight;
                }
                catch { /* Fall through */ }
            }
            catch (FormatException) { /* Fall through */ }
        }

        if (leftVal.CanConvertToDateTime(out var leftDate) && rightVal.CanConvertToDateTime(out var rightDate))
        {
            return leftDate > rightDate;
        }

        var sLeft = leftVal.ToString();
        var sRight = rightVal.ToString();
        return sLeft is not null && sRight is not null && string.Compare(sLeft, sRight, StringComparison.OrdinalIgnoreCase) > 0;
    }
}
