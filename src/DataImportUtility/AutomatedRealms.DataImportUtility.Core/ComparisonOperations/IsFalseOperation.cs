using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is false.
/// </summary>
public class IsFalseOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(IsFalseOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is false";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is false.";

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

        return TransformationResult<bool>.CreateSuccess(leftResult.IsFalse(), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new IsFalseOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(), // Cloning RightOperand for consistency with base class
            EnumMemberOrder = EnumMemberOrder
        };
    }
}

/// <summary>
/// Extension methods for the IsFalse operation.
/// </summary>
public static class IsFalseOperationExtensions
{
    /// <summary>
    /// Checks if the result is false.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is false; otherwise, false.</returns>
    /// <remarks>
    /// A result is considered false if it is null, an empty string,
    /// or a falsy value (zero stored as a string, the string "false").
    /// Everything else is considered not false.
    /// </remarks>
    public static bool IsFalse(this TransformationResult<string?> result)
        => !result.IsTrue(); // Assumes IsTrue() extension method is also updated/available for TransformationResult<string?>
}
