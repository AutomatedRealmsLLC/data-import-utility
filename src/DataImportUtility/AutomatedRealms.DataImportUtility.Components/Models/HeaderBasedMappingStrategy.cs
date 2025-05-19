using AutomatedRealms.DataImportUtility.Components.FieldMappingComponents;

namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// Represents a mapping strategy that maps columns based on header names.
/// </summary>
public class HeaderBasedMappingStrategy : MappingStrategy
{
    /// <inheritdoc/>
    public override string Name => "header-based";

    /// <inheritdoc/>
    public override string DisplayName => "Header-Based Mapping";

    /// <inheritdoc/>
    public override Type ComponentType => typeof(FieldMapperEditor);

    /// <inheritdoc/>
    public override bool SupportsFileType(FileType fileType)
    {
        // Header-based mapping works with all file types that have headers
        return true;
    }
}