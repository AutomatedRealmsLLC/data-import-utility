using System.Data;

namespace AutomatedRealms.DataImportUtility.Abstractions.Helpers;

/// <summary>
/// Extension methods for <see cref="DataTable" />.
/// </summary>
public static class DataTableExtensions
{
    /// <summary>
    /// Gets the values of a column in a data table.
    /// </summary>
    /// <param name="sourceTable">The source table to get the values from.</param>
    /// <param name="forField">The name of the field to get the values for.</param>
    /// <returns>
    /// An array of objects that represent the values of the specified field in the data 
    /// table in the order they appear in the table at the time the method was called.
    /// </returns>
    public static object[] GetColumnValues(this DataTable sourceTable, string forField)
    {
        if (sourceTable is null)
        {
            throw new ArgumentNullException(nameof(sourceTable));
        }
        if (string.IsNullOrEmpty(forField))
        {
            throw new ArgumentException("Field name cannot be null or empty.", nameof(forField));
        }
        if (!sourceTable.Columns.Contains(forField))
        {
            // Or return empty array, or handle as per specific project requirements
            throw new ArgumentException($"Field '{forField}' does not exist in the table '{sourceTable.TableName}'.", nameof(forField));
        }

        return [.. sourceTable.Rows
            .OfType<DataRow>()
            .Select(r => r[forField])];
    }
}
