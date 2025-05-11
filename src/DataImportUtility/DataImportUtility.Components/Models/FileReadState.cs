namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// The states for a file read request.
/// </summary>
public enum FileReadState
{
    /// <summary>
    /// The file has not been read.
    /// </summary>
    NoFile,
    /// <summary>
    /// The file has been selected.
    /// </summary>
    FileSelected,
    /// <summary>
    /// The file is being read.
    /// </summary>
    Reading,
    /// <summary>
    /// An error occurred while reading the file.
    /// </summary>
    Error,
    /// <summary>
    /// The file was read successfully.
    /// </summary>
    Success,
}
