using System.Data;

using DataImportUtility.Models;
using DataImportUtility.Rules;
using DataImportUtility.Tests.SampleData;
using DataImportUtility.Tests.TestHelpers;

namespace DataImportUtility.Tests.MappingRuleTests;

public class CombineFieldsRuleTests : MappingRuleBaseTestContext
{
    /// <summary>
    /// Thests that the <see cref="CombineFieldsRule"/> should return the combined values of the input.
    /// </summary>
    [Fact]
    public async Task CombineRule_ApplyOnTransformResults_ShouldReturnCombinedValues()
    {
        // Arrange
        var rule = ImportDataObjects.CombineFieldsRule;
        rule.RuleDetail = "${0}------${1}";
        var input = new TransformationResult() { Value = @"[""Test Input"",""Test Input 2""]" };

        // Act
        var result = (await rule.Apply(input)).Value;

        // Assert
        Assert.Equal("Test Input------Test Input 2", result);
    }

    /// <summary>
    /// The <see cref="CombineFieldsRule.Apply()"/> should return the combined values of the input data table.
    /// </summary>
    [Fact]
    public async Task CombineRule_ApplyWithoutParameters_ShouldCombineFieldsForDataTable()
    {
        // Arrange
        var randomTableName = Guid.NewGuid().ToString();

        var dataTable = new DataTable(randomTableName);
        dataTable.Columns.Add("Field 1");
        dataTable.Columns.Add("Field 2");

        var row = dataTable.NewRow();
        row["Field 1"] = "Test Input";
        row["Field 2"] = "Test Input 2";
        dataTable.Rows.Add(row);

        row = dataTable.NewRow();
        row["Field 1"] = "Test Input 3";
        row["Field 2"] = "Test Input 4";
        dataTable.Rows.Add(row);

        // Add the DataTable to the global ImportDataFile
        ImportDataObjects.MainDataSet.Tables.Add(dataTable);

        var rule = ImportDataObjects.CombineFieldsRule;
        rule.RuleDetail = "${0} ${1}";

        var fieldDescriptors = new[]
        {
            new ImportedRecordFieldDescriptor()
            {
                FieldName = "Field 1",
                FieldType = typeof(string),
                ForTableName = randomTableName,
                ImportedDataFile = ImportDataObjects.DataFile
            },
            new ImportedRecordFieldDescriptor()
            {
                FieldName = "Field 2",
                FieldType = typeof(string),
                ForTableName = randomTableName,
                ImportedDataFile = ImportDataObjects.DataFile
            }
        };

        rule.AddFieldTransformation(fieldDescriptors.First());
        rule.AddFieldTransformation(fieldDescriptors.Last());

        var expected = new[] { "Test Input Test Input 2", "Test Input 3 Test Input 4" };

        // Act
        var result = (await rule.Apply()).Select(x => x.Value).ToArray();

        // Assert
        Assert.Equal(expected, result);
    }
}
