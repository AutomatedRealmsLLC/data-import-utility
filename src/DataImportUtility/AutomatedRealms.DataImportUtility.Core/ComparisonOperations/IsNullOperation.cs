using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is null.
/// </summary>
public class IsNullOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(IsNullOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is null";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is null.";

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

        return TransformationResult<bool>.CreateSuccess(leftResult.IsNull(), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new IsNullOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(), // Cloning RightOperand for consistency with base class
            EnumMemberOrder = EnumMemberOrder
        };
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
    public static bool IsNull(this TransformationResult<string?> result)
        => result.Value is null;
}
