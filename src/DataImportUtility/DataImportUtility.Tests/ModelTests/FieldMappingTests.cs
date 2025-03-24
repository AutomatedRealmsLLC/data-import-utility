using System.Data;

using DataImportUtility.Abstractions;
using DataImportUtility.Models;
using DataImportUtility.Rules;
using DataImportUtility.Tests.SampleData;

namespace DataImportUtility.Tests.ModelTests;

public class FieldMappingTests
{
    private static FieldMapping FieldMappingUT => ImportDataObjects.DataFile.TableDefinitions.First().FieldMappings.First().Clone();
    private static DataRow ImportRowUT => ImportDataObjects.GetImportRow();

    #region No Field Transforms
    /// <summary>
    /// A field mapping with the <see cref="CombineFieldsRule"/> should return null 
    /// when no transforms are provided.
    /// </summary>
    [Fact]
    public Task FieldMapping_WithCombineFieldsRule_ShouldReturnCombinedValues()
        => FieldMapping_WithMappingRuleBase_NoFieldTransform_ShouldReturnNull(new CombineFieldsRule());

    /// <summary>
    /// A field mapping with the <see cref="CopyRule"/> should return null 
    /// when no transforms are provided.
    /// </summary>
    [Fact]
    public Task FieldMapping_WithCopyRule_NoFieldTransform_ShouldReturnNull()
        => FieldMapping_WithMappingRuleBase_NoFieldTransform_ShouldReturnNull(new CopyRule());

    /// <summary>
    /// A field mapping with the <see cref="IgnoreRule"/> should return null 
    /// when no transforms are provided.
    /// </summary>
    [Fact]
    public Task FieldMapping_WithIgnoreRule_NoFieldTransform_ShouldReturnNull()
        => FieldMapping_WithMappingRuleBase_NoFieldTransform_ShouldReturnNull(new IgnoreRule());
    #endregion No Field Transforms

    #region Field Transforms
    /// <summary>
    /// A field mapping with a <see cref="MappingRuleBase"/> that has <see cref="MappingRuleBase.SourceFieldTranformations" />
    /// should return the correct values.
    /// </summary>
    [Fact]
    public async Task FieldMapping_WithCombineFieldsRule_FieldTransform_ShouldReturnExpectedOutput()
    {
        // Arrange
        // Add a new data table to the ImportDataFile
        var randomTableName = Guid.NewGuid().ToString();

        var dataFile = ImportDataObjects.DataFile.Clone();
        Assert.NotNull(dataFile.DataSet);
        var mainDataSet = dataFile.DataSet;

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

        // Add the DataTable to the ImportDataFile
        mainDataSet.Tables.Add(dataTable);

        var rule = ImportDataObjects.CombineFieldsRule;
        rule.RuleDetail = "${0}-----${1}";

        var fieldDescriptors = new[]
        {
            new ImportedRecordFieldDescriptor()
            {
                FieldName = "Field 1",
                FieldType = typeof(string),
                ForTableName = randomTableName,
                ImportedDataFile = dataFile
            },
            new ImportedRecordFieldDescriptor()
            {
                FieldName = "Field 2",
                FieldType = typeof(string),
                ForTableName = randomTableName,
                ImportedDataFile = dataFile
            }
        };

        rule.AddFieldTransformation(fieldDescriptors.First());
        rule.AddFieldTransformation(fieldDescriptors.Last());

        var newFieldMapping = new FieldMapping()
        {
            FieldName = "Field 3",
            FieldType = typeof(string),
            MappingRule = rule
        };

        var expected = new[] { "Test Input-----Test Input 2", "Test Input 3-----Test Input 4" };

        // Act
        var result = (await newFieldMapping.Apply()).Select(x => x.Value).ToArray();

        // Assert
        Assert.Equal(expected, result);
    }
    #endregion Field Transforms

    #region Helpers
    /// <summary>
    /// A field mapping with the <paramref name="mappingRuleBase"/> should return null 
    /// when no transforms are provided.
    /// </summary>
    /// <param name="mappingRuleBase">
    /// The mapping rule to test. It should not have any field transforms.
    /// </param>
    private static async Task FieldMapping_WithMappingRuleBase_NoFieldTransform_ShouldReturnNull(MappingRuleBase mappingRuleBase)
    {
        // Arrange
        var fieldMapping = FieldMappingUT;
        fieldMapping.MappingRule = mappingRuleBase;

        // Act
        var result = (await fieldMapping.Apply(ImportRowUT))?.Value;

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// A field mapping with the <paramref name="mappingRuleBase"/> should return the expected output
    /// when the field transform is provided.
    /// </summary>
    private static async Task FieldMapping_WithMappingRuleBase_FieldTransform_ShouldReturnExpectedOutput(MappingRuleBase mappingRuleBase, string expectedOutput)
    {
        // Arrange
        var fieldMapping = FieldMappingUT;

        fieldMapping.MappingRule = mappingRuleBase;

        // Act
        var result = (await fieldMapping.Apply(ImportRowUT))?.Value;

        // Assert
        Assert.Equal(expectedOutput, result);
    }
    #endregion Helpers
}
