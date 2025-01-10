using DataImportUtility.Models;

namespace DataImportUtility.CustomExceptions;

/// <summary>
/// Represents an exception that is thrown when a required field mapping does not have a source.
/// </summary>
/// <param name="message">The message that describes the error.</param>
/// <param name="missingFieldMappings">The required field mappings that do not have a source.</param>
/// <param name="innerException">The exception that is the cause of the current exception.</param>
[Serializable]
public class MissingFieldMappingException(IEnumerable<FieldMapping> missingFieldMappings, string? message = null, Exception? innerException = null) : Exception(null, innerException)
{
    private const string _defaultMessage = "One or more required field mappings are missing.";

    /// <summary>
    /// The required field mappings that do not have a source.
    /// </summary>
    public FieldMapping[] MissingFieldMappings { get; } = missingFieldMappings.Select(x => x.Clone()).ToArray();

    /// <inheritdoc />
    public override string Message => $"{message ?? _defaultMessage}{Environment.NewLine}Missing Field Mapping{(MissingFieldMappings.Length != 1 ? "s" : null)}:{Environment.NewLine}  - {string.Join($"{Environment.NewLine} - ", MissingFieldMappings.Select(x => x.FieldName))}";
}