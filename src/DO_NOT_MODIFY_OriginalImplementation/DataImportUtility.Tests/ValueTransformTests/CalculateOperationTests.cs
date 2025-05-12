using DataImportUtility.ValueTransformations;

namespace DataImportUtility.Tests.ValueTransformTests;

public class CalculateOperationTests
{
    /// <summary>
    /// Tests that the <see cref="CalculateTransformation"/> works as expected.
    /// </summary>
    [Theory]
    [InlineData("Random Input", "1 + 1", 0, "2")]
    [InlineData("5493.39", "1 + 1", 0, "2")]
    [InlineData("5493.39", "1 + 1", 1, "2.0")]
    [InlineData("5493.39", "${0}", 0, "5493")]
    [InlineData("5493.39", "${0}", 1, "5493.4")]
    [InlineData("5493.39", "${0}", 2, "5493.39")]
    [InlineData("5493.39", "${0}", 3, "5493.390")]
    [InlineData("5493.39", "${0} + 1.01", 0, "5494")]
    [InlineData("5493.39", "${0} + 1.01", 1, "5494.4")]
    [InlineData("5493.39", "${0} + 1.01", 2, "5494.40")]
    public async Task CalculateOperationTest_WorksWithValidFormulae(string input, string formula, int decimalPlaces, string expected)
    {
        // Arrange
        var operation = new CalculateTransformation()
        {
            TransformationDetail = formula,
            DecimalPlaces = decimalPlaces
        };

        // Act
        var result = await operation.Apply(input);

        // Assert
        Assert.False(result.WasFailure, result.ErrorMessage);
        Assert.Equal(expected, result.Value);
    }

    /// <summary>
    /// Tests that the <see cref="CalculateTransformation"/> fails with an invalid formula.
    /// </summary>
    [Theory]
    [InlineData("5493.39", "1 + 1 +", 0, CalculateTransformation.InvalidFormatMessage)]
    [InlineData("5493.39", "${0} + 1 +", 1, CalculateTransformation.InvalidFormatMessage)]
    [InlineData("5493.39", "${0 + 1 +", 1, CalculateTransformation.InvalidFormatMessage)]
    public async Task CalculateOperationTest_FailsWithInvalidFormulae(string input, string formula, int decimalPlaces, string expected)
    {
        // Arrange
        var operation = new CalculateTransformation()
        {
            TransformationDetail = formula,
            DecimalPlaces = decimalPlaces
        };

        // Act
        var result = await operation.Apply(input);

        // Assert
        Assert.True(result.WasFailure, $"The formula produced the result: {result.Value}");
        Assert.Equal(expected, result.ErrorMessage);
    }

    /// <summary>
    /// Tests that the <see cref="CalculateTransformation" /> fails when the input is not numeric.
    /// </summary>
    [Theory]
    [InlineData("Not a number", "${0}", 0, CalculateTransformation.InvalidFormatMessage)]
    [InlineData(@"[""25"", ""Not a number""]", "${0} + ${1}", 0, CalculateTransformation.InvalidFormatMessage)]
    public async Task CalculateOperationTest_FailsWithNonNumericInput(string input, string formula, int decimalPlaces, string expected)
    {
        // Arrange
        var operation = new CalculateTransformation()
        {
            TransformationDetail = formula,
            DecimalPlaces = decimalPlaces
        };

        // Act
        var result = await operation.Apply(input);

        // Assert
        Assert.True(result.WasFailure, $"The formula produced the result: {result.Value}");
        Assert.Equal(expected, result.ErrorMessage);
    }


    // Works with RegexMatch as long as the regex gives back numeric values
    [Theory]
    [InlineData(@"[""5493.39""]", "${0}", 0, "5493")]
    [InlineData(@"[""5493.39"", ""1.02""]", "${0} + ${1}", 1, "5494.4")]
    public async Task CalculateOperationTest_WorksWithStringArray_WhenElementsAreNumeric(string input, string formula, int decimalPlaces, string expected)
    {
        // Arrange
        var operation = new CalculateTransformation()
        {
            TransformationDetail = formula,
            DecimalPlaces = decimalPlaces
        };

        // Act
        var result = await operation.Apply(input);

        // Assert
        Assert.False(result.WasFailure, result.ErrorMessage);
        Assert.Equal(expected, result.Value);
    }
}
