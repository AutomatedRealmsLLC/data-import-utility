using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // For ConditionalRule, ImportTableDefinition
using AutomatedRealms.DataImportUtility.Abstractions.Services;
using AutomatedRealms.DataImportUtility.Core.Rules;

using System.Data; // Required for DataRow

namespace AutomatedRealms.DataImportUtility.Core.ComparisonOperations;

/// <summary>
/// Factory for creating and configuring comparison operations.
/// </summary>
public static class ComparisonOperationFactory
{
    /// <summary>
    /// Creates and configures a comparison operation based on the conditional rule.
    /// </summary>
    /// <param name="conditionalRule">The conditional rule defining the operation.</param>
    /// <param name="row">The data row for context (currently not directly used by the factory for operation creation but passed to operands).</param>
    /// <param name="tableDefinition">The parent table definition for context.</param>
    /// <param name="typeRegistry">The type registry service to resolve custom operations.</param>
    /// <param name="ruleContextIdentifier">A string identifier for the rule using this factory, for logging/debugging.</param>
    /// <returns>A configured ComparisonOperationBase instance or null if creation or configuration fails.</returns>
    public static Task<ComparisonOperationBase?> CreateOperationAsync(
        ConditionalRule conditionalRule,
        DataRow row,
        ImportTableDefinition? tableDefinition,
        ITypeRegistryService typeRegistry,
        string ruleContextIdentifier)
    {
        if (string.IsNullOrWhiteSpace(conditionalRule.OperationTypeId))
        {
            Console.WriteLine($"Warning: ConditionalRule OperationTypeId is null or empty in context: {ruleContextIdentifier}. Cannot create operation.");
            return Task.FromResult<ComparisonOperationBase?>(null);
        }

        // After the above check, conditionalRule.OperationTypeId is guaranteed not to be null or whitespace.
        string operationTypeId = conditionalRule.OperationTypeId!;

        if (conditionalRule.SourceField == null || string.IsNullOrEmpty(conditionalRule.SourceField.FieldName))
        {
            Console.WriteLine($"Warning: ConditionalRule SourceField or SourceField.FieldName is null for OperationTypeId: {operationTypeId} in context: {ruleContextIdentifier}.");
            return Task.FromResult<ComparisonOperationBase?>(null);
        }

        // Left operand is always based on the conditionalRule's SourceField
        MappingRuleBase leftOperandAccessRule = new FieldAccessRule(conditionalRule.SourceField.FieldName, $"Conditional_Source_{conditionalRule.SourceField.FieldName}_for_{ruleContextIdentifier}");
        if (tableDefinition != null)
        {
            leftOperandAccessRule.ParentTableDefinition = tableDefinition;
        }

        // Right operand (primary) - typically a static value
        MappingRuleBase? rightOperandRule = null;
        if (conditionalRule.ComparisonValue != null)
        {
            rightOperandRule = new StaticValueRule(conditionalRule.ComparisonValue, $"Conditional_ComparisonValue_for_{ruleContextIdentifier}");
            if (tableDefinition != null && rightOperandRule != null)
            {
                rightOperandRule.ParentTableDefinition = tableDefinition;
            }
        }

        // Secondary right operand - for operations like 'Between'
        MappingRuleBase? secondaryRightOperandRule = null;
        if (conditionalRule.SecondaryComparisonValue != null)
        {
            secondaryRightOperandRule = new StaticValueRule(conditionalRule.SecondaryComparisonValue, $"Conditional_SecondaryComparisonValue_for_{ruleContextIdentifier}");
            if (tableDefinition != null && secondaryRightOperandRule != null)
            {
                secondaryRightOperandRule.ParentTableDefinition = tableDefinition;
            }
        }

        var operation = typeRegistry.ResolveComparisonOperation(operationTypeId);
        if (operation == null)
        {
            Console.WriteLine($"Error: OperationTypeId '{operationTypeId}' could not be resolved by the TypeRegistryService in context: {ruleContextIdentifier}.");
            return Task.FromResult<ComparisonOperationBase?>(null);
        }

        try
        {
            // The ConfigureOperands method is responsible for assigning the operand rules 
            // and validating them.
            operation.ConfigureOperands(leftOperandAccessRule, rightOperandRule, secondaryRightOperandRule);
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is InvalidOperationException)
        {
            Console.WriteLine($"Error configuring operands for OperationTypeId '{operationTypeId}': {ex.Message}. Context: {ruleContextIdentifier}");
            return Task.FromResult<ComparisonOperationBase?>(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error configuring operands for OperationTypeId '{operationTypeId}': {ex}. Context: {ruleContextIdentifier}");
            return Task.FromResult<ComparisonOperationBase?>(null);
        }

        return Task.FromResult<ComparisonOperationBase?>(operation);
    }
}
