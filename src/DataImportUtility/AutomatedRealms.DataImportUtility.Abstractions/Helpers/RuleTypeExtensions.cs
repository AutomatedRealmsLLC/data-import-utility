using AutomatedRealms.DataImportUtility.Abstractions.Enums;

namespace AutomatedRealms.DataImportUtility.Abstractions.Helpers;

/// <summary>
/// Extension methods for the <see cref="RuleType"/> enum.
/// </summary>
public static class RuleTypeExtensions
{
    /// <summary>
    /// Gets the corresponding class <see cref="Type"/> for a given <see cref="RuleType"/>.
    /// </summary>
    /// <param name="ruleType">The rule type.</param>
    /// <returns>The <see cref="Type"/> of the rule implementation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the rule type is not recognized.</exception>
    public static Type GetClassType(this RuleType ruleType)
    {
        return ruleType switch
        {
            RuleType.CopyRule => typeof(CopyRule),
            RuleType.ConstantValueRule => typeof(ConstantValueRule),
            RuleType.CombineFieldsRule => typeof(CombineFieldsRule),
            RuleType.IgnoreRule => typeof(IgnoreRule),
            RuleType.CustomFieldlessRule => typeof(CustomFieldlessRule),
            RuleType.None => throw new ArgumentOutOfRangeException(nameof(ruleType), $"RuleType 'None' does not have a corresponding class type."),
            _ => throw new ArgumentOutOfRangeException(nameof(ruleType), $"Unknown rule type: {ruleType}")
        };
    }
}
