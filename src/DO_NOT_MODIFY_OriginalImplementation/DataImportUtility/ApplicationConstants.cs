using DataImportUtility.Abstractions;

namespace DataImportUtility;

/// <summary>
/// Internal Application Constants
/// </summary>
internal static class ApplicationConstants
{
    /// <summary>
    /// The unimplemented mapping rules.
    /// </summary>
    public static MappingRuleType[] UnimplementedMappingRules { get; } =
#if DEBUG
    Array.Empty<MappingRuleType>();
#else
    [
        MappingRuleType.CustomFieldlessRule
    ];
#endif

    /// <summary>
    /// The mapping rule types that are implemented.
    /// </summary>
    public static MappingRuleType[] MappingRuleTypes { get; } = [.. Enum.GetValues(typeof(MappingRuleType)).OfType<MappingRuleType>().Where(x => !UnimplementedMappingRules.Contains(x))];

    /// <summary>
    /// The unimplemented Value transformation types.
    /// </summary>
    public static ValueTransformationType[] UnimplementedValueTransformations { get; } =
#if DEBUG
        Array.Empty<ValueTransformationType>();
#else
        [
            ValueTransformationType.CombineFieldsTransformation,
            ValueTransformationType.ConditionalTransformation
        ];
#endif

    /// <summary>
    /// The value transformation types that are implemented.
    /// </summary>
    public static ValueTransformationType[] ValueTransformationTypes { get; } = [.. Enum.GetValues(typeof(ValueTransformationType)).OfType<ValueTransformationType>().Where(x => !UnimplementedValueTransformations.Contains(x))];
}
