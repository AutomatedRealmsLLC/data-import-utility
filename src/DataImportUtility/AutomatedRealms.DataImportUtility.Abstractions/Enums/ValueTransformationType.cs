namespace AutomatedRealms.DataImportUtility.Abstractions.Enums;

/// <summary>
/// Defines the types of value transformations available.
/// </summary>
public enum ValueTransformationType
{
    /// <summary>
    /// Represents a calculation transformation.
    /// </summary>
    CalculateTransformation,

    /// <summary>
    /// Represents a transformation that combines multiple fields.
    /// </summary>
    CombineFieldsTransformation,

    /// <summary>
    /// Represents a conditional transformation.
    /// </summary>
    ConditionalTransformation,

    /// <summary>
    /// Represents an interpolation transformation.
    /// </summary>
    InterpolateTransformation,

    /// <summary>
    /// Represents a value mapping transformation.
    /// </summary>
    MapTransformation,

    /// <summary>
    /// Represents a regex matching transformation.
    /// </summary>
    RegexMatchTransformation,

    /// <summary>
    /// Represents a substring extraction transformation.
    /// </summary>
    SubstringTransformation
    // Add other transformation types here as they are defined/moved
}
