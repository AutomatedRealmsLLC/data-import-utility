// Original file path: d:\git\AutomatedRealms\data-import-utility\src\DataImportUtility\AutomatedRealms.DataImportUtility.Abstractions\MappingRuleBase.cs
using System.Text.Json.Serialization;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

namespace AutomatedRealms.DataImportUtility.Abstractions;

/// <summary>
/// Base class for all mapping rules.
/// </summary>
public abstract class MappingRuleBase : ICloneable
{
    /// <summary>
    /// Gets the unique identifier for this type of mapping rule.
    /// This is used for serialization and deserialization.
    /// </summary>
    public string TypeId { get; protected set; }

    /// <summary>
    /// The event that is raised when the definition of the rule changes.
    /// </summary>
    public event Func<Task>? OnDefinitionChanged;

    /// <summary>
    /// The name of the enum member that was previously used for this rule type.
    /// This will be removed once the old enum system is fully deprecated.
    /// </summary>
    public abstract string EnumMemberName { get; } // To be removed

    /// <summary>
    /// The display name of the rule.
    /// </summary>
    [JsonIgnore]
    public abstract string DisplayName { get; }

    /// <summary>
    /// The short name to display for the rule.
    /// </summary>
    [JsonIgnore]
    public virtual string ShortName => DisplayName;

    /// <summary>
    /// The description of the rule.
    /// </summary>
    [JsonIgnore]
    public abstract string Description { get; }

    /// <summary>
    /// The error message if the rule configuration is invalid.
    /// </summary>
    [JsonIgnore]
    public string? ErrorMessage { get; protected set; }

    /// <summary>
    /// Indicates whether the rule configuration has an error.
    /// </summary>
    [JsonIgnore]
    public bool IsError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingRuleBase"/> class.
    /// </summary>
    /// <param name="typeId">The unique identifier for this rule type.</param>
    protected MappingRuleBase(string typeId)
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            throw new ArgumentException("TypeId cannot be null or whitespace.", nameof(typeId));
        }
        TypeId = typeId;
    }

    /// <summary>
    /// Applies the mapping rule.
    /// </summary>
    /// <param name="context">The transformation context.</param>
    /// <returns>The result of the mapping rule.</returns>
    public abstract Task<TransformationResult> ApplyRule(ITransformationContext context);

    /// <summary>
    /// Creates a deep clone of this mapping rule.
    /// </summary>
    /// <returns>A new instance with the same properties.</returns>
    public virtual MappingRuleBase Clone()
    {
        return (MappingRuleBase)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    /// <summary>
    /// Invokes the OnDefinitionChanged event.
    /// </summary>
    protected virtual Task InvokeOnDefinitionChangedAsync()
    {
        return OnDefinitionChanged?.Invoke() ?? Task.CompletedTask;
    }
}
