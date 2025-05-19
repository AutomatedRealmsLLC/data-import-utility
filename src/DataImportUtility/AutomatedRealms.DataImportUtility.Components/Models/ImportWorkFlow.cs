namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// Represents a workflow for importing data files.
/// This class uses the "smart enum" pattern to allow for extensibility beyond the built-in workflows.
/// </summary>
public abstract class ImportWorkflow : IEquatable<ImportWorkflow>
{
    #region Built-in Types

    /// <summary>
    /// Standard import workflow with mapping and validation steps.
    /// </summary>
    public static readonly ImportWorkflow Standard = new StandardImportWorkflow();

    /// <summary>
    /// Advanced import workflow with additional customization options.
    /// </summary>
    public static readonly ImportWorkflow Advanced = new AdvancedImportWorkflow();

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique identifier name for this workflow.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the display name for this workflow.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Gets the number of steps in this workflow.
    /// </summary>
    public abstract int StepCount { get; }

    /// <summary>
    /// Gets the component type, which may be a generic type definition.
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
    /// Determines whether this workflow supports the specified file type.
    /// </summary>
    /// <param name="fileType">The file type to check.</param>
    /// <returns>True if this workflow supports the specified file type; otherwise, false.</returns>
    public abstract bool SupportsFileType(FileType fileType);

    #endregion

    #region Registry

    private static readonly List<ImportWorkflow> _knownWorkflows = [Standard, Advanced];

    /// <summary>
    /// Gets all registered import workflows.
    /// </summary>
    public static IReadOnlyList<ImportWorkflow> KnownWorkflows => _knownWorkflows.AsReadOnly();

    /// <summary>
    /// Registers a new import workflow.
    /// </summary>
    /// <param name="workflow">The workflow to register.</param>
    public static void Register(ImportWorkflow workflow)
    {
        if (!_knownWorkflows.Contains(workflow))
        {
            _knownWorkflows.Add(workflow);
        }
    }

    /// <summary>
    /// Gets a workflow by its name.
    /// </summary>
    /// <param name="name">The name of the workflow.</param>
    /// <returns>The workflow with the specified name, or null if not found.</returns>
    public static ImportWorkflow? GetByName(string name)
    {
        return KnownWorkflows.FirstOrDefault(w =>
            w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all workflows that support the specified file type.
    /// </summary>
    /// <param name="fileType">The file type to check support for.</param>
    /// <returns>A collection of workflows that support the specified file type.</returns>
    public static IEnumerable<ImportWorkflow> GetForFileType(FileType fileType)
    {
        return KnownWorkflows.Where(w => w.SupportsFileType(fileType));
    }

    #endregion

    #region Equality

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ImportWorkflow);
    }

    /// <inheritdoc/>
    public bool Equals(ImportWorkflow? other)
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
    public static bool operator ==(ImportWorkflow? left, ImportWorkflow? right)
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
    public static bool operator !=(ImportWorkflow? left, ImportWorkflow? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override string ToString() => DisplayName;

    #endregion
}
