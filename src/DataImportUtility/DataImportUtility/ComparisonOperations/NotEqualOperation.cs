using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if two values are not equal.
/// </summary>
public class NotEqualOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(NotEqualOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Does not equal";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if two values are not equal.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null && RightOperand is null)
        {
            return true;
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

        return leftResult.IsNotEqualTo(rightResult);
    }
}

/// <summary>
/// Extension methods for the IsNotEqualTo operation.
/// </summary>
public static class NotEqualOperationExtensions
{
    /// <summary>
    /// Checks if the left result does not equal the right result.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="rightResult">The result of the right operand.</param>
    /// <returns>True if the left result does not equal the right result; otherwise, false.</returns>
    public static bool IsNotEqualTo(this TransformationResult leftResult, TransformationResult rightResult)
        => !leftResult.Equals(rightResult);
}