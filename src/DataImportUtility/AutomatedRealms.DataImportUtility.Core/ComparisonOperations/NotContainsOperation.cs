using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value does not contain another value.
/// </summary>
public class NotContainsOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(NotContainsOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Does not contain";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value does not contain another value.";

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

        return TransformationResult<bool>.CreateSuccess(leftResult.NotContains(rightResult), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new NotContainsOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
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
    public static bool NotContains(this TransformationResult<string?> leftResult, TransformationResult<string?> value)
        => !leftResult.Contains(value); // Assumes Contains() extension method is available for TransformationResult<string?>
}
