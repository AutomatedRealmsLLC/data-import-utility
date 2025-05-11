namespace AutomatedRealms.DataImportUtility.Abstractions.Enums
{
    public enum MappingRuleType
    {
        CopyRule,
        IgnoreRule,
        CombineFieldsRule,
        ConstantValueRule,
        FieldAccessRule,
        ConditionalRule, // Assuming this might exist or be planned
        CustomRule,      // General custom rule
        CustomFieldlessRule // Mentioned in ApplicationConstants
        // Add other rule types as identified or needed
    }
}
