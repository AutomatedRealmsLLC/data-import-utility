using System;
using System.Linq;

using DataImportUtility.Abstractions;

namespace DataImportUtility.Helpers;

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
            .OfType<MappingRuleType>()
            .Select(ruleType => $"{ruleType}: {ruleType.GetDescription()}")
    );

}
