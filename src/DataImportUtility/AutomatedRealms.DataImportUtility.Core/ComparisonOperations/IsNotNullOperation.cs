using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is not null.
/// </summary>
public class IsNotNullOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(IsNotNullOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is not null";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is not null.";

    public int EnumMemberOrder { get; set; }

    /// <inheritdoc />
    public override async Task<TransformationResult<bool>> Evaluate(ITransformationContext context)
    {
        if (LeftOperand is null)
        {
            return TransformationResult<bool>.CreateFailure($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(context);
        if (leftResult.WasFailure)
        {
            return TransformationResult<bool>.CreateFailure($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        return TransformationResult<bool>.CreateSuccess(leftResult.IsNotNull(), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new IsNotNullOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(), // Cloning RightOperand for consistency with base class
            EnumMemberOrder = EnumMemberOrder
        };
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
    public static bool IsNotNull(this TransformationResult<string?> result)
        => !result.IsNull(); // Assumes IsNull() extension method is also updated/available for TransformationResult<string?>
}
