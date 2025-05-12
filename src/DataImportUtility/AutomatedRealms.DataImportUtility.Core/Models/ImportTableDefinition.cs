using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.Helpers;

namespace AutomatedRealms.DataImportUtility.Core.Models;

/// <summary>
/// The definition of a table to import, including its fields and mappings to a target type.
/// </summary>
public class ImportTableDefinition
{
    /// <summary>
    /// The name of the table.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// The fields in the table.
    /// </summary>
    public List<AutomatedRealms.DataImportUtility.Abstractions.Models.ImportedRecordFieldDescriptor> FieldDescriptors { get; set; } = new List<AutomatedRealms.DataImportUtility.Abstractions.Models.ImportedRecordFieldDescriptor>();
    /// <summary>
    /// The mappings to the target type.
    /// </summary>
    public List<AutomatedRealms.DataImportUtility.Abstractions.Models.FieldMapping> FieldMappings { get; set; } = new List<AutomatedRealms.DataImportUtility.Abstractions.Models.FieldMapping>();

    /// <summary>
    /// Clones the <see cref="ImportTableDefinition" />.
    /// </summary>
    /// <returns>The cloned <see cref="ImportTableDefinition" />.</returns>
    public ImportTableDefinition Clone()
    {
        var forRet = (ImportTableDefinition)MemberwiseClone();
        forRet.FieldDescriptors = FieldDescriptors.Select(x => x.Clone()).ToList();
        forRet.FieldMappings = FieldMappings.Select(x => x.Clone()).ToList();
        return forRet;
    }
    
    /// <summary>
    /// Replaces the field mappings with the provided ones.
    /// </summary>
    /// <param name="incomingFieldMappings">The field mappings to replace with.</param>
    public void ReplaceFieldMappings(IEnumerable<AutomatedRealms.DataImportUtility.Abstractions.Models.FieldMapping> incomingFieldMappings)
    {
        FieldMappings = incomingFieldMappings.Select(x => x.Clone()).ToList();
    }
}

/// <summary>
/// Extension methods for <see cref="ImportTableDefinition" />.
/// </summary>
public static class ImportTableDefinitionExtensions
{
    /// <summary>
    /// Attempts to get the table definition for the given table name.
    /// </summary>
    /// <param name="tableDefinitions">The table definitions to search.</param>
    /// <param name="tableName">The name of the table to search for.</param>
    /// <param name="tableDefinition">The table definition found, if any, for the table.</param>
    /// <returns>True if the collection had an element with the specified <paramref name="tableName"/>, otherwise false.</returns>
    public static bool TryGetTableDefinition(this List<ImportTableDefinition> tableDefinitions, string tableName, out ImportTableDefinition? tableDefinition)
    {
        tableDefinition = tableDefinitions.FirstOrDefault(x => x.TableName == tableName);
        return tableDefinition is not null;
    }

    /// <summary>
    /// Attempts to get the field descriptors for the given table name.
    /// </summary>
    /// <param name="tableDefinitions">The table definitions to search.</param>
    /// <param name="tableName">The name of the table to search for.</param>
    /// <param name="fieldDescriptors">The field descriptors found, if any, for the table.</param>
    /// <returns>True if the collection had an element with the specified <paramref name="tableName"/>, otherwise false.</returns>
    public static bool TryGetFieldDescriptors(this List<ImportTableDefinition> tableDefinitions, string tableName, out List<AutomatedRealms.DataImportUtility.Abstractions.Models.ImportedRecordFieldDescriptor>? fieldDescriptors)
    {
        fieldDescriptors = tableDefinitions.FirstOrDefault(x => x.TableName == tableName)?.FieldDescriptors;
        return fieldDescriptors is not null;
    }

    /// <summary>
    /// Attempts to get the field mappings for the given table name.
    /// </summary>
    /// <param name="tableDefinitions">The table definitions to search.</param>
    /// <param name="tableName">The name of the table to search for.</param>
    /// <param name="fieldMappings">The field mappings found, if any, for the table.</param>
    /// <returns>True if the collection had an element with the specified <paramref name="tableName"/>, otherwise false.</returns>
    public static bool TryGetFieldMappings(this List<ImportTableDefinition> tableDefinitions, string tableName, out List<AutomatedRealms.DataImportUtility.Abstractions.Models.FieldMapping>? fieldMappings)
    {
        fieldMappings = tableDefinitions.FirstOrDefault(x => x.TableName == tableName)?.FieldMappings;
        return fieldMappings is not null;
    }

    /// <summary>
    /// Gets the field descriptors for the given table name.
    /// </summary>
    /// <param name="tableDefinitions">The table definitions to search.</param>
    /// <param name="tableName">The name of the table to search for.</param>
    /// <returns>The field descriptors found, if any, for the table.</returns>
    /// <exception cref="ArgumentNullException">Thrown if no item was found with the table name.</exception>
    public static ImportTableDefinition Get(this List<ImportTableDefinition> tableDefinitions, string tableName)
        => tableDefinitions.First(table => table.TableName == tableName);

