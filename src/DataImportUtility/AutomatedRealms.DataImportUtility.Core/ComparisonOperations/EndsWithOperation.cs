using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult and ITransformationContext

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a string value ends with another string value.
/// </summary>
public class EndsWithOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.EndsWith";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Ends With";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a string value ends with another string value (case-insensitive by default).";

    /// <summary>
    /// Initializes a new instance of the <see cref="EndsWithOperation"/> class.
    /// </summary>
    public EndsWithOperation() : base(TypeIdString)
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
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (contextResult is not ITransformationContext context)
        {
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
            // If right is null or empty, any string (including null represented as empty) "ends with" it.
            if (rightValue == null || (rightValue is string rs && string.IsNullOrEmpty(rs))) return true;
            // If left is null and right is not null/empty, then it's false.
            if (leftValue == null) return false;
        }

        if (leftValue is string leftString && rightValue is string rightString)
        {
            // Defaulting to OrdinalIgnoreCase. Consider adding a property for StringComparison.
            return leftString.EndsWith(rightString, StringComparison.OrdinalIgnoreCase);
        }

        return false; // Operands are not strings or one is null and the other isn't (and not empty for rightValue)
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new EndsWithOperation
        {
            LeftOperand = LeftOperand?.Clone(),
            RightOperand = RightOperand?.Clone()
        };
        return clone;
    }
}
