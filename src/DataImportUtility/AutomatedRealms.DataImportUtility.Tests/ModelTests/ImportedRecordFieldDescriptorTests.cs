using System.Data;
using AutomatedRealms.DataImportUtility.Abstractions.Models;

using AutomatedRealms.DataImportUtility.Tests.SampleData;

namespace AutomatedRealms.DataImportUtility.Tests.ModelTests;
public class ImportedRecordFieldDescriptorTests
{
    /// <summary>
    /// Tests that the <see cref="ImportedRecordFieldDescriptor.ValueSet"/> property is set correctly when accessing it.
    /// </summary>
    [Fact]
    public void ImportedRecordFieldDescriptor_ValueSet_ShouldUpdateWhenAccessingIt()
    {
        // Arrange
        var expected = "Test Value";

        var randomTableName = Guid.NewGuid().ToString();
        var dataFile = ImportDataObjects.DataFile;
        var newTable = new DataTable(randomTableName);
        newTable.Columns.Add("Test Field", typeof(string));
        newTable.Rows.Add(expected);
        dataFile.DataSet!.Tables.Add(newTable);

        var fieldDescriptor = new ImportedRecordFieldDescriptor()
        {
            FieldName = "Test Field",
            ImportedDataFile = dataFile,
            ForTableName = randomTableName,
            FieldType = typeof(string)
        };

        // Act
        var valueSet = fieldDescriptor.ValueSet;

        // Assert
        Assert.NotNull(valueSet);
        Assert.Equal(expected, valueSet[0]);
    }

    /// <summary>
    /// Tests that the <see cref="ImportedRecordFieldDescriptor.ValueSet"/> property is updated when the data set changes.
    /// </summary>
    [Fact]
    public void ImportedRecordFieldDescriptor_ValueSet_ShouldUpdateWhenDataSetChanges()
    {
        // Arrange
        var expected = "Test Value";

        var randomTableName = Guid.NewGuid().ToString();
        var dataFile = ImportDataObjects.DataFile;
        var newTable = new DataTable(randomTableName);
        newTable.Columns.Add("Test Field", typeof(string));
        newTable.Rows.Add(expected);
        dataFile.DataSet!.Tables.Add(newTable);

        var fieldDescriptor = new ImportedRecordFieldDescriptor()
        {
            FieldName = "Test Field",
            ImportedDataFile = dataFile,
            ForTableName = randomTableName,
            FieldType = typeof(string)
        };

        // Act
        var valueSet = fieldDescriptor.ValueSet;

        // Assert
        Assert.NotNull(valueSet);
        Assert.Equal(expected, valueSet[0]);

        // Arrange
        var newExpected = "New Test Value";
        newTable.Rows.Clear();
        newTable.Rows.Add(newExpected);

        // Act
        valueSet = fieldDescriptor.ValueSet;

        // Assert
        Assert.NotNull(valueSet);
        Assert.Equal(newExpected, valueSet[0]);
    }

    /// <summary>
    /// Tests that the <see cref="ImportedRecordFieldDescriptor.ValueSet"/> property is updated when the data changes.
    /// </summary>
    [Fact]
    public void ImportedRecordFieldDescriptor_ValueSet_ShouldUpdateWhenAnyDataChanges()
    {
        // Arrange
        var expected = new[] { "Test Value 1", "Test Value 2" };

        var randomTableName = Guid.NewGuid().ToString();
        var dataFile = ImportDataObjects.DataFile;
        var newTable = new DataTable(randomTableName);
        newTable.Columns.Add("Test Field", typeof(string));
        foreach (var value in expected)
        {
            newTable.Rows.Add(value);
        }
        dataFile.DataSet!.Tables.Add(newTable);

        var fieldDescriptor = new ImportedRecordFieldDescriptor()
        {
            FieldName = "Test Field",
            ImportedDataFile = dataFile,
            ForTableName = randomTableName,
            FieldType = typeof(string)
        };

        // Act
        var valueSet = fieldDescriptor.ValueSet;

        // Assert
        Assert.NotNull(valueSet);
        Assert.All(valueSet, (value, index) => Assert.Equal(expected[index], value));

        // Arrange
        expected[1] = "New Test Value";
        newTable.Rows[1]["Test Field"] = expected[1];

        // Act
        valueSet = fieldDescriptor.ValueSet;

        // Assert
        Assert.NotNull(valueSet);
        Assert.All(valueSet, (value, index) => Assert.Equal(expected[index], value));
    }
}
