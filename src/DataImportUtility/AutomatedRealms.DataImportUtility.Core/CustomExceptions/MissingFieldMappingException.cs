using AutomatedRealms.DataImportUtility.Abstractions.Models; // Changed from Core.Models to Abstractions.Models
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatedRealms.DataImportUtility.Core.CustomExceptions;

/// <summary>
/// Represents an exception that is thrown when a required field mapping does not have a source.
/// </summary>
[Serializable]
public class MissingFieldMappingException : Exception
{
    private const string _defaultMessage = "One or more required field mappings are missing.";

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingFieldMappingException"/> class with
    /// the specified field mappings and optional message and inner exception.
    /// </summary>
    /// <param name="missingFieldMappings">The field mappings that are missing.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MissingFieldMappingException(IEnumerable<FieldMapping> missingFieldMappings, string? message = null, Exception? innerException = null)
        : base(null, innerException)
    {
        MissingFieldMappings = missingFieldMappings.Select(x => (FieldMapping)x.Clone()).ToArray();
        CustomMessage = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingFieldMappingException"/> class with
    /// just a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MissingFieldMappingException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
        MissingFieldMappings = Array.Empty<FieldMapping>();
        CustomMessage = message;
    }

    /// <summary>
    /// Gets the custom message provided during initialization.
    /// </summary>
    public string? CustomMessage { get; }

    /// <summary>
    /// The required field mappings that do not have a source.
    /// </summary>
    // Assuming FieldMapping has a Clone method. This might need adjustment if Clone is an extension or part of an interface not directly on FieldMapping.
    public FieldMapping[] MissingFieldMappings { get; }

    /// <inheritdoc />
    public override string Message => MissingFieldMappings.Length > 0 
        ? $"{CustomMessage ?? _defaultMessage}{Environment.NewLine}Missing Field Mapping{(MissingFieldMappings.Length != 1 ? "s" : null)}:{Environment.NewLine}  - {string.Join($"{Environment.NewLine} - ", MissingFieldMappings.Select(x => x.FieldName))}"
        : CustomMessage ?? base.Message;
}
