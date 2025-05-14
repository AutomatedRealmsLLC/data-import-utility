using AutomatedRealms.DataImportUtility.Abstractions.Models; // Added for TransformationResult
using AutomatedRealms.DataImportUtility.Core.Rules; // Added for CopyRule
using AutomatedRealms.DataImportUtility.Tests.SampleData;
using AutomatedRealms.DataImportUtility.Tests.TestHelpers;

namespace AutomatedRealms.DataImportUtility.Tests.MappingRuleTests;

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
        var expectedOutput = input.CurrentValue; // Updated to CurrentValue

        // Act
        var result = await ImportDataObjects.CopyRule.Clone().Apply(input); // Apply returns TransformationResult

        // Assert
        Assert.NotNull(result); // Add null check for result
        Assert.False(result.WasFailure, result.ErrorMessage);
        Assert.Equal(expectedOutput, result.CurrentValue); // Updated to CurrentValue
    }

    /// <summary>
    /// The <see cref="CopyRule"/> should return a failure result when the input is a collection.
    /// </summary>
    [Fact]
    public async Task CopyRule_ShouldReturnFailureResultWhenInputIsCollection()
    {
        // Arrange
        // Create a TransformationResult with a collection as CurrentValue
        var inputValue = new List<string> { "Test Input", "Test Input 2" };
        var input = TransformationResult.Success(
            originalValue: inputValue,
            originalValueType: inputValue.GetType(),
            currentValue: inputValue,
            currentValueType: inputValue.GetType(),
            appliedTransformations: null,
            record: null,
            tableDefinition: null,
            sourceRecordContext: null,
            targetFieldType: typeof(string) // Target type for the rule
        );

        var rule = ImportDataObjects.CopyRule.Clone();

        // Act
        var result = await rule.Apply(input);

        // Assert
        Assert.NotNull(result); // Add null check for result
        Assert.True(result.WasFailure);
        // Optionally, check for a specific error message if the rule provides one for collection inputs
        // Assert.Contains("Cannot copy a collection", result.ErrorMessage);
    }
}
