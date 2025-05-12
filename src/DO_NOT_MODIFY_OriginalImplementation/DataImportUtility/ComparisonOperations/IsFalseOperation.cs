using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value is false.
/// </summary>
public class IsFalseOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(IsFalseOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Is false";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value is false.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {nameof(IsFalseOperation)}.");
        }

        var leftResult = await LeftOperand.Apply(result);

        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        return leftResult.IsFalse();
    }
}

/// <summary>
/// Extension methods for the IsFalse operation.
/// </summary>
public static class IsFalseOperationExtensions
{
    /// <summary>
    /// Checks if the result is false.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is false; otherwise, false.</returns>
    /// <remarks>
    /// A result is considered false if it is null, an empty string,
    /// or a falsy value (zero stored as a string, the string "false").
    /// Everything else is considered not false.
    /// </remarks>
    public static bool IsFalse(this TransformationResult result)
        => !result.IsTrue();
}