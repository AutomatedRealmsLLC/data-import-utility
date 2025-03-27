using DataImportUtility.Models;
using DataImportUtility.Rules;
using DataImportUtility.Tests.SampleData;
using DataImportUtility.Tests.TestHelpers;

namespace DataImportUtility.Tests.MappingRuleTests;

public class CopyRuleTests : MappingRuleBaseTestContext
{
    /// <summary>
    /// The <see cref="CopyRule"/> should return the provided input.
    /// </summary>
    [Fact]
    public async Task CopyRule_ShouldReturnProvidedTransformValue()
    {
        // Arrange
        var input = ImportDataObjects.TransformResultForRuleInput;
        var expectedOutput = input.Value;

        // Act
        var result = (await ImportDataObjects.CopyRule.Clone().Apply(input)).Value;

        // Assert
        Assert.Equal(expectedOutput, result);
    }

    /// <summary>
    /// The <see cref="CopyRule"/> should return a failure result when the input is a collection.
    /// </summary>
    [Fact]
    public async Task CopyRule_ShouldReturnFailureResultWhenInputIsCollection()
    {
        // Arrange
        var input = new TransformationResult() { Value = @"[""Test Input"",""Test Input 2""]" };

        // Act
        var result = await ImportDataObjects.CopyRule.Clone().Apply(input);

        // Assert
        Assert.True(result.WasFailure);
    }
}
