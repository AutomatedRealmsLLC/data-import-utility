using System.Data;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.Rules; // For CombineFieldsRule
using AutomatedRealms.DataImportUtility.Tests.TestHelpers;
using AutomatedRealms.DataImportUtility.Tests.SampleData; // For ImportDataObjects
using System.Linq;
using System.Collections.Generic;
using AutomatedRealms.DataImportUtility.Abstractions; // For IMappingRule, ValueTransformationBase

namespace AutomatedRealms.DataImportUtility.Tests.MappingRuleTests;

public class CombineFieldsRuleTests : MappingRuleBaseTestContext
{
    [Fact]
    public async Task CombineRule_ApplyOnTransformResults_ShouldReturnCombinedValues()
    {
        // Arrange
        var rule = (CombineFieldsRule)ImportDataObjects.CombineFieldsRule.Clone(); 
        // In Core.CombineFieldsRule, RuleDetail is CombinationFormat
        rule.CombinationFormat = "${0}------${1}"; 
        
        // The Apply(ITransformationContext) overload in Core.CombineFieldsRule expects
        // the input to be processed by CombineFieldsTransformation.
        // CombineFieldsTransformation expects an array of objects as CurrentValue.
        var valuesForCombination = new object[] { "Test Input", "Test Input 2" };
        var inputJson = System.Text.Json.JsonSerializer.Serialize(valuesForCombination);

        // Create a context. The Apply(ITransformationContext) in CombineFieldsRule
        // will use the configured InputFields. If InputFields is empty, it might try to use context.CurrentValue.
        // For this test, let's assume we are testing the direct combination of pre-transformed values
        // that are passed via the context, and the rule's InputFields might be empty or ignored
        // if the Apply(ITransformationContext) is smart enough.
        // However, the primary way CombineFieldsRule gets its multiple values is via its InputFields.
        // The Apply(TransformationResult) overload on MappingRuleBase is likely not what we want to hit directly
        // for a CombineFieldsRule specific test if it bypasses InputFields logic.

        // Let's set up InputFields to provide constant values for simplicity if we want to test CombinationFormat
        rule.InputFields.Clear();
        rule.InputFields.Add(new ConfiguredInputField { ConstantValue = "Test Input" });
        rule.InputFields.Add(new ConfiguredInputField { ConstantValue = "Test Input 2" });

        // Create a minimal context. The Apply() method will be called.
        // The Apply() overload that takes no arguments will then create its own context.
        var results = await rule.Apply(); // This should hit the Apply() in CombineFieldsRule
        var result = results.FirstOrDefault();


        // Assert
        Assert.NotNull(result);
        Assert.False(result.WasFailure, result.ErrorMessage);
        Assert.Equal("Test Input------Test Input 2", result.CurrentValue);
    }

    [Fact]
    public async Task CombineRule_ApplyWithoutParameters_ShouldCombineFieldsForDataTable()
    {
        // Arrange
        var randomTableName = Guid.NewGuid().ToString();
        var dataFile = ImportDataObjects.DataFile.Clone(); // Clones a pre-configured ImportedDataFile
        Assert.NotNull(dataFile.DataSet);
        var mainDataSet = dataFile.DataSet;

        var dataTable = new DataTable(randomTableName);
        dataTable.Columns.Add("Field1"); // Changed to avoid space
        dataTable.Columns.Add("Field2"); // Changed to avoid space

        var row1 = dataTable.NewRow();
        row1["Field1"] = "Test Input A";
        row1["Field2"] = "Test Input B";
        dataTable.Rows.Add(row1);

        var row2 = dataTable.NewRow();
        row2["Field1"] = "Test Input C";
        row2["Field2"] = "Test Input D";
        dataTable.Rows.Add(row2);

        mainDataSet.Tables.Add(dataTable);
        
        // Ensure ImportedDataFile has the table definition set up correctly.
        // This involves creating an ImportTableDefinition and adding it to dataFile.TableDefinitions.
        // The ImportTableDefinition needs FieldDescriptors.
        ImportTableDefinition? tableDef = dataFile.TableDefinitions.FirstOrDefault(td => td.TableName == randomTableName);
        if (tableDef == null)
        {
            tableDef = new ImportTableDefinition
            {
                TableName = randomTableName,             // FieldDescriptors for ImportTableDefinition should be ImmutableList<ImportedRecordFieldDescriptor>
                FieldDescriptors =
                [
                    new ImportedRecordFieldDescriptor { FieldName = "Field1", FieldType = typeof(string), ForTableName = randomTableName, ImportedDataFile = dataFile },
                    new ImportedRecordFieldDescriptor { FieldName = "Field2", FieldType = typeof(string), ForTableName = randomTableName, ImportedDataFile = dataFile }
                ]
            };
            dataFile.TableDefinitions.Add(tableDef);
        }
        // Crucially, ImportedDataFile needs to be told to process the new DataTable structure
        dataFile.RefreshFieldDescriptors(false); // This should populate/update field descriptors based on DataSet

        var combineRule = (CombineFieldsRule)ImportDataObjects.CombineFieldsRule.Clone();
        combineRule.CombinationFormat = "${0} ${1}";
        
        // Set ParentTableDefinition for context. This is vital for the rule to find the table.
        combineRule.ParentTableDefinition = dataFile.TableDefinitions.First(td => td.TableName == randomTableName);

        // Populate the InputFields list of the CombineFieldsRule
        combineRule.InputFields.Clear(); // Clear any existing from clone
        combineRule.InputFields.Add(new ConfiguredInputField { FieldName = "Field1" });
        combineRule.InputFields.Add(new ConfiguredInputField { FieldName = "Field2" });

        var expected = new[] { "Test Input A Test Input B", "Test Input C Test Input D" };

        // Act
        // Apply() on CombineFieldsRule should use its ParentTableDefinition and InputFields
        // to fetch data from the DataTable.
        var results = await combineRule.Apply(dataTable); // Use Apply(DataTable)
        
        Assert.NotNull(results);
        var actualValues = results.Where(r => r != null && !r.WasFailure).Select(x => x!.CurrentValue).ToArray();

        // Assert
        Assert.Equal(expected.Length, actualValues.Length);
        for(int i=0; i<expected.Length; i++)
        {
            Assert.Equal(expected[i], actualValues[i]);
        }
    }
}
