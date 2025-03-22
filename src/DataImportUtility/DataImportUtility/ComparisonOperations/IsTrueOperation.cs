using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value is true.
/// </summary>
public class IsTrueOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(IsTrueOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Is true";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value is true.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {nameof(IsTrueOperation)}.");
        }

        var leftResult = await LeftOperand.Apply(result);
        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        return leftResult.IsTrue();
    }
}

/// <summary>
/// Extension methods for the IsTrue operation.
/// </summary>
public static class IsTrueOperationExtensions
{
    /// <summary>
    /// Checks if the result is true.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is true; otherwise, false.</returns>
    /// <remarks>
    /// A result is considered true if it not null and
    /// a truthy value (non-zero numbers stored as a string, the string "true").
    /// Everything else is considered not true.
    /// </remarks>
    public static bool IsTrue(this TransformationResult result)
    {
        // First check if the result is null (treat this as a falsy value)
        if (string.IsNullOrWhiteSpace(result.Value))
        {
            return false;
        }

        // If the result is a boolean, return the value
        if (bool.TryParse(result.Value, out var boolValue))
        {
            return boolValue;
        }

        // If the result is a number, return false if the number is zero
        if (double.TryParse(result.Value, out var numberValue))
        {
            return numberValue != 0;
        }

        // All other values are treated as not true
        return false;
    }
}