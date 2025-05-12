using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Added for ConditionalRule
using AutomatedRealms.DataImportUtility.Core.Rules;
using System;

namespace AutomatedRealms.DataImportUtility.Core.Helpers
{
    /// <summary>
    /// Provides extension methods for the <see cref="MappingRuleType"/> enum.
    /// </summary>
    public static class MappingRuleTypeExtensions
    {
        /// <summary>
        /// Gets the corresponding class <see cref="Type"/> for a given <see cref="MappingRuleType"/>.
        /// </summary>
        /// <param name="ruleType">The mapping rule type.</param>
        /// <returns>The <see cref="Type"/> of the class that implements the rule, or null if no direct mapping exists.</returns>
        public static Type? GetClassType(this MappingRuleType ruleType)
        {
            return ruleType switch
            {
                MappingRuleType.CopyRule => typeof(CopyRule),
                MappingRuleType.IgnoreRule => typeof(IgnoreRule),
                MappingRuleType.CombineFieldsRule => typeof(CombineFieldsRule),
                MappingRuleType.ConstantValueRule => typeof(ConstantValueRule),
                MappingRuleType.FieldAccessRule => typeof(FieldAccessRule),
                MappingRuleType.ConditionalRule => typeof(ConditionalRule), // From Abstractions.Models
                MappingRuleType.CustomRule => null, // No specific CustomRule class identified in Core.Rules
                MappingRuleType.CustomFieldlessRule => typeof(CustomFieldlessRule),
                _ => null // Default to null for any unhandled or new enum members
            };
        }

        /// <summary>
        /// Gets a human-readable description of the mapping rule type.
        /// </summary>
        /// <param name="ruleType">The mapping rule type to get a description for.</param>
        /// <returns>A string describing the purpose of the rule type.</returns>
        public static string GetDescription(this MappingRuleType ruleType)
        {
            return ruleType switch
            {
                MappingRuleType.CopyRule => "Copies a value directly from source to destination field",
                MappingRuleType.IgnoreRule => "Ignores this field in the mapping process",
                MappingRuleType.CombineFieldsRule => "Combines multiple source fields into one destination field",
                MappingRuleType.ConstantValueRule => "Uses a constant value for the destination field",
                MappingRuleType.FieldAccessRule => "Accesses a specific field in the source data",
                MappingRuleType.ConditionalRule => "Applies a rule conditionally based on evaluation criteria", 
                MappingRuleType.CustomRule => "Custom rule implementation",
                MappingRuleType.CustomFieldlessRule => "Custom rule that doesn't use field mappings",
                MappingRuleType.StaticValueRule => "Provides a static value for use in operations",
                _ => $"Unknown rule type: {ruleType}"
            };
        }
    }
}
