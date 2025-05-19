namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// Represents a CSV file type.
/// </summary>
public class CsvFileType : FileType
{
    /// <inheritdoc/>
    public override string Name => "csv";

    /// <inheritdoc/>
    public override string DisplayName => "CSV";

    /// <inheritdoc/>
    public override string[] SupportedExtensions => [".csv"];

    /// <inheritdoc/>
    public override string[] MimeTypes => [
        "text/csv",
        "application/csv",
        "text/comma-separated-values"
    ];
}