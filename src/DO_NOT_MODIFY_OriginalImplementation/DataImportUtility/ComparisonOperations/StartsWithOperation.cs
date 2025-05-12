using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value starts with another value.
/// </summary>
public class StartsWithOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(StartsWithOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Starts with";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value starts with another value.";

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
            throw new InvalidOperationException("Left operand failed to transform.");
        }

        var rightResult = await RightOperand.Apply(result);
        if (rightResult.WasFailure)
        {
            throw new InvalidOperationException("Right operand failed to transform.");
        }

        return leftResult.StartsWith(rightResult);
    }
}

/// <summary>
/// Extension methods for the StartsWith operation.
/// </summary>
public static class StartsWithOperationExtensions
{
    /// <summary>
    /// Checks if the left result starts with the value.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the left result starts with the value; otherwise, false.</returns>
    public static bool StartsWith(this TransformationResult leftResult, TransformationResult value)
    {
        // Handle null cases
        if (leftResult.Value is null || value.Value is null) { return false; }

        return leftResult.Value.StartsWith(value.Value, StringComparison.Ordinal);
    }
}