using System.Data;

using DataImportUtility.Models;
using DataImportUtility.Rules;
using DataImportUtility.Tests.SampleData;

namespace DataImportUtility.Tests.ModelTests;

public class ImportedDataFileTests
{
    /// <summary>
    /// Tests that the GenerateOutputDataTable method returns the expected output data table.
    /// </summary>
    [Fact]
    public async Task GenerateOutputDataTableTest()
    {
        // Arrange
        var dataFile = ImportDataObjects.DataFile.Clone();

        // Add a new data table to the ImportDataFile
        var randomTableName = Guid.NewGuid().ToString();

        var inputDataTable = new DataTable(randomTableName);
        inputDataTable.Columns.Add("Field 1");
        inputDataTable.Columns.Add("Field 2");

        var row = inputDataTable.NewRow();
        row["Field 1"] = "Test Input";
        row["Field 2"] = "Test Input 2";
        inputDataTable.Rows.Add(row);

        row = inputDataTable.NewRow();
        row["Field 1"] = "Test Input 3";
        row["Field 2"] = "Test Input 4";
        inputDataTable.Rows.Add(row);

        var newDataSet = new DataSet();
        newDataSet.Tables.Add(inputDataTable);

        // Add the DataTable to the global ImportDataFile
        dataFile.SetTargetType<FakeTargetType>();
        dataFile.SetData(newDataSet, false);

        // To be moved to a data file test
        Assert.NotNull(dataFile.DataSet);

        // To be moved to a data table test
        Assert.True(dataFile.DataSet.Tables.Contains(randomTableName));

        // To be moved to a table definitions test
        Assert.True(dataFile.TableDefinitions.TryGetTableDefinition(randomTableName, out var tableDefinition));
        Assert.NotNull(tableDefinition);

        // Get the field descriptors
        var fieldDescriptors = tableDefinition.FieldDescriptors;

        // To be moved to a field descriptors test
        Assert.Contains(tableDefinition.FieldDescriptors, x => x.FieldName == "Field 1");
        Assert.Contains(tableDefinition.FieldDescriptors, x => x.FieldName == "Field 2");

        // Get the field mappings
        var fieldMappings = tableDefinition!.FieldMappings;
        var targetTypeFieldNames = typeof(FakeTargetType).GetProperties().Select(x => x.Name).ToArray();

        // To be moved to a field mappings test -- tests that all of the target type properties are mapped
        Assert.Equal(targetTypeFieldNames, fieldMappings.Select(x => x.FieldName).ToArray());

        var labAnalysisFieldMapping = fieldMappings.First();
        labAnalysisFieldMapping.MappingRule = new CombineFieldsRule
        {
            RuleDetail = "${0}-----${1}"
        };
        labAnalysisFieldMapping.MappingRule.AddFieldTransformation(fieldDescriptors.First());
        labAnalysisFieldMapping.MappingRule.AddFieldTransformation(fieldDescriptors.Last());

        foreach (var remainingRule in fieldMappings.Skip(1))
        {
            remainingRule.MappingRule = new IgnoreRule();
        }

        // Expectations
        var expectedColumnNames = targetTypeFieldNames;
        var expectedColumnCount = expectedColumnNames.Length;
        var expectedRowCount = inputDataTable.Rows.Count;
        var expectedValues = new[] { "Test Input-----Test Input 2", "Test Input 3-----Test Input 4" };

        // Act
        var outputDataTable = await dataFile.GenerateOutputDataTable(randomTableName);

        // Assert
        Assert.NotNull(outputDataTable);
        Assert.Equal(expectedRowCount, outputDataTable.Rows.Count);
        Assert.Equal(expectedColumnCount, outputDataTable.Columns.Count);
        Assert.Equal(expectedColumnNames, outputDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray());

        var result = outputDataTable.Rows.Cast<DataRow>().Select(x => x[labAnalysisFieldMapping.FieldName]).ToArray();
        Assert.Equal(expectedValues, result);
    }
}
