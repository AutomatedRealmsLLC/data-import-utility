namespace AutomatedRealms.DataImportUtility.Abstractions.Models;

/// <summary>
/// The definition of a table to import, including its fields and mappings to a target type.
/// </summary>
public class ImportTableDefinition
{
    /// <summary>
    /// The name of the table.
    /// </summary>
    public string? TableName { get; set; } // Made nullable
    /// <summary>
    /// The fields in the table.
    /// </summary>
    public List<ImportedRecordFieldDescriptor> FieldDescriptors { get; set; } = [];
    /// <summary>
    /// The mappings to the target type.
    /// </summary>
    public List<FieldMapping> FieldMappings { get; set; } = []; // FieldMapping will also need to be in Abstractions.Models
}

// Note: The ImportTableDefinitionExtensions class was not moved as it might have dependencies
// on List<ImportTableDefinition> which might be used in other contexts in the original project.
// If these extensions are purely for ImportTableDefinition and have no other dependencies,
// they could also be moved and their namespace updated.
// For now, only the class itself is moved as per the DTO/model migration task.
