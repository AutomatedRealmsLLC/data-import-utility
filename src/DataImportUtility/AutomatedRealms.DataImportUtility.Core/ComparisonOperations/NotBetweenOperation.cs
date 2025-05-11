using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is not between two other values.
/// </summary>
public class NotBetweenOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(NotBetweenOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Not between";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is not between two other values.";

    public int EnumMemberOrder { get; set; }

    /// <summary>
    /// The low limit (inclusive) for the comparison.
    /// </summary>
    public MappingRuleBase? LowLimit { get; set; }

    /// <summary>
    /// The high limit (inclusive) for the comparison.
    /// </summary>
    public MappingRuleBase? HighLimit { get; set; }

    /// <inheritdoc />
    public override async Task<TransformationResult<bool>> Evaluate(ITransformationContext context)
    {
        if (LeftOperand is null)
        {
            return TransformationResult<bool>.CreateFailure($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }

        if (LowLimit is null || HighLimit is null)
        {
            return TransformationResult<bool>.CreateFailure($"{nameof(LowLimit)} and {nameof(HighLimit)} must be set for {DisplayName} operation.");
        }

        var leftResult = await (LeftOperand?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));
        if (leftResult.WasFailure)
        {
            return TransformationResult<bool>.CreateFailure($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var lowLimitResult = await (LowLimit?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));
        if (lowLimitResult.WasFailure)
        {
            return TransformationResult<bool>.CreateFailure($"Failed to evaluate {nameof(LowLimit)} for {DisplayName} operation: {lowLimitResult.ErrorMessage}");
        }

        var highLimitResult = await (HighLimit?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));
        if (highLimitResult.WasFailure)
        {
            return TransformationResult<bool>.CreateFailure($"Failed to evaluate {nameof(HighLimit)} for {DisplayName} operation: {highLimitResult.ErrorMessage}");
        }

        return TransformationResult<bool>.CreateSuccess(leftResult.NotBetween(lowLimitResult, highLimitResult), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new NotBetweenOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(), // Though NotBetweenOperation doesn't use RightOperand directly, clone for base class consistency
            LowLimit = (MappingRuleBase?)LowLimit?.Clone(),
            HighLimit = (MappingRuleBase?)HighLimit?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
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
    public static bool NotBetween(this TransformationResult<string?> leftResult, TransformationResult<string?> lowLimitInclusive, TransformationResult<string?> highLimitInclusive)
        => !leftResult.Between(lowLimitInclusive, highLimitInclusive); // Assumes Between() extension method is available for TransformationResult<string?>
}
