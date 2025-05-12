using AutomatedRealms.DataImportUtility.Abstractions.Enums;

namespace AutomatedRealms.DataImportUtility.Abstractions;

/// <summary>
/// Internal Application Constants for the Core library.
/// </summary>
internal static class ApplicationConstants
{
    /// <summary>
    /// The unimplemented mapping rules.
    /// </summary>
    public static MappingRuleType[] UnimplementedMappingRules { get; } =
#if DEBUG
    [];
#else
    new MappingRuleType[]
    {
        MappingRuleType.CustomFieldlessRule // Example, adjust as needed for Core context
    };
#endif

    /// <summary>
    /// The mapping rule types that are considered implemented within the Core library.
    /// </summary>
    public static MappingRuleType[] MappingRuleTypes { get; } = [.. Enum.GetValues(typeof(MappingRuleType)).Cast<MappingRuleType>().Where(x => !UnimplementedMappingRules.Contains(x))];

    /// <summary>
    /// The unimplemented Value transformation types.
    /// </summary>
    public static ValueTransformationType[] UnimplementedValueTransformations { get; } =
#if DEBUG
        [];
#else
        new ValueTransformationType[]
        {
            // Example, adjust as needed for Core context
            // ValueTransformationType.CombineFieldsTransformation, 
            // ValueTransformationType.ConditionalTransformation 
        };
#endif

    /// <summary>
    /// The value transformation types that are considered implemented within the Core library.
    /// </summary>
    public static ValueTransformationType[] ValueTransformationTypes { get; } = [.. Enum.GetValues(typeof(ValueTransformationType)).Cast<ValueTransformationType>().Where(x => !UnimplementedValueTransformations.Contains(x))];
}
