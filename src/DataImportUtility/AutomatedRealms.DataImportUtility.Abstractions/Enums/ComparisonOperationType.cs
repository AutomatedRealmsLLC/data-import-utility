namespace AutomatedRealms.DataImportUtility.Abstractions.Enums;

/// <summary>
/// Defines the types of comparison operations available for conditional rules.
/// </summary>
public enum ComparisonOperationType
{
    /// <summary>
    /// No operation specified.
    /// </summary>
    None,

    /// <summary>
    /// Checks if the field value is equal to the specified value.
    /// </summary>
    Equals,

    /// <summary>
    /// Checks if the field value is not equal to the specified value.
    /// </summary>
    NotEquals,

    /// <summary>
    /// Checks if the field value is greater than the specified value.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Checks if the field value is greater than or equal to the specified value.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Checks if the field value is less than the specified value.
    /// </summary>
    LessThan,

    /// <summary>
    /// Checks if the field value is less than or equal to the specified value.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Checks if the field value contains the specified value (for strings).
    /// </summary>
    Contains,

    /// <summary>
    /// Checks if the field value does not contain the specified value (for strings).
    /// </summary>
    NotContains,

    /// <summary>
    /// Checks if the field value starts with the specified value (for strings).
    /// </summary>
    StartsWith,

    /// <summary>
    /// Checks if the field value ends with the specified value (for strings).
    /// </summary>
    EndsWith,

    /// <summary>
    /// Checks if the field value is null.
    /// </summary>
    IsNull,

    /// <summary>
    /// Checks if the field value is not null.
    /// </summary>
    IsNotNull,

    /// <summary>
    /// Checks if the field value is null or an empty string.
    /// </summary>
    IsNullOrEmpty,

    /// <summary>
    /// Checks if the field value is not null and not an empty string.
    /// </summary>
    IsNotNullOrEmpty,

    /// <summary>
    /// Checks if the field value is null, an empty string, or consists only of white-space characters.
    /// </summary>
    IsNullOrWhiteSpace,

    /// <summary>
    /// Checks if the field value is not null, not an empty string, and does not consist only of white-space characters.
    /// </summary>
    IsNotNullOrWhiteSpace,

    /// <summary>
    /// Checks if the field value matches a specified regular expression.
    /// </summary>
    RegexMatch,

    /// <summary>
    /// Checks if the field value is between two specified values (inclusive or exclusive based on implementation).
    /// </summary>
    IsBetween
    // Add other comparison types as needed
}
