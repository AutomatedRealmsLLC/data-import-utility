using System.Text.Json.Serialization;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Operation to check if a value is not null or empty.
/// </summary>
public class IsNotNullOrEmptyOperation : ComparisonOperationBase
{
    /// <summary>
    /// Static TypeId for this operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.IsNotNullOrEmpty";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "Is Not Null or Empty";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if the input value is not null or an empty string.";

    /// <summary>
    /// Initializes a new instance of the <see cref="IsNotNullOrEmptyOperation"/> class.
    /// </summary>
    public IsNotNullOrEmptyOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(MappingRuleBase? leftOperand, MappingRuleBase? rightOperand = null, MappingRuleBase? secondaryRightOperand = null)
    {
        if (leftOperand == null)
        {
            throw new ArgumentNullException(nameof(leftOperand), $"Left operand cannot be null for {DisplayName}.");
        }
        LeftOperand = leftOperand;
        // RightOperand and SecondaryRightOperand are not used by this operation.
    }

    /// <summary>
    /// Evaluates whether the left operand's value is not null or empty.
    /// </summary>
    /// <param name="contextResult">The transformation context.</param>
    /// <returns>True if the value is not null or empty, otherwise false.</returns>
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        ITransformationContext? context = contextResult as ITransformationContext;
        if (context == null)
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

        if (LeftOperand == null)
        {
            throw new InvalidOperationException($"{nameof(LeftOperand)} must be configured for {DisplayName} operation.");
        }

        var leftValueResult = await LeftOperand.Apply(context);

        if (leftValueResult == null || leftValueResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftValueResult?.ErrorMessage ?? "Result was null."}");
        }

        object? leftValue = leftValueResult.CurrentValue;

        if (leftValue == null)
        {
            return false; // Null is considered null or empty
        }

        if (leftValue is string stringValue)
        {
            return !string.IsNullOrEmpty(stringValue);
        }

        // If it's not a string and not null, it is considered not null or empty.
        return true;
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = new IsNotNullOrEmptyOperation();
        clone.LeftOperand = LeftOperand?.Clone();
        // RightOperand is not used by this operation.
        return clone;
    }
}
