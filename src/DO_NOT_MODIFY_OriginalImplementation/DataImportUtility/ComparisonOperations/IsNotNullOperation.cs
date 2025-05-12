using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value is not null.
/// </summary>
public class IsNotNullOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(IsNotNullOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Is not null";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value is not null.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {nameof(IsNotNullOperation)}.");
        }

        var leftResult = await LeftOperand.Apply(result);
        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        return leftResult.IsNotNull();
    }
}

/// <summary>
/// Extension method for the IsNotNull operation.
/// </summary>
public static class IsNotNullOperationExtensions
{
    /// <summary>
    /// Checks if the result is not null.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is not null; otherwise, false.</returns>
    public static bool IsNotNull(this TransformationResult result)
        => !result.IsNull();
}
