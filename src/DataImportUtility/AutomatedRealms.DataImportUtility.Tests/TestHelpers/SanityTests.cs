using System.Data;

using AutomatedRealms.DataImportUtility.Abstractions.Models;

using AutomatedRealms.DataImportUtility.Tests.SampleData;

namespace AutomatedRealms.DataImportUtility.Tests.TestHelpers;

public class SanityTests
{
    /// <summary>
    /// The <see cref="ImportDataObjects.DataFile"/> should have a <see cref="DataSet"/>.
    /// </summary>
    [Fact]
    public void ImportData_DataSetExists()
    {
        Assert.True(ImportDataObjects.DataFile.DataSet is not null, "The DataSet was null");
    }

    /// <summary>
    /// The <see cref="ImportDataObjects.DataFile"/> should have a <see cref="DataSet"/>.
    /// </summary>
    [Fact]
    public void ImportData_DataSetHasTables()
    {
        Assert.True(ImportDataObjects.DataFile.DataSet!.Tables.Count > 0, "The DataSet didn't have any DataTables");
    }

    /// <summary>
    /// The <see cref="ImportDataObjects.DataFile"/> should have a <see cref="DataTable"/> with the <see cref="ImportDataObjects.DataTableName"/>.
    /// </summary>
    [Fact]
    public void ImportData_DataSetHasImportTable()
    {
        Assert.True(ImportDataObjects.DataFile.DataSet!.Tables[ImportDataObjects.DataTableName] is not null, "The DataTable named 'Import' was null");
    }

    /// <summary>
    /// The <see cref="ImportDataObjects.DataFile"/> should have a <see cref="DataTable"/> with rows.
    /// </summary>
    [Fact]
    public void ImportData_ImportTableHasRows()
    {
        Assert.True(ImportDataObjects.MainDataTable.Rows.Count > 0, "There were no rows in the DataTable named 'Import'");
    }

    /// <summary>
    /// The <see cref="ImportDataObjects.DataFile"/> should have a <see cref="DataTable"/> with field descriptors.
    /// </summary>
    [Fact]
    public void ImportData_ImportTableHasFieldDescriptors()
    {
        // Arrange + Act
        ImportDataObjects.DataFile.TableDefinitions.TryGetFieldDescriptors(ImportDataObjects.DataTableName, out var fieldDescriptors);

        Assert.NotNull(fieldDescriptors);
        Assert.True(fieldDescriptors.Count > 0, "There were no ImportedRecordFieldDescriptors");
    }
}
