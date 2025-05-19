namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// Represents the standard import workflow with basic mapping and validation.
/// </summary>
public class StandardImportWorkflow : ImportWorkflow
{
    /// <inheritdoc/>
    public override string Name => "standard";

    /// <inheritdoc/>
    public override string DisplayName => "Standard Import";

    /// <inheritdoc/>
    public override int StepCount => 3; // File selection, mapping, validation/import

    /// <inheritdoc/>
    public override Type ComponentType => typeof(DataFileMapper<>); // Return the generic type definition

    /// <summary>
    /// Standard workflow is a generic component that requires a type parameter.
    /// </summary>
    public override bool IsGenericComponent => true;

    /// <inheritdoc/>
    public override bool SupportsFileType(FileType fileType)
    {
        // Standard workflow supports all file types
        return true;
    }
}