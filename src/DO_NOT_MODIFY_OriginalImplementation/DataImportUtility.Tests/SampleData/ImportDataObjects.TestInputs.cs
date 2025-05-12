using System.Data;

using DataImportUtility.Models;

namespace DataImportUtility.Tests.SampleData;

internal static partial class ImportDataObjects
{
    internal static string[] ValueCollectionForFailures { get; } = ["1234567890", "1234567890"];
    internal static TransformationResult TransformResultForRuleInput { get; } = new() { OriginalValue = "Test Input", Value = "Test Input" };

    /// <summary>
    /// Gets the data row at the specified index from the main data table.
    /// </summary>
    /// <param name="rowIndex">The index of the row to get.</param>
    /// <returns>
    /// The data row at the specified index.
    /// </returns>
    internal static DataRow GetImportRow(int rowIndex = 0)
        => MainDataTable.Rows.OfType<DataRow>().Skip(rowIndex).First();

    /// <summary>
    /// Gets the ImportFieldDescriptor for the specified field name from the main data table's <see cref="ImportedDataFile.TableDefinitions" /> entry.
    /// </summary>
    internal static ImportedRecordFieldDescriptor GetImportField(string? fieldName = null)
        => MainFieldDescriptors.First(x => string.IsNullOrWhiteSpace(fieldName) || x.FieldName == fieldName);

    /// <summary>
    /// Gets the ImportFieldDescriptor for the specified field index from the main data table's <see cref="ImportedDataFile.TableDefinitions" /> entry.
    /// </summary>
    internal static ImportedRecordFieldDescriptor GetImportField(int skip)
        => DataFile.TableDefinitions.First().FieldDescriptors.Skip(skip).First();

    /// <summary>
    /// Gets the FieldMapping for the specified field name from the main data table's <see cref="ImportedDataFile.TableDefinitions" /> entry.
    /// </summary>
    internal static FieldMapping GetFieldMapping(string? fieldName = null)
        => MainFieldMappings.First(x => string.IsNullOrWhiteSpace(fieldName) || x.FieldName == fieldName);

    /// <summary>
    /// Gets the FieldMapping for the specified field index from the main data table's <see cref="ImportedDataFile.TableDefinitions" /> entry.
    /// </summary>
    internal static FieldMapping GetFieldMapping(int fieldIndex)
        => MainFieldMappings.Skip(fieldIndex).First().Clone();
}
