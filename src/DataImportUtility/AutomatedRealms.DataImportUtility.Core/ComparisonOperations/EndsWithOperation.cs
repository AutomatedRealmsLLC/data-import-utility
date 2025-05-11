using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.Models;
using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value ends with another value.
/// </summary>
public class EndsWithOperation : ComparisonOperationBase
{
    /// <inheritdoc />
    public override int EnumMemberOrder { get; } = 2;

    /// <inheritdoc />
    public override string EnumMemberName { get; } = nameof(EndsWithOperation);

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Ends with";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value ends with another value.";

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult result)
    {
        if (LeftOperand is null || RightOperand is null)
        {
            throw new InvalidOperationException($"Both {nameof(LeftOperand)} and {nameof(RightOperand)} must be set.");
        }

        var leftResult = await LeftOperand.Apply(result);
        if (leftResult.WasFailure)
        {
            // Consider using leftResult.ErrorMessage
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        var rightResult = await RightOperand.Apply(result);
        if (rightResult.WasFailure)
        {
            // Consider using rightResult.ErrorMessage
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult.ErrorMessage}");
        }

        return leftResult.EndsWith(rightResult);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = (EndsWithOperation)MemberwiseClone();
        clone.LeftOperand = LeftOperand?.Clone();
        clone.RightOperand = RightOperand?.Clone();
        clone.ExpectedValue = ExpectedValue; // string, shallow copy is fine
        return clone;
    }
}

/// <summary>
/// Extension methods for the EndsWith operation.
/// </summary>
public static class EndsWithOperationExtensions
{
    /// <summary>
    /// Checks if the left result ends with the value.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if the left result ends with the value; otherwise, false.</returns>
    public static bool EndsWith(this TransformationResult leftResult, TransformationResult value)
    {
        // Handle null cases
        if (leftResult.Value is null || value.Value is null) { return false; }

        return leftResult.Value.EndsWith(value.Value, StringComparison.Ordinal);
    }
}
