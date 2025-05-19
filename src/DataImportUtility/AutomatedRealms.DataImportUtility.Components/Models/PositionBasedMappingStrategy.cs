using AutomatedRealms.DataImportUtility.Components.FieldMappingComponents;

namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// Represents a mapping strategy that maps columns based on their position.
/// </summary>
public class PositionBasedMappingStrategy : MappingStrategy
{
    /// <inheritdoc/>
    public override string Name => "position-based";

    /// <inheritdoc/>
    public override string DisplayName => "Position-Based Mapping";

    /// <inheritdoc/>
    public override Type ComponentType => typeof(FieldMapperEditor);

    /// <inheritdoc/>
    public override bool SupportsFileType(FileType fileType)
    {
        // Position-based mapping works best with CSV files
        // For Excel, it can work but is less intuitive
        return true;
    }
}