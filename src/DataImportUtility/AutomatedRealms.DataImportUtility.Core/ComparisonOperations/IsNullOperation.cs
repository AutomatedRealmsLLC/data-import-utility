using System.Text.Json.Serialization;
using System.Threading.Tasks; // Required for Task
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

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

    // EnumMemberName and EnumMemberOrder removed

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is null";

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

        if (this.LeftOperand is null) // Validation after base call
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

        var leftResult = await LeftOperand.Apply(contextResult);

        // It's possible for Apply to return a null TransformationResult if the rule itself is fundamentally broken
        // or if the context passed in is such that it cannot operate.
        if (leftResult == null) 
        {
            // This indicates a problem with the LeftOperand rule execution itself, not necessarily that its *value* is null.
            // For IsNullOperation, if the rule execution fails to produce a result, we might consider it as not being a determinable value,
            // which is distinct from the value *being* null. Depending on strictness, this could be an error or false.
            // Let's treat a failure to evaluate the operand as an error.
            throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned a null TransformationResult, indicating an issue with the operand rule itself.");
        }

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
