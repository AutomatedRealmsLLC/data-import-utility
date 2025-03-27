using DataImportUtility.Abstractions;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value contains another value.
/// </summary>
public class ContainsOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(ContainsOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Contains";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value contains another value.";

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
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var rightResult = await RightOperand.Apply(result);
        if (rightResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult.ErrorMessage}");
        }

        return leftResult.Contains(rightResult);
    }
}

/// <summary>
/// Extension methods for the Contains operation.
/// </summary>
public static class ContainsOperationExtensions
{
    /// <summary>
    /// Checks if the left result contains the value.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the left result contains the value; otherwise, false.</returns>
    /// <remarks>
    /// If the input value is an array, it will check if the left result contains the
    /// string value from the value TransformationResult.<br />
    /// <br />
    /// If the input value is a single string, it will check if the left result contains the
    /// string value from the value TransformationResult.
    /// </remarks>
    public static bool Contains(this TransformationResult leftResult, TransformationResult value)
    {
        // Handle null cases
        if (leftResult.Value is null || value.Value is null) { return false; }

        // If the left result is a JSON array
        if (leftResult.IsJsonArray())
        {
            try
            {
                var values = leftResult.ResultValueAsArray();
                return values?.Contains(value.Value) ?? false;
            }
            catch
            {
                // If we can't parse the JSON, fall back to string contains
            }
        }

        // Default string contains check
        return leftResult.Value.Contains(value.Value);
    }
}
