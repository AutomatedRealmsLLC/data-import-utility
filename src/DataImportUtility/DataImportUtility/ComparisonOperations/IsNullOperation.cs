using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value is null.
/// </summary>
public class IsNullOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(IsNullOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Is null";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value is null.";

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

        return leftResult.IsNull();
    }
}

/// <summary>
/// Extension method for the IsNull operation.
/// </summary>
public static class IsNullOperationExtensions
{
    /// <summary>
    /// Checks if the result is null.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is null; otherwise, false.</returns>
    public static bool IsNull(this TransformationResult result)
        => result.Value is null;
}
