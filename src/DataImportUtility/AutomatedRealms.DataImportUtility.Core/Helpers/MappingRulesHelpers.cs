using AutomatedRealms.DataImportUtility.Abstractions; // For MappingRuleType
using System;
using System.Linq;
// Assuming GetDescription() is an extension method for MappingRuleType,
// possibly in AutomatedRealms.DataImportUtility.Core.Helpers.EnumExtensions
// or AutomatedRealms.DataImportUtility.Abstractions if it's a general enum helper.

namespace AutomatedRealms.DataImportUtility.Core.Helpers;

/// <summary>
/// The methods for applying transformation rules.
/// </summary>
public static class MappingRulesHelpers
{
    /// <summary>
    /// The string blurb describing each rule.
    /// </summary>
    public static string RuleDescriptions { get; } = string.Join(
        Environment.NewLine,
        Enum.GetValues(typeof(MappingRuleType))
            .Cast<MappingRuleType>()
            .Select(ruleType => $"{ruleType}: {ruleType.GetDescription()}") // Assumes MappingRuleType has GetDescription() extension
    );
}
