using System.Data;

using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Abstractions;

/// <summary>
/// Defines the contract for a mapping rule.
/// </summary>
public interface IMappingRule : IDisposable
{
    /// <summary>
    /// The event that is raised when the definition of the rule changes.
    /// </summary>
    event Func<Task>? OnDefinitionChanged;

    /// <summary>
    /// The value the generated enum member display in the MappingRuleType.
    /// </summary>
    int EnumMemberOrder { get; }

    /// <summary>
    /// The member name to use for the MappingRuleType.
    /// </summary>
    string EnumMemberName { get; }

    /// <summary>
    /// The name of the rule.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The short name to display for the rule.
    /// </summary>
    string ShortName { get; }

    /// <summary>
    /// Gets the description of the rule.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The additional information for the rule.
    /// </summary>
    string? RuleDetail { get; set; }

    /// <summary>
    /// An indicator of whether the rule is empty.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// The unique identifier for the rule.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The type of the rule.
    /// </summary>
    RuleType RuleType { get; }

    /// <summary>
    /// Gets the value of the rule.
    /// </summary>
    /// <param name="row">The row to get the value from.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <returns>The value of the rule.</returns>
    Task<TransformationResult> GetValue(DataRow row, Type targetType);

    /// <summary>
    /// Creates a deep clone of this mapping rule.
    /// </summary>
    /// <returns>A new instance with the same properties.</returns>
    IMappingRule Clone();
}
