using System;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using System.Data; // Required for DataRow
using AutomatedRealms.DataImportUtility.Core.Rules; // Required for FieldAccessRule, StaticValueRule

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
    /// <param name="row">The data row for context.</param>
    /// <param name="tableDefinition">The parent table definition for context.</param>
    /// <param name="ruleContextIdentifier">A string identifier for the rule using this factory, for logging/debugging.</param>
    /// <returns>A configured ComparisonOperationBase instance or null if creation fails.</returns>
    public static Task<ComparisonOperationBase?> CreateOperationAsync(
        ConditionalRule conditionalRule, 
        DataRow row, 
        ImportTableDefinition? tableDefinition, 
        string ruleContextIdentifier)
    {
        if (conditionalRule.SourceField == null || string.IsNullOrEmpty(conditionalRule.SourceField.FieldName))
        {
            Console.WriteLine($"Warning: ConditionalRule SourceField or SourceField.FieldName is null for OperationType: {conditionalRule.OperationType} in context: {ruleContextIdentifier}.");
            return Task.FromResult<ComparisonOperationBase?>(null);
        }

        // Left operand is always based on the conditionalRule's SourceField
        MappingRuleBase leftOperandAccessRule = new FieldAccessRule(conditionalRule.SourceField.FieldName, $"Conditional_Source_{conditionalRule.SourceField.FieldName}_for_{ruleContextIdentifier}");
        leftOperandAccessRule.ParentTableDefinition = tableDefinition;


        // Right operand (primary) - typically a static value
        MappingRuleBase? rightOperandRule = null;
        if (conditionalRule.ComparisonValue != null)
        {
            rightOperandRule = new StaticValueRule(conditionalRule.ComparisonValue, $"Conditional_ComparisonValue_for_{ruleContextIdentifier}");
            rightOperandRule.ParentTableDefinition = tableDefinition;
        }
        else if (conditionalRule.OperationType != ComparisonOperationType.IsNullOrEmpty &&
                 conditionalRule.OperationType != ComparisonOperationType.IsNotNullOrEmpty &&
                 conditionalRule.OperationType != ComparisonOperationType.IsNullOrWhiteSpace &&
                 conditionalRule.OperationType != ComparisonOperationType.IsNotNullOrWhiteSpace)
        {
            // For operations requiring a comparison value, log if it's missing.
            LogOperandMissing(conditionalRule.OperationType, "ComparisonValue (RightOperand)", ruleContextIdentifier);
            // Depending on the operation, we might still proceed if it's a unary operation that was misconfigured,
            // or return null if the operation is invalid without it.
        }
        
        // Secondary right operand - for operations like 'Between'
        MappingRuleBase? secondaryRightOperandRule = null;
        if (conditionalRule.SecondaryComparisonValue != null)
        {
            secondaryRightOperandRule = new StaticValueRule(conditionalRule.SecondaryComparisonValue, $"Conditional_SecondaryComparisonValue_for_{ruleContextIdentifier}");
            secondaryRightOperandRule.ParentTableDefinition = tableDefinition;
        }


        ComparisonOperationBase? operation = null;

        switch (conditionalRule.OperationType)
        {
            case ComparisonOperationType.Equals:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new EqualsOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.NotEquals:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new NotEqualOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.GreaterThan:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new GreaterThanOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.GreaterThanOrEqual:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new GreaterThanOrEqualOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.LessThan:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new LessThanOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.LessThanOrEqual:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new LessThanOrEqualOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.Contains:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new ContainsOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.NotContains:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new NotContainsOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.StartsWith:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new StartsWithOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.EndsWith:
                if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RightOperand", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new EndsWithOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            case ComparisonOperationType.IsNull:
                operation = new IsNullOperation { LeftOperand = leftOperandAccessRule };
                break;
            case ComparisonOperationType.IsNotNull:
                operation = new IsNotNullOperation { LeftOperand = leftOperandAccessRule };
                break;
            case ComparisonOperationType.IsNullOrEmpty:
                operation = new IsNullOrEmptyOperation { LeftOperand = leftOperandAccessRule };
                break;
            case ComparisonOperationType.IsNotNullOrEmpty:
                operation = new IsNotNullOrEmptyOperation { LeftOperand = leftOperandAccessRule };
                break;
            case ComparisonOperationType.IsNullOrWhiteSpace:
                operation = new IsNullOrWhiteSpaceOperation { LeftOperand = leftOperandAccessRule };
                break;
            case ComparisonOperationType.IsNotNullOrWhiteSpace:
                operation = new IsNotNullOrWhiteSpaceOperation { LeftOperand = leftOperandAccessRule };
                break;
            case ComparisonOperationType.IsBetween: // Corrected from Between to IsBetween
                if (rightOperandRule == null || secondaryRightOperandRule == null) 
                { 
                    LogOperandMissing(conditionalRule.OperationType, "RightOperand (LowLimit) and/or SecondaryRightOperand (HighLimit)", ruleContextIdentifier); 
                    return Task.FromResult<ComparisonOperationBase?>(null); 
                }
                operation = new BetweenOperation { LeftOperand = leftOperandAccessRule, LowLimit = rightOperandRule, HighLimit = secondaryRightOperandRule };
                break;
            case ComparisonOperationType.RegexMatch:
                 if (rightOperandRule == null) { LogOperandMissing(conditionalRule.OperationType, "RegexPattern (RightOperand)", ruleContextIdentifier); return Task.FromResult<ComparisonOperationBase?>(null); }
                operation = new RegexMatchOperation { LeftOperand = leftOperandAccessRule, RightOperand = rightOperandRule };
                break;
            default:
                Console.WriteLine($"Warning: Unsupported ComparisonOperationType: {conditionalRule.OperationType} in context: {ruleContextIdentifier}.");
                return Task.FromResult<ComparisonOperationBase?>(null);
        }

        // Common configuration for all operations, if any, could go here.
        // For example, setting the original conditionalRule if needed for evaluation context.
        if (operation != null)
        {
            // operation.OriginalRule = conditionalRule; // If ComparisonOperationBase had such a property
        }

        return Task.FromResult(operation);
    }

    private static void LogOperandMissing(ComparisonOperationType operationType, string operandName, string ruleContextIdentifier)
    {
        Console.WriteLine($"Warning: {operandName} is missing for {operationType} in context: {ruleContextIdentifier}. Conditional rule may not behave as expected.");
    }
}
