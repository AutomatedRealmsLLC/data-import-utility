using AutomatedRealms.DataImportUtility.Abstractions.Enums; // For MappingRuleType
using System;
using System.Linq;
// Using extension methods from the same namespace

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
            .Select(ruleType => $"{ruleType}: {ruleType.GetDescription()}") // Uses MappingRuleTypeExtensions.GetDescription
    );
}
