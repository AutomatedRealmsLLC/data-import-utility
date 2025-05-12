using DataImportUtility.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.ComparisonOperations;

/// <summary>
/// Checks if a value is not between two other values.
/// </summary>
public class NotBetweenOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(NotBetweenOperation);

    /// <inheritdoc />
    public override string DisplayName { get; } = "Not between";

    /// <inheritdoc />
    public override string Description { get; } = "Checks if a value is not between two other values.";

    /// <summary>
    /// The low limit (inclusive) for the comparison.
    /// </summary>
    public MappingRuleBase? LowLimit { get; set; }

    /// <summary>
    /// The high limit (inclusive) for the comparison.
    /// </summary>
    public MappingRuleBase? HighLimit { get; set; }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {nameof(NotBetweenOperation)}.");
        }

        if (LowLimit is null || HighLimit is null)
        {
            throw new InvalidOperationException($"{nameof(LowLimit)} and {nameof(HighLimit)} must be set for {nameof(NotBetweenOperation)}.");
        }

        var leftResult = await LeftOperand.Apply(result);
        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var lowLimitResult = await LowLimit.Apply(result);
        if (lowLimitResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LowLimit)} for {DisplayName} operation: {lowLimitResult.ErrorMessage}");
        }

        var highLimitResult = await HighLimit.Apply(result);
        if (highLimitResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(HighLimit)} for {DisplayName} operation: {highLimitResult.ErrorMessage}");
        }

        return leftResult.NotBetween(lowLimitResult, highLimitResult);
    }
}

/// <summary>
/// Extension methods for the NotBetween operation.
/// </summary>
public static class NotBetweenOperationExtensions
{
    /// <summary>
    /// Checks if the left result is not between the low and high limits (inclusive).
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="lowLimitInclusive">The low limit (inclusive).</param>
    /// <param name="highLimitInclusive">The high limit (inclusive).</param>
    /// <returns>True if the left result is not between the low and high limits (inclusive); otherwise, false.</returns>
    public static bool NotBetween(this TransformationResult leftResult, TransformationResult lowLimitInclusive, TransformationResult highLimitInclusive)
        => !leftResult.Between(lowLimitInclusive, highLimitInclusive);
}