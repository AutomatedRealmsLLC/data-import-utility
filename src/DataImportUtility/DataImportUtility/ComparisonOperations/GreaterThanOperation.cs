﻿using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value is greater than another value.
/// </summary>
public class GreaterThanOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(GreaterThanOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Greater than";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value is greater than another value.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null || RightOperand is null)
        {
            throw new InvalidOperationException($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set.");
        }

        var leftResult = await (LeftOperand?.Apply(result) ?? Task.FromResult(result with { Value = null }));
        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate left operand for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var rightResult = await (RightOperand?.Apply(result) ?? Task.FromResult(result with { Value = null }));
        if (rightResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate right operand for {DisplayName} operation: {rightResult.ErrorMessage}");
        }

        return leftResult.GreaterThan(rightResult);
    }
}

/// <summary>
/// Extension methods for the GreaterThan operation.
/// </summary>
public static class GreaterThanOperationExtensions
{
    /// <summary>
    /// Checks if the left result is greater than the right result.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="rightResult">The result of the right operand.</param>
    /// <returns>True if the left result is greater than the right result; otherwise, false.</returns>
    public static bool GreaterThan(this TransformationResult leftResult, TransformationResult rightResult)
    {
        // Handle null cases
        if (leftResult.Value is null || rightResult.Value is null) { return false; }

        // Try numeric comparison
        if (double.TryParse(leftResult.Value, out var leftValue)
            && double.TryParse(rightResult.Value, out var rightValue))
        {
            return leftValue > rightValue;
        }

        // Try date comparison
        if (DateTime.TryParse(leftResult.Value, out var leftDate)
            && DateTime.TryParse(rightResult.Value, out var rightDate))
        {
            return leftDate > rightDate;
        }

        // Fall back to string comparison
        return string.Compare(leftResult.Value, rightResult.Value, StringComparison.Ordinal) > 0;
    }
}