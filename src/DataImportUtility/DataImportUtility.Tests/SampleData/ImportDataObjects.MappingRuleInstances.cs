using System.Diagnostics.CodeAnalysis;

using DataImportUtility.Abstractions;
using DataImportUtility.Models;
using DataImportUtility.Rules;

namespace DataImportUtility.Tests.SampleData;

internal static partial class ImportDataObjects
{
    private static List<MappingRuleBase>? _mappingRules;
    /// <summary>
    /// The list of mapping rules to use for testing.
    /// </summary>
    internal static List<MappingRuleBase> MappingRules
    {
        get
        {
            PrepareMappingRuleInstances();
            return _mappingRules;
        }
    }

    /// <summary>
    /// The copy rule instance to use for testing.
    /// </summary>
    internal static CopyRule CopyRule => MappingRules.OfType<CopyRule>().First().Clone<CopyRule>();
    /// <summary>
    /// The combine fields rule instance to use for testing.
    /// </summary>
    internal static CombineFieldsRule CombineFieldsRule => MappingRules.OfType<CombineFieldsRule>().First().Clone<CombineFieldsRule>();
    /// <summary>
    /// The ignore rule instance to use for testing.
    /// </summary>
    internal static IgnoreRule IgnoreRule => MappingRules.OfType<IgnoreRule>().First().Clone<IgnoreRule>();

    private static readonly object _mappingRulePreparationLock = new();

    [MemberNotNull(nameof(_mappingRules))]
    private static void PrepareMappingRuleInstances()
    {
        lock (_mappingRulePreparationLock)
        {
            if (_mappingRules is { Count: >0 }) { return; }

            _mappingRules = Enum.GetValues(typeof(MappingRuleType)).OfType<MappingRuleType>().Select(x => x.CreateNewInstance()!).ToList();

            foreach (var ruleType in Enum.GetValues(typeof(MappingRuleType)).OfType<MappingRuleType>())
            {
                var newRule = ruleType.CreateNewInstance();
                switch (newRule)
                {
                    case CopyRule copyRule:
                        copyRule.FieldTransformation = new FieldTransformation(GetImportField(0)).Clone();

                        break;
                    case CombineFieldsRule combineFieldsRule:
                        combineFieldsRule.RuleDetail = "${0}-${1}";
                        combineFieldsRule.AddFieldTransformation(
                            new FieldTransformation(GetImportField(0).Clone()));
                        combineFieldsRule.AddFieldTransformation(
                            new FieldTransformation(GetImportField(1).Clone()));

                        break;
                    case IgnoreRule ignoreRule:
                        // Nothing to add to the ignoreRule
                        break;
                }
            }
        }
    }
}
