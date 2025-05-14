using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is not null.
/// </summary>
public class IsNotNullOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.IsNotNull";

    // EnumMemberName and EnumMemberOrder removed

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is not null";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is not null.";

    /// <summary>
    /// Initializes a new instance of the <see cref="IsNotNullOperation"/> class.
    /// </summary>
    public IsNotNullOperation() : base(TypeIdString) // Pass TypeIdString to base constructor
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(
        MappingRuleBase leftOperand,
        MappingRuleBase? rightOperand, // Not used by IsNotNullOperation
        MappingRuleBase? secondaryRightOperand) // Not used by IsNotNullOperation
    {
        // Call base to set LeftOperand. RightOperand and HighLimit will be set to null or the provided values,
        // but IsNotNullOperation only uses LeftOperand.
        base.ConfigureOperands(leftOperand, null, null);

        if (this.LeftOperand is null) // Validation after base call
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand must be provided for {TypeIdString}.");
        }
        // RightOperand and secondaryRightOperand are not used by IsNotNullOperation.
        // No specific validation for them is needed here beyond what the base class does.
    }

    /// <summary>
    /// Evaluates whether the left operand's value is not null.
    /// </summary>
    /// <param name="contextResult">The transformation context.</param>
    /// <returns>True if the value is not null, otherwise false.</returns>
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null) // Should be caught by ConfigureOperands
        {
            throw new InvalidOperationException($"LeftOperand must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);

        if (leftResult == null)
        {
            throw new InvalidOperationException($"Applying {nameof(LeftOperand)} for {DisplayName} operation returned a null TransformationResult, indicating an issue with the operand rule itself.");
        }

        if (leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult.ErrorMessage}");
        }

        // If we reach here, leftResult is not null and WasFailure is false.
        // Now we check the actual CurrentValue of the successful transformation.
        return leftResult.CurrentValue != null;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        // Base Clone() handles LeftOperand, RightOperand, HighLimit and MemberwiseClone.
        // TypeId is set by the constructor.
        return (IsNotNullOperation)base.Clone();
    }
}

// Extension method class IsNotNullOperationExtensions removed as its logic is now inlined in Evaluate.
