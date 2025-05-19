namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// Represents a strategy for mapping columns from a source file to target model properties.
/// This class uses the "smart enum" pattern to allow for extensibility beyond the built-in strategies.
/// </summary>
public abstract class MappingStrategy : IEquatable<MappingStrategy>
{
    #region Built-in Types

    /// <summary>
    /// Header-based mapping strategy that maps columns based on header names.
    /// </summary>
    public static readonly MappingStrategy HeaderBased = new HeaderBasedMappingStrategy();

    /// <summary>
    /// Position-based mapping strategy that maps columns based on their position.
    /// </summary>
    public static readonly MappingStrategy PositionBased = new PositionBasedMappingStrategy();

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique identifier name for this mapping strategy.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the display name for this mapping strategy.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Gets the component type that implements this mapping strategy.
    /// </summary>
    /// <remarks>
    /// If this is a generic component, check <see cref="IsGenericComponent"/> and use
    /// the component with appropriate type parameters.
    /// </remarks>
    public abstract Type ComponentType { get; }

    /// <summary>
    /// Gets whether the component type is a generic type definition that needs type parameters.
    /// </summary>
    public virtual bool IsGenericComponent => ComponentType.IsGenericTypeDefinition;

    #endregion

    #region Methods

    /// <summary>
    /// Determines whether this mapping strategy supports the specified file type.
    /// </summary>
    /// <param name="fileType">The file type to check.</param>
    /// <returns>True if this strategy supports the specified file type; otherwise, false.</returns>
    public abstract bool SupportsFileType(FileType fileType);

    #endregion

    #region Registry

    private static readonly List<MappingStrategy> _knownStrategies = [HeaderBased, PositionBased];

    /// <summary>
    /// Gets all registered mapping strategies.
    /// </summary>
    public static IReadOnlyList<MappingStrategy> KnownStrategies => _knownStrategies.AsReadOnly();

    /// <summary>
    /// Registers a new mapping strategy.
    /// </summary>
    /// <param name="strategy">The strategy to register.</param>
    public static void Register(MappingStrategy strategy)
    {
        if (!_knownStrategies.Contains(strategy))
        {
            _knownStrategies.Add(strategy);
        }
    }

    /// <summary>
    /// Gets a mapping strategy by its name.
    /// </summary>
    /// <param name="name">The name of the strategy.</param>
    /// <returns>The strategy with the specified name, or null if not found.</returns>
    public static MappingStrategy? GetByName(string name)
    {
        return KnownStrategies.FirstOrDefault(s =>
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all strategies that support the specified file type.
    /// </summary>
    /// <param name="fileType">The file type to check support for.</param>
    /// <returns>A collection of strategies that support the specified file type.</returns>
    public static IEnumerable<MappingStrategy> GetForFileType(FileType fileType)
    {
        return KnownStrategies.Where(s => s.SupportsFileType(fileType));
    }

    #endregion

    #region Equality

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as MappingStrategy);
    }

    /// <inheritdoc/>
    public bool Equals(MappingStrategy? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(MappingStrategy? left, MappingStrategy? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(MappingStrategy? left, MappingStrategy? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override string ToString() => DisplayName;

    #endregion
}