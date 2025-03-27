﻿using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value is less than or equal to another value.
/// </summary>
public class LessThanOrEqualOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(LessThanOrEqualOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Less than or equal";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value is less than or equal to another value.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null || RightOperand is null)
        {
            throw new InvalidOperationException($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set.");
        }

        var leftResult = await LeftOperand.Apply(result);
        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var rightResult = await RightOperand.Apply(result);
        if (rightResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult.ErrorMessage}");
        }

        return leftResult.LessThanOrEqual(rightResult);
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
    public static bool LessThanOrEqual(this TransformationResult leftResult, TransformationResult rightResult)
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
