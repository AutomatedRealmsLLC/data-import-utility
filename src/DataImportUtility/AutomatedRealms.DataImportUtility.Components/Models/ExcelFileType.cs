namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// Represents an Excel file type.
/// </summary>
public class ExcelFileType : FileType
{
    /// <inheritdoc/>
    public override string Name => "excel";

    /// <inheritdoc/>
    public override string DisplayName => "Excel";

    /// <inheritdoc/>
    public override string[] SupportedExtensions => [".xlsx", ".xls"];

    /// <inheritdoc/>
    public override string[] MimeTypes => [
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    ];
}