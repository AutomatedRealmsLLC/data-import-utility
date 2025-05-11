using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is less than or equal to another value.
/// </summary>
public class LessThanOrEqualOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(LessThanOrEqualOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Less than or equal";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is less than or equal to another value.";

    public int EnumMemberOrder { get; set; }

    /// <inheritdoc />
    public override async Task<TransformationResult<bool>> Evaluate(ITransformationContext context)
    {
        if (LeftOperand is null || RightOperand is null)
        {
            return TransformationResult<bool>.CreateFailure($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set for {DisplayName} operation.");
        }

        var leftResult = await (LeftOperand?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));
        if (leftResult.WasFailure)
        {
            return TransformationResult<bool>.CreateFailure($"Failed to evaluate left operand for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var rightResult = await (RightOperand?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));
        if (rightResult.WasFailure)
        {
            return TransformationResult<bool>.CreateFailure($"Failed to evaluate right operand for {DisplayName} operation: {rightResult.ErrorMessage}");
        }

        return TransformationResult<bool>.CreateSuccess(leftResult.LessThanOrEqual(rightResult), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new LessThanOrEqualOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
    }
}

/// <summary>
/// Extension methods for the LessThanOrEqual operation.
/// </summary>
public static class LessThanOrEqualOperationExtensions
{
    /// <summary>
    /// Checks if the left result is less than or equal to the right result.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="rightResult">The result of the right operand.</param>
    /// <returns>True if the left result is less than or equal to the right result; otherwise, false.</returns>
    public static bool LessThanOrEqual(this TransformationResult<string?> leftResult, TransformationResult<string?> rightResult)
    {
        // Handle null cases
        if (leftResult.Value is null || rightResult.Value is null) { return false; }

        // Try numeric comparison
        if (double.TryParse(leftResult.Value, out var leftValue)
            && double.TryParse(rightResult.Value, out var rightValue))
        {
            return leftValue <= rightValue;
        }

        // Try date comparison
        if (DateTime.TryParse(leftResult.Value, out var leftDate)
            && DateTime.TryParse(rightResult.Value, out var rightDate))
        {
            return leftDate <= rightDate;
        }

        // Fall back to string comparison
        return string.Compare(leftResult.Value, rightResult.Value, StringComparison.Ordinal) <= 0;
    }
}
