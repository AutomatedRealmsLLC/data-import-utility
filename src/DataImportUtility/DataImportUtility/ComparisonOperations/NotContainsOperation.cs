using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value does not contain another value.
/// </summary>
public class NotContainsOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(NotContainsOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Does not contain";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value does not contain another value.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand == null || RightOperand == null)
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

        return leftResult.NotContains(rightResult);
    }
}

/// <summary>
/// Extension methods for the NotContains operation.
/// </summary>
public static class NotContainsOperationExtensions
{
    /// <summary>
    /// Checks if the left result does not contain the value.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the left result does not contain the value; otherwise, false.</returns>
    /// <remarks>
    /// If the input value is an array, it will check if the left result does not contain the
    /// string value from the value TransformationResult.<br />
    /// <br />
    /// If the input value is a single string, it will check if the left result does not contain the
    /// string value from the value TransformationResult.
    /// </remarks>
    public static bool NotContains(this TransformationResult leftResult, TransformationResult value)
    {
        throw new NotImplementedException();
    }
}