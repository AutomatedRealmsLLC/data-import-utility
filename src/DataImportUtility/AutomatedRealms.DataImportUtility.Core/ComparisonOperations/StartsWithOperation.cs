using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

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
    public override void ConfigureOperands(MappingRuleBase? leftOperand, MappingRuleBase? rightOperand, MappingRuleBase? _ = null)
    {
        if (leftOperand is null)
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand cannot be null for {DisplayName}.");
        }
        if (rightOperand is null)
        {
            throw new ArgumentNullException(nameof(rightOperand), $"Right operand cannot be null for {DisplayName}.");
        }
        LeftOperand = leftOperand;
        RightOperand = rightOperand;
        // SecondaryRightOperand is not used by this operation.
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (contextResult is not ITransformationContext context)
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
        if (leftResult is null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }

        var rightResult = await RightOperand.Apply(context);
        if (rightResult is null || rightResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult?.ErrorMessage ?? "Result was null."}");
        }

        var leftValue = leftResult.CurrentValue;
        var rightValue = rightResult.CurrentValue;

        if (leftValue is null || rightValue is null)
        {
            if (rightValue is null || (rightValue is string rs && string.IsNullOrEmpty(rs))) return true;
            if (leftValue is null) return false;
        }

        return leftValue is string leftString
            && rightValue is string rightString
            && leftString.StartsWith(rightString, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new StartsWithOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone()
        };
        return clone;
    }
}
