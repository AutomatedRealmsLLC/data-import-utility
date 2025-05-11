using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if two values are not equal.
/// </summary>
public class NotEqualOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(NotEqualOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Does not equal";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if two values are not equal.";

    public int EnumMemberOrder { get; set; }

    /// <inheritdoc />
    public override async Task<TransformationResult<bool>> Evaluate(ITransformationContext context)
    {
        // If both are null, they are considered equal, so NotEqual is false.
        // If only one is null, they are not equal, so NotEqual is true.
        // This logic is handled by the IsNotEqualTo extension, which relies on the Equals extension.

        var leftResult = await (LeftOperand?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));
        // No need to check leftResult.WasFailure here as null is a valid outcome for comparison.

        var rightResult = await (RightOperand?.Apply(context) ?? Task.FromResult(TransformationResult.CreateSuccess<string?>(null, context)));
        // No need to check rightResult.WasFailure here.

        // The original logic for `if (LeftOperand is null && RightOperand is null)` returning true for NotEqual seems counterintuitive.
        // Standard interpretation is null == null, thus null != null is false.
        // The IsNotEqualTo extension (which calls Equals) should handle this correctly based on standard equality interpretation.
        // If LeftOperand and RightOperand are both null, their .Apply will result in TransformationResults with Value = null.
        // The Equals extension should treat (null, null) as true, so IsNotEqualTo(null,null) will be false.

        return TransformationResult<bool>.CreateSuccess(leftResult.IsNotEqualTo(rightResult), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new NotEqualOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
    }
}

/// <summary>
/// Extension methods for the IsNotEqualTo operation.
/// </summary>
public static class NotEqualOperationExtensions
{
    /// <summary>
    /// Checks if the left result does not equal the right result.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="rightResult">The result of the right operand.</param>
    /// <returns>True if the left result does not equal the right result; otherwise, false.</returns>
    public static bool IsNotEqualTo(this TransformationResult<string?> leftResult, TransformationResult<string?> rightResult)
        => !leftResult.IsEqualTo(rightResult); // Assumes IsEqualTo extension method is available and correctly handles nulls for TransformationResult<string?>
}
