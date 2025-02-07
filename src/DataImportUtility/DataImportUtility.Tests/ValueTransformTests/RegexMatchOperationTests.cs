using DataImportUtility.TransformOperations;

namespace DataImportUtility.Tests.ValueTransformTests;

public class RegexMatchOperationTests
{
    /// <summary>
    /// Tests that the <see cref="RegexMatchOperation"/> works as expected.
    /// </summary>
    [Theory]
    [InlineData("280-190533-1", @"\d+", @"[""280"",""190533"",""1""]")]
    [InlineData("abc-190533-d", @"\d+", "190533")]
    [InlineData("2801905331", @"\D+", "")]
    private async Task RegexMatchOperation_WorksOnValidInput(string input, string regExPattern, string expected)
    {
        // Arrange
        var operation = new RegexMatchOperation()
        {
            OperationDetail = regExPattern
        };

        // Act
        var result = await operation.Apply(input);

        // Assert
        Assert.False(result.WasFailure);
        Assert.Equal(expected, result.Value);
    }

    /// <summary>
    /// Tests that the <see cref="RegexMatchOperation"/> works as expected when chained with another operation (using <see cref="InterpolateOperation" />).
    /// </summary>
    [Fact]
    private async Task RegexMatchOperation_ChainedWithInterpolateOperation()
    {
        // Arrange
        var input = "280-190533-1";
        var regExPattern = @"\d+";
        var interpolationPattern = "Sample For ${0}|${1}";
        var expected = "Sample For 280|190533";

        var regExOperation = new RegexMatchOperation()
        {
            OperationDetail = regExPattern
        };

        var interpolateOperation = new InterpolateOperation()
        {
            OperationDetail = interpolationPattern
        };

        // Act
        var result = await regExOperation.Apply(input);
        result = await interpolateOperation.Apply(result);

        // Assert
        Assert.False(result.WasFailure, result.ErrorMessage);
        Assert.Equal(expected, result.Value);
    }
}
