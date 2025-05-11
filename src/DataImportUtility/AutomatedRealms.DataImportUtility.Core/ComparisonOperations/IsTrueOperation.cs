using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is true.
/// </summary>
public class IsTrueOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    [JsonIgnore]
    public override string EnumMemberName { get; } = nameof(IsTrueOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is true";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is true.";

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

        return TransformationResult<bool>.CreateSuccess(leftResult.IsTrue(), context);
    }

    /// <inheritdoc />
    public override IComparisonOperation Clone()
    {
        return new IsTrueOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(), // Cloning RightOperand for consistency with base class
            EnumMemberOrder = EnumMemberOrder
        };
    }
}

/// <summary>
/// Extension methods for the IsTrue operation.
/// </summary>
public static class IsTrueOperationExtensions
{
    /// <summary>
    /// Checks if the result is true.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is true; otherwise, false.</returns>
    /// <remarks>
    /// A result is considered true if it not null and
    /// a truthy value (non-zero numbers stored as a string, the string "true").
    /// Everything else is considered not true.
    /// </remarks>
    public static bool IsTrue(this TransformationResult<string?> result)
    {
        // First check if the result is null (treat this as a falsy value)
        if (string.IsNullOrWhiteSpace(result.Value))
        {
            return false;
        }

        // If the result is a boolean, return the value
        if (bool.TryParse(result.Value, out var boolValue))
        {
            return boolValue;
        }

        // If the result is a number, return false if the number is zero
        if (double.TryParse(result.Value, out var numberValue))
        {
            return numberValue != 0;
        }

        // All other values are treated as not true
        return false;
    }
}
