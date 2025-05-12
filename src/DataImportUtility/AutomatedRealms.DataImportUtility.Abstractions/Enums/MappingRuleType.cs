namespace AutomatedRealms.DataImportUtility.Abstractions.Enums
{
    /// <summary>
    /// Defines the types of mapping rules available in the system.
    /// </summary>
    public enum MappingRuleType
    {
        /// <summary>
        /// Rule to directly copy a value.
        /// </summary>
        CopyRule,
        /// <summary>
        /// Rule to ignore a field or value.
        /// </summary>
        IgnoreRule,
        /// <summary>
        /// Rule to combine multiple fields into one.
        /// </summary>
        CombineFieldsRule,
        /// <summary>
        /// Rule to use a predefined constant value.
        /// </summary>
        ConstantValueRule,
        /// <summary>
        /// Rule to access a field from the source data.
        /// </summary>
        FieldAccessRule,
        /// <summary>
        /// Rule that defines a condition for other rules.
        /// </summary>
        ConditionalRule, 
        /// <summary>
        /// Rule for custom logic not covered by other types.
        /// </summary>
        CustomRule,      
        /// <summary>
        /// Rule for custom logic that does not require a source field.
        /// </summary>
        CustomFieldlessRule,
        /// <summary>
        /// Rule to use a static, predefined value.
        /// </summary>
        StaticValueRule
    }
}
