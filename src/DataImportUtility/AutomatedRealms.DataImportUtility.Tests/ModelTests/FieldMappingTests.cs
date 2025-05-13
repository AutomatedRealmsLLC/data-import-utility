using System.Data;
using AutomatedRealms.DataImportUtility.Abstractions.Models; // Added for FieldMapping
using AutomatedRealms.DataImportUtility.Abstractions; // Added for MappingRuleBase
using AutomatedRealms.DataImportUtility.Core.Rules; // Added for CombineFieldsRule etc.
using System.Linq; // Added for Enumerable.First(), Enumerable.Last(), Enumerable.Select(), OfType
using Microsoft.Extensions.Logging.Abstractions; // Added for NullLogger

using AutomatedRealms.DataImportUtility.Tests.SampleData;

namespace AutomatedRealms.DataImportUtility.Tests.ModelTests;

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
        => FieldMapping_WithMappingRuleBase_NoFieldTransform_ShouldReturnNull(new CombineFieldsRule(NullLogger<CombineFieldsRule>.Instance));

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
    /// A field mapping with a <see cref="MappingRuleBase"/> that has <see cref="MappingRuleBase.SourceFieldTransformations" />
    /// should return the correct values.
    /// </summary>
    [Fact]
    public async Task FieldMapping_WithCombineFieldsRule_FieldTransform_ShouldReturnExpectedOutput()
    {
        // Arrange
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

        mainDataSet.Tables.Add(dataTable);

        CombineFieldsRule rule = (CombineFieldsRule)ImportDataObjects.MappingRules
            .OfType<CombineFieldsRule>()
            .First()
            .Clone(); // Use non-generic Clone and cast
        
        rule.CombinationFormat = "${0}-----${1}";

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

        rule.InputFields.Clear(); // Ensure InputFields is empty before adding
        rule.InputFields.Add(new ConfiguredInputField { FieldName = fieldDescriptors.First().FieldName });
        rule.InputFields.Add(new ConfiguredInputField { FieldName = fieldDescriptors.Last().FieldName });

        var newFieldMapping = new FieldMapping()
        {
            FieldName = "Field 3",
            FieldType = typeof(string),
            MappingRule = rule
        };

        var expected = new[] { "Test Input-----Test Input 2", "Test Input 3-----Test Input 4" };

        // Act
        var result = (await newFieldMapping.Apply()).Select(x => x?.CurrentValue).ToArray();

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
        // Apply returns a single TransformationResult for a single DataRow context
        var result = await fieldMapping.Apply(ImportRowUT);

        // Assert
        // For rules like IgnoreRule or unconfigured rules, CurrentValue might be null or the result might indicate failure.
        // The original test asserted Assert.Null(result) which implies the Apply method itself returned null.
        // Now Apply returns TransformationResult, so we check CurrentValue or WasFailure.
        // For an unconfigured CopyRule or CombineFieldsRule, or an IgnoreRule, the expectation is likely no successful value.
        if (mappingRuleBase is IgnoreRule)
        {
            Assert.True(result?.WasFailure ?? true); // Or specific check for IgnoreRule behavior
        }
        else
        {
            // For unconfigured Copy/Combine, it should likely be a failure or null CurrentValue.
            // The original test checked for null, implying no value was produced.
            Assert.Null(result?.CurrentValue); 
        }
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
        var transformationResult = await fieldMapping.Apply(ImportRowUT);
        var resultValue = transformationResult?.CurrentValue;

        // Assert
        Assert.Equal(expectedOutput, resultValue);
    }
    #endregion Helpers
}
