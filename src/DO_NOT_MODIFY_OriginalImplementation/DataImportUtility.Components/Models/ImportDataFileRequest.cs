using DataImportUtility.Abstractions;

using Microsoft.AspNetCore.Components.Forms;

namespace DataImportUtility.Components.Models;

/// <summary>
/// Represents a request to import a data file
/// </summary>
public class ImportDataFileRequest : IImportDataFileRequest
{
    /// <summary>
    /// The file to read the data from.
    /// </summary>
    public IBrowserFile? File { get; set; }

    /// <inheritdoc />
    public bool HasHeaderRow { get; set; }

    /// <inheritdoc />
    public string? SheetName { get; set; }

    /// <inheritdoc />
    public string? Name => File?.Name;

    /// <inheritdoc />
    public DateTimeOffset? LastModified => File?.LastModified;

    /// <inheritdoc />
    public long? Size => File?.Size;

    /// <inheritdoc />
    public string? ContentType => File?.ContentType;

    /// <inheritdoc />
    public Stream? OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        => File?.OpenReadStream(maxAllowedSize, cancellationToken);
}
