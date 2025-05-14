using System.Text.Json;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

namespace AutomatedRealms.DataImportUtility.Tests.ValueTransformTests;

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
        var prevTransformResult = TransformationResult.Success(prevResult, prevResult?.GetType(), prevResult, prevResult?.GetType());

        var operation = new CombineFieldsTransformation()
        {
            TransformationDetail = operationDetail,
            SourceFieldTransforms = [new FieldTransformation(new ImportedRecordFieldDescriptor { FieldName = "Field 1" })]
        };

        // Act
        var result = (await operation.ApplyTransformationAsync(prevTransformResult)).CurrentValue;

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
        var prevTransformResult = TransformationResult.Success(prevResult, prevResult?.GetType(), prevResult, prevResult?.GetType());

        var values = JsonSerializer.Deserialize<string[]>(prevResult ?? "[]") ?? [];

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
        var result = (await operation.ApplyTransformationAsync(prevTransformResult)).CurrentValue;

        // Assert
        Assert.Equal(expectedOutput, result);
    }
}
