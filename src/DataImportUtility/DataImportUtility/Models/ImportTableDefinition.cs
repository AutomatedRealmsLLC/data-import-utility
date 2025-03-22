using System.Diagnostics.CodeAnalysis;

namespace DataImportUtility.Models;

/// <summary>
/// The definition of a table to import, including its fields and mappings to a target type.
/// </summary>
public class ImportTableDefinition
{
    /// <summary>
    /// The name of the table.
    /// </summary>
    public required string TableName { get; set; }
    /// <summary>
    /// The fields in the table.
    /// </summary>
    public List<ImportedRecordFieldDescriptor> FieldDescriptors { get; set; } = [];
    /// <summary>
    /// The mappings to the target type.
    /// </summary>
    public List<FieldMapping> FieldMappings { get; set; } = [];
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
#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
    public static bool TryGetTableDefinition(this List<ImportTableDefinition> tableDefinitions, string tableName, [MyMaybeNullWhen(false), MyNotNullWhen(true)] out ImportTableDefinition? tableDefinition)
#else
    public static bool TryGetTableDefinition(this List<ImportTableDefinition> tableDefinitions, string tableName, [MaybeNullWhen(false), NotNullWhen(true)] out ImportTableDefinition? tableDefinition)
#endif
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
#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
    public static bool TryGetFieldDescriptors(this List<ImportTableDefinition> tableDefinitions, string tableName, [MyMaybeNullWhen(false), MyNotNullWhen(true)] out List<ImportedRecordFieldDescriptor>? fieldDescriptors)
#else
        public static bool TryGetFieldDescriptors(this List<ImportTableDefinition> tableDefinitions, string tableName, [MaybeNullWhen(false), NotNullWhen(true)] out List<ImportedRecordFieldDescriptor>? fieldDescriptors)
#endif
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
#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
    public static bool TryGetFieldMappings(this List<ImportTableDefinition> tableDefinitions, string tableName, [MyMaybeNullWhen(false), MyNotNullWhen(true)] out List<FieldMapping>? fieldMappings)
#else
        public static bool TryGetFieldMappings(this List<ImportTableDefinition> tableDefinitions, string tableName, [MaybeNullWhen(false), NotNullWhen(true)] out List<FieldMapping>? fieldMappings)
#endif
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
    public static bool TryAdd(this List<ImportTableDefinition> tableDefinitions, string tableName, List<ImportedRecordFieldDescriptor>? fieldDescriptors = null, List<FieldMapping>? fieldMappings = null)
    {
        if (tableDefinitions.Any(table => table.TableName == tableName))
        {
            return false;
        }

        tableDefinitions.Add(new ImportTableDefinition
        {
            TableName = tableName,
            FieldDescriptors = fieldDescriptors ?? [],
            FieldMappings = fieldMappings ?? []
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
}
