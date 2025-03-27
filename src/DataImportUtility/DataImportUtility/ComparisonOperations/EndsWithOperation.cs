using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value ends with another value.
/// </summary>
public class EndsWithOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(EndsWithOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Ends with";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value ends with another value.";

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

        return leftResult.EndsWith(rightResult);
    }
}

/// <summary>
/// Extension methods for the EndsWith operation.
/// </summary>
public static class EndsWithOperationExtensions
{
    /// <summary>
    /// Checks if the left result ends with the value.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the left result ends with the value; otherwise, false.</returns>
    public static bool EndsWith(this TransformationResult leftResult, TransformationResult value)
    {
        // Handle null cases
        if (leftResult.Value is null || value.Value is null) { return false; }

        return leftResult.Value.EndsWith(value.Value, StringComparison.Ordinal);
    }
}