using System.Data;
using System.Linq; // Added for OfType<DataRow>(), Skip(), First()
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Added for TransformationResult, ImportedRecordFieldDescriptor, FieldMapping
using System.Collections.Generic;

namespace AutomatedRealms.DataImportUtility.Tests.SampleData;

internal static partial class ImportDataObjects
{
    // Changed collection expression to new string[] for compatibility with older .NET versions
    internal static string[] ValueCollectionForFailures { get; } = new string[] { "1234567890", "1234567890" };
    // Corrected the TransformationResult.Success call to have 9 arguments
    internal static TransformationResult TransformResultForRuleInput { get; } = TransformationResult.Success("Test Input", typeof(string), "Test Input", typeof(string), null, null, null, null, typeof(string));

    public static IEnumerable<object[]> InterpolateOperationHappyPathInputs =>
        [
            new object[] { "Simple Interpolation", "Hello {0}", "World", "Hello World" },
            new object[] { "Multiple Interpolations", "{0} {1}", "Hello", "World", "Hello World" }, // This case needs adjustment if Transform takes only one source value
            new object[] { "No Interpolation Token", "Hello World", "Test", "Hello World" },
            new object[] { "Empty Source Value", "Hello {0}", "", "Hello " },
            new object[] { "Empty Pattern", "", "Test", "" },
        ];

    public static IEnumerable<object[]> InterpolateOperationInvalidPatternInputs =>
        [
            new object[] { "Mismatched Brace", "Hello {0", "World", "Invalid interpolation pattern" },
            // Add more invalid pattern test cases if necessary
        ];

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
