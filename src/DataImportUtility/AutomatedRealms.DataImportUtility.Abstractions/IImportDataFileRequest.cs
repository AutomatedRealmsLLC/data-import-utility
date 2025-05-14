namespace AutomatedRealms.DataImportUtility.Abstractions;

/// <summary>
/// Represents a request to import data from a file.
/// </summary>
/// <remarks>
/// This is essentially an IBrowserFile, but we don't want to couple the service to the web project.
/// </remarks>
public interface IImportDataFileRequest
{
    /// <summary>
    /// Indicates whether the file has a header row.
    /// </summary>
    bool HasHeaderRow { get; set; }

    /// <summary>
    /// If the file is an Excel file, this property specifies the name of the sheet to read.
    /// </summary>
    string? SheetName { get; set; }

    /// <summary>
    /// Gets the name of the file as specified by the browser.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the last modified date as specified by the browser.
    /// </summary>
    DateTimeOffset? LastModified { get; }

    /// <summary>
    /// Gets the size of the file in bytes as specified by the browser.
    /// </summary>
    long? Size { get; }

    /// <summary>
    /// Gets the MIME type of the file as specified by the browser.
    /// </summary>
    string? ContentType { get; }

    /// <summary>
    ///  Opens the stream for reading the uploaded file.
    /// </summary>
    /// <param name="maxAllowedSize">
    /// The maximum number of bytes that can be supplied by the Stream. Defaults to 500
    /// KB.
    ///
    /// Calling Microsoft.AspNetCore.Components.Forms.IBrowserFile.OpenReadStream(System.Int64,System.Threading.CancellationToken)
    /// will throw if the file's size, as specified by Microsoft.AspNetCore.Components.Forms.IBrowserFile.Size
    /// is larger than maxAllowedSize. By default, if the user supplies a file larger
    /// than 500 KB, this method will throw an exception.
    ///
    /// It is valuable to choose a size limit that corresponds to your use case. If you
    /// allow excessively large files, this may result in excessive consumption of memory
    /// or disk/database space, depending on what your code does with the supplied System.IO.Stream.
    ///
    /// For Blazor Server in particular, beware of reading the entire stream into a memory
    /// buffer unless you have passed a suitably low size limit, since you will be consuming
    /// that memory on the server.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token to signal the cancellation of streaming file data.
    /// </param>  
    /// <exception cref="IOException">
    /// Thrown if the file's length exceeds the maxAllowedSize value.
    /// </exception>
    Stream? OpenReadStream(long maxAllowedSize = 512000L, CancellationToken cancellationToken = default);
}
