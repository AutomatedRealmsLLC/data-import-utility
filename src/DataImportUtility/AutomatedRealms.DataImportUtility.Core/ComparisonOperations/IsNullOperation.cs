using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

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

    /// <summary>
    /// Gets or sets the order of this operation if it's part of an enumerated list of operations.
    /// This property might be used for sorting or specific ordering logic if applicable.
    /// </summary>
    public int EnumMemberOrder { get; set; }

    /// <summary>
    /// Evaluates whether the left operand's value is null.
    /// </summary>
    /// <param name="contextResult">The transformation context.</param>
    /// <returns>True if the value is null, otherwise false.</returns>
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be set for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);

        if (leftResult == null)
        {
            throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned null unexpectedly.");
        }

        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        return leftResult.CurrentValue is null;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        return new IsNullOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone(),
            EnumMemberOrder = EnumMemberOrder
        };
    }
}