    /// <summary>
    /// Tries to add a new <see cref="ImportTableDefinition" /> to the collection.
    /// </summary>
    /// <param name="tableDefinitions">The table definitions to add to.</param>
    /// <param name="tableName">The name of the table to add.</param>
    /// <param name="fieldDescriptors">The field descriptors for the table.</param>
    /// <param name="fieldMappings">The field mappings for the table.</param>
    /// <returns>True if the table was added, otherwise false.</returns>
    public static bool TryAdd(this List<ImportTableDefinition> tableDefinitions, string tableName, List<AutomatedRealms.DataImportUtility.Abstractions.Models.ImportedRecordFieldDescriptor>? fieldDescriptors = null, List<AutomatedRealms.DataImportUtility.Abstractions.Models.FieldMapping>? fieldMappings = null)
    {
        if (tableDefinitions.Any(table => table.TableName == tableName))
        {
            return false;
        }

        tableDefinitions.Add(new ImportTableDefinition
        {
            TableName = tableName,
            FieldDescriptors = fieldDescriptors ?? new List<AutomatedRealms.DataImportUtility.Abstractions.Models.ImportedRecordFieldDescriptor>(),
            FieldMappings = fieldMappings ?? new List<AutomatedRealms.DataImportUtility.Abstractions.Models.FieldMapping>()
        });

        return true;
    }

    /// <summary>
    /// Checks if the table definitions contain a table with the given name.
    /// </summary>
    /// <param name="tableDefinitions">The table definitions to search.</param>
    /// <param name="tableName">The name of the table to search for.</param>
    /// <returns>True if the collection had an element with the specified <paramref name="tableName"/>, otherwise false.</returns>
    public static bool ContainsTable(this List<ImportTableDefinition> tableDefinitions, string tableName)
        => tableDefinitions.Any(table => table.TableName == tableName);
        
    /// <summary>
    /// Refreshes the field descriptors for the table.
    /// </summary>
    /// <param name="tableDefinition">The table definition to refresh.</param>
    /// <param name="dataTable">The data table to use for refreshing.</param>
    /// <param name="hasHeader">Whether or not the data has a header row.</param>
    /// <param name="targetTypeFieldMappings">The field mappings to copy from for the target type.</param>
    /// <param name="overwriteExisting">Whether or not to overwrite existing field mappings.</param>
    /// <param name="autoMatch">Whether or not to automatically match fields.</param>
    public static void RefreshFieldDescriptors(this ImportTableDefinition tableDefinition, DataTable dataTable, bool hasHeader, ImmutableList<AutomatedRealms.DataImportUtility.Abstractions.Models.FieldMapping>? targetTypeFieldMappings, bool overwriteExisting = false, bool autoMatch = false)
    {
        // Clear the field descriptors if requested
        if (overwriteExisting)
        {
            tableDefinition.FieldDescriptors.Clear();
        }

        // Add field descriptors for each column in the data table
        foreach (DataColumn column in dataTable.Columns)
        {
            var fieldName = column.ColumnName;
            var existingDescriptor = tableDefinition.FieldDescriptors.FirstOrDefault(x => x.FieldName == fieldName);
            
            if (existingDescriptor is null)
            {
                // Create a new field descriptor
                var descriptor = new AutomatedRealms.DataImportUtility.Abstractions.Models.ImportedRecordFieldDescriptor
                {
                    FieldName = fieldName,
                    FieldType = column.DataType
                };
                
                tableDefinition.FieldDescriptors.Add(descriptor);
            }
        }
        
        // Update the field mappings
        if (overwriteExisting)
        {
            tableDefinition.FieldMappings.Clear();
        }
        
        // Add field mappings for the target type
        if (targetTypeFieldMappings is not null)
        {
            foreach (var mapping in targetTypeFieldMappings)
            {
                var existingMapping = tableDefinition.FieldMappings.FirstOrDefault(x => x.FieldName == mapping.FieldName);
                
                if (existingMapping is null)
                {
                    // Add a new field mapping
                    var newMapping = mapping.Clone();
                    tableDefinition.FieldMappings.Add(newMapping);
                    
                    // Auto match if requested
                    if (autoMatch)
                    {
                        // Look for a field descriptor with the same name
                        var matchingDescriptor = tableDefinition.FieldDescriptors.FirstOrDefault(x => 
                            string.Equals(x.FieldName, mapping.FieldName, StringComparison.OrdinalIgnoreCase));
                            
                        if (matchingDescriptor is not null && newMapping.MappingRule is not null)
                        {
                            // TODO: Create a field transformation from the matching descriptor
                            // This would depend on the specific implementation of FieldTransformation and MappingRule
                        }
                    }
                }
            }
        }
    }
}
