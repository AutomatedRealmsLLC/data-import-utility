using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if two values are equal.
/// </summary>
public class EqualsOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(EqualsOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Equals";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if two values are equal.";

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Failed to evaluate left operand for {DisplayName} operation: {leftResult.ErrorMessage}</exception>"
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

        return leftResult.IsEqualTo(rightResult);
    }
}

/// <summary>
/// Extension methods for the IsEqualTo operation.
/// </summary>
public static class EqualsOperationExtensions
{
    /// <summary>
    /// Checks if the left result equals the right result.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="rightResult">The result of the right operand.</param>
    /// <returns>True if the left result equals the right result; otherwise, false.</returns>
    public static bool IsEqualTo(this TransformationResult leftResult, TransformationResult rightResult)
        => leftResult.Value == rightResult.Value;
}
