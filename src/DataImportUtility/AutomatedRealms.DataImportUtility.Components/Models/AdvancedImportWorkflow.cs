namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// Represents an advanced import workflow with additional customization options.
/// </summary>
public class AdvancedImportWorkflow : ImportWorkflow
{
    /// <inheritdoc/>
    public override string Name => "advanced";

    /// <inheritdoc/>
    public override string DisplayName => "Advanced Import";

    /// <inheritdoc/>
    public override int StepCount => 5; // File selection, column selection, mapping, transformation, validation/import

    /// <inheritdoc/>
    public override Type ComponentType => typeof(DataFileMapper<>); // Return the generic type definition

    /// <summary>
    /// Advanced workflow is a generic component that requires a type parameter.
    /// </summary>
    public override bool IsGenericComponent => true;

    /// <inheritdoc/>
    public override bool SupportsFileType(FileType fileType)
    {
        // Advanced workflow supports only Excel for complex mapping
        return fileType == FileType.Excel;
    }
}