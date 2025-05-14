using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is null.
/// </summary>
public class IsNullOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.IsNull";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "is null";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is null.";

    /// <summary>
    /// Initializes a new instance of the <see cref="IsNullOperation"/> class.
    /// </summary>
    public IsNullOperation() : base(TypeIdString) // Pass TypeIdString to base constructor
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(
        MappingRuleBase leftOperand,
        MappingRuleBase? rightOperand, // Not used by IsNullOperation
        MappingRuleBase? secondaryRightOperand) // Not used by IsNullOperation
    {
        // Call base to set LeftOperand. RightOperand and HighLimit will be set to null or the provided values,
        // but IsNullOperation only uses LeftOperand.
        base.ConfigureOperands(leftOperand, null, null);

        if (LeftOperand is null) // Validation after base call
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand must be provided for {TypeIdString}.");
        }
        // RightOperand and secondaryRightOperand are not used by IsNullOperation.
        // No specific validation for them is needed here beyond what the base class does.
    }

    /// <summary>
    /// Evaluates whether the left operand's value is null.
    /// </summary>
    /// <param name="contextResult">The transformation context.</param>
    /// <returns>True if the value is null, otherwise false.</returns>
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null) // Should be caught by ConfigureOperands
        {
            throw new InvalidOperationException($"LeftOperand must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }

        var leftResult = await LeftOperand.Apply(contextResult) ?? throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned a null TransformationResult, indicating an issue with the operand rule itself.");
        if (leftResult.WasFailure)
        {
            // The operand rule executed but reported a failure (e.g., type conversion error, etc.)
            // Similar to the above, this is an evaluation failure, not a successful evaluation to a null value.
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        // If we reach here, leftResult is not null and WasFailure is false.
        // Now we check the actual CurrentValue of the successful transformation.
        return leftResult.CurrentValue is null;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        // Base Clone() handles LeftOperand, RightOperand, HighLimit and MemberwiseClone.
        // TypeId is set by the constructor.
        return (IsNullOperation)base.Clone();
    }
}
