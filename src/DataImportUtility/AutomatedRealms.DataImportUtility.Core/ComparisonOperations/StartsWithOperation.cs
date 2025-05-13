using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult and ITransformationContext

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value starts with another value.
/// </summary>
public class StartsWithOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.StartsWith";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Starts with";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value starts with another value (case-sensitive by default).";

    /// <summary>
    /// Initializes a new instance of the <see cref="StartsWithOperation"/> class.
    /// </summary>
    public StartsWithOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(MappingRuleBase? leftOperand, MappingRuleBase? rightOperand, MappingRuleBase? secondaryRightOperand = null)
    {
        if (leftOperand == null)
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand cannot be null for {DisplayName}.");
        }
        if (rightOperand == null)
        {
            throw new ArgumentNullException(nameof(rightOperand), $"Right operand cannot be null for {DisplayName}.");
        }
        LeftOperand = leftOperand;
        RightOperand = rightOperand;
        // SecondaryRightOperand is not used by this operation.
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult) // Corrected signature
    {
        ITransformationContext? context = contextResult as ITransformationContext;
        if (context == null)
        {
            // This attempts to use TransformationResult directly as context if it's not already one.
            // This relies on TransformationResult implementing ITransformationContext or being adaptable.
            // If TransformationResult has a specific property for the context, that should be used.
            // For now, we assume TransformationResult can be treated as or provide an ITransformationContext.
            if (contextResult is ITransformationContext directContext)
            {
                context = directContext;
            }
            else
            {
                 throw new InvalidOperationException($"The provided contextResult (type: {contextResult?.GetType().FullName}) could not be interpreted as an {nameof(ITransformationContext)} for {DisplayName} operation.");
            }
        }

        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be configured for {DisplayName} operation.");
        }
        if (RightOperand is null)
        {
            throw new InvalidOperationException($"{nameof(RightOperand)} must be configured for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(context);
        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }

        var rightResult = await RightOperand.Apply(context);
        if (rightResult == null || rightResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult?.ErrorMessage ?? "Result was null."}");
        }
        
        object? leftValue = leftResult.CurrentValue;
        object? rightValue = rightResult.CurrentValue;

        if (leftValue == null || rightValue == null)
        {
            if (rightValue == null || (rightValue is string rs && string.IsNullOrEmpty(rs))) return true; 
            if (leftValue == null) return false; 
        }

        if (leftValue is string leftString && rightValue is string rightString)
        {
            return leftString.StartsWith(rightString, StringComparison.Ordinal);
        }
        
        return false;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new StartsWithOperation();
        clone.LeftOperand = LeftOperand?.Clone();
        clone.RightOperand = RightOperand?.Clone();
        return clone;
    }
}
