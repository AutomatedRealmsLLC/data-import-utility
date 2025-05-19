namespace AutomatedRealms.DataImportUtility.Components.Models;

/// <summary>
/// Represents a file type that can be processed by the import utility.
/// This class uses the "smart enum" pattern to allow for extensibility beyond the built-in types.
/// </summary>
public abstract class FileType : IEquatable<FileType>
{
    #region Built-in Types

    /// <summary>
    /// Excel file type (.xlsx, .xls)
    /// </summary>
    public static readonly FileType Excel = new ExcelFileType();

    /// <summary>
    /// CSV file type (.csv)
    /// </summary>
    public static readonly FileType Csv = new CsvFileType();

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique identifier name for this file type.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the display name for this file type.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Gets the supported file extensions for this file type.
    /// </summary>
    public abstract string[] SupportedExtensions { get; }

    /// <summary>
    /// Gets the MIME types associated with this file type.
    /// </summary>
    public abstract string[] MimeTypes { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Determines whether the specified file can be handled by this file type.
    /// </summary>
    /// <param name="fileName">Name of the file to check.</param>
    /// <param name="mimeType">Optional MIME type of the file.</param>
    /// <returns>True if this file type can handle the specified file; otherwise, false.</returns>
    public virtual bool CanHandleFile(string fileName, string? mimeType = null)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        // Check extensions
        if (SupportedExtensions.Any(ext =>
            fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Check MIME type if provided
        if (mimeType != null && MimeTypes.Any(m =>
            m.Equals(mimeType, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Registry

    private static readonly List<FileType> _knownTypes = [Excel, Csv];

    /// <summary>
    /// Gets all registered file types.
    /// </summary>
    public static IReadOnlyList<FileType> KnownTypes => _knownTypes.AsReadOnly();

    /// <summary>
    /// Registers a new file type.
    /// </summary>
    /// <param name="fileType">The file type to register.</param>
    public static void Register(FileType fileType)
    {
        if (!_knownTypes.Contains(fileType))
        {
            _knownTypes.Add(fileType);
        }
    }

    /// <summary>
    /// Gets a file type by its name.
    /// </summary>
    /// <param name="name">The name of the file type.</param>
    /// <returns>The file type with the specified name, or null if not found.</returns>
    public static FileType? GetByName(string name)
    {
        return KnownTypes.FirstOrDefault(t =>
            t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a file type that can handle the specified file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="mimeType">Optional MIME type of the file.</param>
    /// <returns>A file type that can handle the file, or null if none found.</returns>
    public static FileType? GetForFile(string fileName, string? mimeType = null)
    {
        return KnownTypes.FirstOrDefault(t => t.CanHandleFile(fileName, mimeType));
    }

    #endregion

    #region Equality

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as FileType);
    }

    /// <inheritdoc/>
    public bool Equals(FileType? other)
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
    public static bool operator ==(FileType? left, FileType? right)
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
    public static bool operator !=(FileType? left, FileType? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override string ToString() => DisplayName;

    #endregion
}
