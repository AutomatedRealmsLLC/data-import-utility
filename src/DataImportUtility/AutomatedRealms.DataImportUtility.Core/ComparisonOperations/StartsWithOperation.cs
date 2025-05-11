using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value starts with another value.
/// </summary>
public class StartsWithOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(StartsWithOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Starts with";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value starts with another value.";

    public int EnumMemberOrder { get; set; }

    /// <inheritdoc />
    public override async Task<TransformationResult<bool>> Evaluate(ITransformationContext context)
    {
        if (LeftOperand is null || RightOperand is null)
        {
            return TransformationResult<bool>.CreateFailure($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set for {DisplayName} operation.");
        }

        var leftResult = await (LeftOperand?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));
        if (leftResult.WasFailure)
        {
            return TransformationResult<bool>.CreateFailure($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var rightResult = await (RightOperand?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));
        if (rightResult.WasFailure)
        {
            return TransformationResult<bool>.CreateFailure($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult.ErrorMessage}");
        }

        return TransformationResult<bool>.CreateSuccess(leftResult.StartsWith(rightResult), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new StartsWithOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
    }
}

/// <summary>
/// Extension methods for the StartsWith operation.
/// </summary>
public static class StartsWithOperationExtensions
{
    /// <summary>
    /// Checks if the left result starts with the value.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the left result starts with the value; otherwise, false.</returns>
    public static bool StartsWith(this TransformationResult<string?> leftResult, TransformationResult<string?> value)
    {
        // Handle null cases
        if (leftResult.Value is null || value.Value is null) { return false; }

        return leftResult.Value.StartsWith(value.Value, StringComparison.Ordinal);
    }
}
