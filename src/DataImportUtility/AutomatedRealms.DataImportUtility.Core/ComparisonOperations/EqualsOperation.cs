using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if two values are equal.
/// </summary>
public class EqualsOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 3;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(EqualsOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Equals";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if two values are equal.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null && RightOperand is null)
        {
            // If both operands are null, they are considered equal in this context.
            // Or, if ExpectedValue is also null, it's true.
            // The original logic returned true.
            return true;
        }

        // Apply the left operand's rule to get its value
        var leftResult = await (LeftOperand?.Apply(result) ?? Task.FromResult(TransformationResult.CreateSuccess(null, result.Record, result.TableDefinition)));
        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate left operand for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        // Apply the right operand's rule to get its value
        var rightResult = await (RightOperand?.Apply(result) ?? Task.FromResult(TransformationResult.CreateSuccess(null, result.Record, result.TableDefinition)));
        if (rightResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate right operand for {DisplayName} operation: {rightResult.ErrorMessage}");
        }

        return leftResult.IsEqualTo(rightResult);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = (EqualsOperation)MemberwiseClone();
        clone.LeftOperand = LeftOperand?.Clone();
        clone.RightOperand = RightOperand?.Clone();
        clone.ExpectedValue = ExpectedValue; // string, shallow copy is fine
        return clone;
    }
}

/// <summary>
/// Extension methods for the IsEqualTo operation.
/// </summary>
public static class EqualsOperationExtensions
{
    /// <summary>
    /// Checks if the left result equals the right result.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="rightResult">The result of the right operand.</param>
    /// <returns>True if the left result equals the right result; otherwise, false.</returns>
    public static bool IsEqualTo(this TransformationResult leftResult, TransformationResult rightResult)
        => leftResult.Value == rightResult.Value;
}
