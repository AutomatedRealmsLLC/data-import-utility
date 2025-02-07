using System.Text.Json;

using DataImportUtility.Abstractions;
using DataImportUtility.Tests.SampleData;
using DataImportUtility.TransformOperations;

namespace DataImportUtility.Tests.ValueTransformTests;

public class SubstringOperationTests
{
    /// <summary>
    /// Tests that the <see cref="SubstringOperation"/> works as expected.
    /// </summary>
    [Theory]
    // Positive start index, positive max length
    [InlineData("1234567890", 0, 5, "12345")]
    // Positive start index, zero max length
    [InlineData("1234567890", 5, 0, "")]
    // Positive start index, positive max length (max length is greater than the length of the string)
    [InlineData("1234567890", 2, 100, "34567890")]
    // Positive start index, positive max length (start index is greater than the length of the string)
    [InlineData("1234567890", 100, 10, "")]
    // Negative start index, positive max length
    [InlineData("1234567890", -5, 3, "678")]
    // Negative start index, positive max length (abs(start index) and max length are greater than the length of the string)
    [InlineData("1234567890", -100, 100, "1234567890")]
    // Positive start index, negative max length
    [InlineData("1234567890", 2, -4, "345678")]
    // Negative start index, negative max length
    [InlineData("1234567890", -9, -2, "23456789")]
    // Positive start index, null max length
    [InlineData("1234567890", 2, null, "34567890")]
    // Negative start index, null max length
    [InlineData("1234567890", -2, null, "90")]
    private async Task SubstringOperation_WorksOnValidInput(string input, int startIndex, int? maxLength, string expected)
    {
        // Arrange
        var operation = new SubstringOperation()
        {
            StartIndex = startIndex,
            MaxLength = maxLength
        };

        // Act
        var result = await operation.Apply(input);

        // Assert
        Assert.False(result.WasFailure);
        Assert.Equal(expected, result.Value);
    }
    /// <summary>
    /// Tests that the <see cref="SubstringOperation"/> fails when applied to a collection.
    /// </summary>
    [Fact]
    private async Task SubstringOperation_FailsOnCollection()
    {
        // Arrange
        var input = JsonSerializer.Serialize(ImportDataObjects.ValueCollectionForFailures);
        var startIndex = 0;
        var maxLength = 5;

        var operation = new SubstringOperation()
        {
            StartIndex = startIndex,
            MaxLength = maxLength
        };

        // Act
        var result = await operation.Apply(input);

        // Assert
        Assert.True(result.WasFailure);
        Assert.Equal(ValueTransformationBase.OperationInvalidForCollectionsMessage, result.ErrorMessage);
    }
}
