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
            return _mappingRules ?? throw new InvalidOperationException("Mapping rules failed to initialize. Please check the test setup.");
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

    private static readonly object _mappingRulePreparationLock = new();

    private static void PrepareMappingRuleInstances()
    {
        lock (_mappingRulePreparationLock)
        {
            if (_mappingRules is { Count: >0 }) { return; }

            _mappingRules = ApplicationConstants.MappingRuleTypes.Select(x => x.CreateNewInstance()!).ToList();

            foreach (var ruleType in ApplicationConstants.MappingRuleTypes)
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
