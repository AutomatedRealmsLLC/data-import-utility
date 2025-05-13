using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For TransformationResult

using AutomatedRealms.DataImportUtility.Core.Rules; // For StaticValueRule

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
    public List<MappingRuleBase> Values { get; private set; } = new List<MappingRuleBase>();

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
        this.LeftOperand = leftOperand ?? throw new ArgumentNullException(nameof(leftOperand), $"Left operand must be provided for {TypeIdString}.");
        this.RightOperand = rightOperand;
        this.HighLimit = secondaryRightOperand; 

        if (rightOperand is StaticValueRule staticValueRule && staticValueRule.Value != null && !string.IsNullOrWhiteSpace(staticValueRule.Value.ToString()))
        {
            var rawValue = staticValueRule.Value.ToString();
            var individualValues = (rawValue ?? string.Empty).Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
            
            foreach (var valStr in individualValues)
            {
                // Use DisplayName from the left operand to create a more descriptive identifier for the generated rules.
                var namePart = this.LeftOperand?.DisplayName ?? "UnknownLeftOperand";
                // Create a unique identifier for the StaticValueRule using its value and the context from the left operand.
                var ruleIdentifier = $"InValue_{valStr}_for_{namePart.Replace(" ", "_")}";
                var valueItemRule = new StaticValueRule(valStr, ruleIdentifier);
                if (staticValueRule.ParentTableDefinition != null)
                {
                    valueItemRule.ParentTableDefinition = staticValueRule.ParentTableDefinition;
                }
                this.Values.Add(valueItemRule);
            }
        }
        else if (rightOperand != null && !(rightOperand is StaticValueRule && ((StaticValueRule)rightOperand).Value == null))
        {
            throw new InvalidOperationException($"For {TypeIdString}, if 'rightOperand' (ComparisonValue) is provided, it must be a StaticValueRule containing a non-empty, comma-separated string of values. Current rightOperand type: {rightOperand.GetType().Name}, Value: '{(rightOperand as StaticValueRule)?.Value?.ToString()}'.");
        }

        if (!this.Values.Any())
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

        if (leftResult == null || leftResult.WasFailure)
        {
            throw new InvalidOperationException($"Failed to evaluate {nameof(LeftOperand)} for {DisplayName} operation: {leftResult?.ErrorMessage ?? "Result was null."}");
        }

        var valueResults = new List<TransformationResult>();
        foreach (var valueRule in Values)
        {
            var valueEvaluationResult = await valueRule.Apply(contextResult);
            if (valueEvaluationResult == null || valueEvaluationResult.WasFailure)
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
        clone.TypeId = this.TypeId; 

        clone.LeftOperand = this.LeftOperand?.Clone() as MappingRuleBase;
        clone.RightOperand = this.RightOperand?.Clone() as MappingRuleBase;
        clone.HighLimit = this.HighLimit?.Clone() as MappingRuleBase;

        clone.Values = this.Values.Select(v => (MappingRuleBase)v.Clone()).ToList();
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
        if (leftResult.CurrentValue is null)
        {
            return values.Any(val => val.CurrentValue is null);
        }

        return values.Any(val => object.Equals(leftResult.CurrentValue, val.CurrentValue));
    }
}
