using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models; // For TransformationResult
using System.Text.Json.Serialization; // For JsonIgnore

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is between two other values.
/// </summary>
public class BetweenOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 0;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(BetweenOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Between";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is between two other values.";

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
            // Consider logging an error or returning a specific result instead of throwing.
            // For now, keeping original behavior but this could be improved for robustness.
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {nameof(BetweenOperation)}.");
        }

        if (LowLimit is null || HighLimit is null)
        {
            throw new InvalidOperationException($"{nameof(LowLimit)} and {nameof(HighLimit)} must be set for {nameof(BetweenOperation)}.");
        }

        var leftResult = await LeftOperand.Apply(result);
        if (leftResult.WasFailure)
        {
            // Log error or handle: result.ErrorMessage = $"Failed to evaluate {nameof(LeftOperand)}..."
            // For now, throw to maintain original behavior.
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

        return leftResult.Between(lowLimitResult, highLimitResult);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = (BetweenOperation)MemberwiseClone();
        clone.LeftOperand = LeftOperand?.Clone();
        // RightOperand is not used by BetweenOperation directly, but clone if present from base
        clone.RightOperand = RightOperand?.Clone();
        clone.LowLimit = LowLimit?.Clone();
        clone.HighLimit = HighLimit?.Clone();
        clone.ExpectedValue = ExpectedValue; // string, shallow copy is fine
        return clone;
    }
}

/// <summary>
/// Extension methods for the Between operation.
/// </summary>
public static class BetweenOperationExtensions
{
    /// <summary>
    /// Checks if the left result is between the low and high limits (inclusive).
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="lowLimitInclusive">The low limit (inclusive).</param>
    /// <param name="highLimitInclusive">The high limit (inclusive).</param>
    /// <returns>True if the left result is between the low and high limits (inclusive); otherwise, false.</returns>
    public static bool Between(this TransformationResult leftResult, TransformationResult lowLimitInclusive, TransformationResult highLimitInclusive)
    {
        // Handle null cases
        if (leftResult.Value is null) { return false; }

        // Try to parse as numbers for numeric comparison
        if (double.TryParse(leftResult.Value, out var leftValue)
            && double.TryParse(lowLimitInclusive.Value, out var lowValue)
            && double.TryParse(highLimitInclusive.Value, out var highValue))
        {
            return leftValue >= lowValue && leftValue <= highValue;
        }

        // For dates
        if (DateTime.TryParse(leftResult.Value, out var leftDate)
            && DateTime.TryParse(lowLimitInclusive.Value, out var lowDate)
            && DateTime.TryParse(highLimitInclusive.Value, out var highDate))
        {
            return leftDate >= lowDate && leftDate <= highDate;
        }

        // Fall back to string comparison
        var leftStr = leftResult.Value;
        var lowStr = lowLimitInclusive.Value ?? string.Empty;
        var highStr = highLimitInclusive.Value ?? string.Empty;

        return string.Compare(leftStr, lowStr, StringComparison.Ordinal) >= 0
            && string.Compare(leftStr, highStr, StringComparison.Ordinal) <= 0;
    }
}
