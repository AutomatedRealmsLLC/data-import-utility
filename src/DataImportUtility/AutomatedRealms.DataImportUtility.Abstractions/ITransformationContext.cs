using System.Data; // Required for DataRow

using AutomatedRealms.DataImportUtility.Abstractions.Models; // For ImportedRecordFieldDescriptor and ImportTableDefinition

namespace AutomatedRealms.DataImportUtility.Abstractions;

/// <summary>
/// Defines the context for a transformation operation.
/// </summary>
public interface ITransformationContext
{
    /// <summary>
    /// Gets or sets the DataRow associated with this transformation, if applicable.
    /// </summary>
    DataRow? Record { get; set; }

    /// <summary>
    /// Gets or sets the definition of the table from which the record originates, if applicable.
    /// </summary>
    ImportTableDefinition? TableDefinition { get; set; }

    /// <summary>
    /// Gets or sets the context of the source record, typically a list of field descriptors.
    /// </summary>
    List<ImportedRecordFieldDescriptor>? SourceRecordContext { get; set; }

    /// <summary>
    /// Gets or sets the expected target type of the field being transformed.
    /// </summary>
    Type? TargetFieldType { get; set; }
}
