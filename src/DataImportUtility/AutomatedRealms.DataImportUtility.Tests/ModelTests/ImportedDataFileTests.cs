using System.Data;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.Rules;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using AutomatedRealms.DataImportUtility.Abstractions.Helpers; // For ApplyTransformation
using AutomatedRealms.DataImportUtility.Tests.SampleData; // For ImportDataObjects and FakeTargetType
using System; // For Guid
using System.Threading.Tasks; // For Task
using Xunit; // For Fact and Assert

namespace AutomatedRealms.DataImportUtility.Tests.ModelTests;

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
        dataFile.SetTargetType(typeof(FakeTargetType)); 
        dataFile.SetData(newDataSet, false);

        // To be moved to a data file test
        Assert.NotNull(dataFile.DataSet);

        // To be moved to a data table test
        Assert.True(dataFile.DataSet.Tables.Contains(randomTableName));

        // To be moved to a table definitions test
        var tableDefinition = dataFile.TableDefinitions.FirstOrDefault(td => td.TableName == randomTableName); 
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
        Assert.Equal(targetTypeFieldNames, [.. fieldMappings.Select(x => x.FieldName)]);

        var labAnalysisFieldMapping = fieldMappings.First();
        var combineRule = new CombineFieldsRule(NullLogger<CombineFieldsRule>.Instance) 
        {
            CombinationFormat = "${0}-----${1}" 
        };
        
        if (fieldDescriptors.Any())
        {
            combineRule.InputFields.Add(new ConfiguredInputField { FieldName = fieldDescriptors.First().FieldName });
            combineRule.InputFields.Add(new ConfiguredInputField { FieldName = fieldDescriptors.Last().FieldName });
        }
        labAnalysisFieldMapping.MappingRule = combineRule;

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
        var sourceDataTable = dataFile.DataSet!.Tables[randomTableName]; 
        Assert.NotNull(sourceDataTable); 
        
        var outputDataTable = await sourceDataTable.ApplyTransformation(fieldMappings, null); 

        // Assert
        Assert.NotNull(outputDataTable);
        Assert.Equal(expectedRowCount, outputDataTable.Rows.Count);
        Assert.Equal(expectedColumnCount, outputDataTable.Columns.Count);
        Assert.Equal(expectedColumnNames, [.. outputDataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName)]);

        var result = outputDataTable.Rows.Cast<DataRow>().Select(x => x[labAnalysisFieldMapping.FieldName]).ToArray();
        Assert.Equal(expectedValues, result);
    }
}
