using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.Rules;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Checks if a value is in a set of values.
/// </summary>
public class InOperation : ComparisonOperationBase
{
    /// <summary>
    /// The unique type identifier for this comparison operation.
    /// </summary>
    public static readonly string TypeIdString = "Core.InOperation";

    /// <inheritdoc />
    [JsonIgnore]
    public override string DisplayName { get; } = "In";

    /// <inheritdoc />
    [JsonIgnore]
    public override string Description { get; } = "Checks if a value is in a set of values.";

    /// <summary>
    /// The set of values to check against.
    /// These are rules that will be evaluated to get the actual values for comparison.
    /// </summary>
    public List<MappingRuleBase> Values { get; private set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="InOperation"/> class.
    /// </summary>
    public InOperation() : base(TypeIdString)
    {
    }

    /// <inheritdoc />
    public override void ConfigureOperands(
        MappingRuleBase leftOperand,
        MappingRuleBase? rightOperand, // Expected to be a StaticValueRule with a comma-separated list of values
        MappingRuleBase? secondaryRightOperand) // Not used by InOperation
    {
        LeftOperand = leftOperand ?? throw new ArgumentNullException(nameof(leftOperand), $"Left operand must be provided for {TypeIdString}.");
        RightOperand = rightOperand;
        HighLimit = secondaryRightOperand;

        if (rightOperand is StaticValueRule staticValueRule && staticValueRule.Value is not null && !string.IsNullOrWhiteSpace(staticValueRule.Value.ToString()))
        {
            var rawValue = staticValueRule.Value.ToString();
            var individualValues = (rawValue ?? string.Empty).Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));

            foreach (var valStr in individualValues)
            {
                // Use DisplayName from the left operand to create a more descriptive identifier for the generated rules.
                var namePart = LeftOperand?.DisplayName ?? "UnknownLeftOperand";
                // Create a unique identifier for the StaticValueRule using its value and the context from the left operand.
                var ruleIdentifier = $"InValue_{valStr}_for_{namePart.Replace(" ", "_")}";
                var valueItemRule = new StaticValueRule(valStr, ruleIdentifier);
                if (staticValueRule.ParentTableDefinition is not null)
                {
                    valueItemRule.ParentTableDefinition = staticValueRule.ParentTableDefinition;
                }
                Values.Add(valueItemRule);
            }
        }
        else if (rightOperand is not null && !(rightOperand is StaticValueRule && ((StaticValueRule)rightOperand).Value is null))
        {
            throw new InvalidOperationException($"For {TypeIdString}, if 'rightOperand' (ComparisonValue) is provided, it must be a StaticValueRule containing a non-empty, comma-separated string of values. Current rightOperand type: {rightOperand.GetType().Name}, Value: '{(rightOperand as StaticValueRule)?.Value?.ToString()}'.");
        }

        if (!Values.Any())
        {
            throw new InvalidOperationException($"The '{nameof(Values)}' collection must be populated for the {TypeIdString} operation. This typically comes from parsing the 'rightOperand' (ComparisonValue) as a comma-separated string.");
        }
    }

    /// <inheritdoc />
    public override async Task<bool> Evaluate(TransformationResult contextResult)
    {
        if (LeftOperand is null)
        {
            throw new InvalidOperationException($"LeftOperand must be set for {DisplayName} operation. Ensure ConfigureOperands was called.");
        }

        if (!Values.Any())
        {
            throw new InvalidOperationException($"'{nameof(Values)}' collection must not be empty for {DisplayName} operation. Ensure ConfigureOperands was called and processed valid input.");
        }

        var leftResult = await LeftOperand.Apply(contextResult);

        if (leftResult is null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }

        var valueResults = new List<TransformationResult>();
        foreach (var valueRule in Values)
        {
            var valueEvaluationResult = await valueRule.Apply(contextResult);
            if (valueEvaluationResult is null || valueEvaluationResult.WasFailure)
            {
                // Include DisplayName or TypeId in the error for better diagnostics.
                var ruleDesc = !string.IsNullOrEmpty(valueRule.DisplayName) ? valueRule.DisplayName : valueRule.TypeId;
                throw new InvalidOperationException($"Failed to evaluate a value in {nameof(Values)} for {DisplayName} operation (Rule: {ruleDesc}): {valueEvaluationResult?.ErrorMessage ?? "Result was null."}");
            }
            valueResults.Add(valueEvaluationResult);
        }

        return leftResult.In([.. valueResults]);
    }

    /// <inheritdoc />
    public override ComparisonOperationBase Clone()
    {
        var clone = (InOperation)MemberwiseClone();
        clone.TypeId = TypeId;

        clone.LeftOperand = LeftOperand?.Clone();
        clone.RightOperand = RightOperand?.Clone();
        clone.HighLimit = HighLimit?.Clone();

        clone.Values = [.. Values.Select(v => v.Clone())];
        return clone;
    }
}

/// <summary>
/// Extension methods for the In operation.
/// </summary>
public static class InOperationExtensions
{
    /// <summary>
    /// Checks if the left result is in the set of values.
    /// </summary>
    /// <param name="leftResult">The result of the left operand.</param>
    /// <param name="values">The set of values to check for.</param>
    /// <returns>True if the left result is in the set of values; otherwise, false.</returns>
    public static bool In(this TransformationResult leftResult, params TransformationResult[] values)
    {
        return leftResult.CurrentValue is null
            ? values.Any(val => val.CurrentValue is null)
            : values.Any(val => Equals(leftResult.CurrentValue, val.CurrentValue));
    }
}
