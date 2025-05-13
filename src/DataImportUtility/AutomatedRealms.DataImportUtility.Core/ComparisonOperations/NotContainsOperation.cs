using System.Collections; // For IEnumerable
using System.Text.Json.Serialization;

using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult and ITransformationContext

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value does not contain another value.
/// </summary>
public class NotContainsOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.NotContains";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Does not contain";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value does not contain another value (case-insensitive).";

    /// <summary>
    /// Initializes a new instance of the <see cref="NotContainsOperation"/> class.
    /// </summary>
    public NotContainsOperation() : base(TypeIdString)
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
    public override async Task<bool> Evaluate(TransformationResult contextResult) // contextResult is an ITransformationContext
    {
        ITransformationContext? transformationContext = contextResult as ITransformationContext;
        if (transformationContext == null)
        {
            // If contextResult is not directly an ITransformationContext,
            // and it doesn't carry one, this will be an issue.
            // For now, proceeding with the assumption based on prior comments that it is.
            // If TransformationResult has a property like .Context, that should be used.
            // Based on the error "NotContainsOperation' does not implement inherited abstract member 'ComparisonOperationBase.Evaluate(TransformationResult)'"
            // this signature is correct. The issue is how to get ITransformationContext.
            // Let's assume for now that TransformationResult itself implements ITransformationContext
            // or this specific contextResult instance passed in will be one.
             throw new InvalidOperationException($"The provided contextResult could not be interpreted as an {nameof(ITransformationContext)} for {DisplayName} operation. Actual type: {contextResult?.GetType().FullName}");
        }

        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be configured for {DisplayName} operation.");
        }
        if (RightOperand is null)
        {
            throw new InvalidOperationException($"{nameof(RightOperand)} must be configured for {DisplayName} operation.");
        }

        var leftResult = await LeftOperand.Apply(transformationContext);
        var rightResult = await RightOperand.Apply(transformationContext);

        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }
        if (rightResult == null || rightResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(RightOperand)} for {DisplayName} operation: {rightResult?.ErrorMessage ?? "Result was null."}");
        }

        object? leftValue = leftResult.CurrentValue;
        object? rightValue = rightResult.CurrentValue;

        if (leftValue == null)
        {
            // null does not contain anything, unless rightValue is also null.
            // If rightValue is null, previous Contains logic said true (everything contains null).
            // So, !(null contains null) = !true = false.
            // If rightValue is not null, previous Contains logic said false (null does not contain non-null).
            // So, !(null contains non-null) = !false = true.
            return rightValue != null;
        }

        if (rightValue == null)
        {
            // Previous Contains logic: everything contains null (empty string) -> true.
            // So, NotContains null is false.
            return false; 
        }

        string leftString = leftValue.ToString() ?? ""; // Ensure not null for operations
        string rightString = rightValue.ToString() ?? "";

        // Handle if leftValue is a collection (e.g., string array from a multi-select or split)
        if (leftValue is IEnumerable enumerable && !(leftValue is string))
        {
            foreach (var item in enumerable)
            {
                if (item?.ToString()?.IndexOf(rightString, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false; // Found a match in the collection, so "NotContains" is false.
                }
            }
            return true; // No match in the collection, so "NotContains" is true.
        }
        
        // Standard string contains check (case-insensitive)
        return leftString.IndexOf(rightString, StringComparison.OrdinalIgnoreCase) < 0;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new NotContainsOperation();
        clone.LeftOperand = LeftOperand?.Clone(); 
        clone.RightOperand = RightOperand?.Clone();
        return clone;
    }
}
