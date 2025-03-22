using System.Text.Json;

using DataImportUtility.Models;
using DataImportUtility.ValueTransformations;

namespace DataImportUtility.Tests.ValueTransformTests;

public class CombineFieldsOperationTests
{
    /// <summary>
    /// The <seeOperation"/> should return the transformed output
    /// based on the rule detail provided when the previous result was a single string.
    /// </summary>
    [Theory]
    [InlineData("28", "Interpolated", "Interpolated")]
    [InlineData("28", "${0} Interpolated", "28 Interpolated")]
    [InlineData("28", "${0} ${1} Interpolated", "28 ${1} Interpolated")]
    public async Task CombineFieldsOperation_PreviousResultIsSingleString_ShouldReturnCorrectInterpolation(string prevResult, string operationDetail, string expectedOutput)
    {
        // Arrange
        var prevTransformResult = new TransformationResult() { Value = prevResult };

        var operation = new CombineFieldsTransformation()
        {
            TransformationDetail = operationDetail,
            SourceFieldTransforms = [new() { Field = new() { FieldName = "Field 1", ImportedDataFile = new(), ForTableName = string.Empty } }]
        };

        // Act
        var result = (await operation.Apply(prevTransformResult)).Value;

        // Assert
        Assert.Equal(expectedOutput, result);
    }

    /// <summary>
    /// The <seeOperation"/> should return the transformed output
    /// based on the rule detail provided when the previous result was a 
    /// collection of strings (combination of fields).
    /// </summary>
    [Theory]
    [InlineData(@"[""28"",""PN""]", "Interpolated", "Interpolated")]
    [InlineData(@"[""28"",""PN""]", "${0} ${1} Interpolated", "28 PN Interpolated")]
    [InlineData(@"[""28"",""PN""]", "${0} ${1} ${3} Interpolated", "28 PN ${3} Interpolated")]
    public async Task CombineFieldsOperation_PreviousResultProducedArray_ShouldReturnCorrectInterpolation(string prevResult, string operationDetail, string expectedOutput)
    {
        // Arrange
        var prevTransformResult = new TransformationResult() { Value = prevResult };

        var values = JsonSerializer.Deserialize<string[]>(prevResult) ?? [];

        var importFields = values
            .Select((_, index) => new ImportedRecordFieldDescriptor()
            {
                FieldName = $"Field {index}",
                ImportedDataFile = new(),
                ForTableName = string.Empty
            })
            .ToList();

        var fieldTransforms = importFields
            .Select(importField => new FieldTransformation(importField))
            .ToList();

        var operation = new CombineFieldsTransformation()
        {
            TransformationDetail = operationDetail,
            SourceFieldTransforms = fieldTransforms
        };

        // Act
        var result = (await operation.Apply(prevTransformResult)).Value;

        // Assert
        Assert.Equal(expectedOutput, result);
    }
}
