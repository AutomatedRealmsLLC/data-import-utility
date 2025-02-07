using DataImportUtility.Models;
using DataImportUtility.TransformOperations;

namespace DataImportUtility.Tests.ValueTransformTests;

public class InterpolateOperationTests
{
    /// <summary>
    /// Tests that the <see cref="InterpolateOperation"/> works as expected.
    /// </summary>
    [Theory]
    [InlineData("280-190533-1", "Sample For ${0}", "Sample For 280-190533-1")]
    [InlineData("280-190533-1", "Sample For ${1}", "Sample For ${1}")]
    [InlineData("280-190533-1", "Sample For ${0}-${1}", "Sample For 280-190533-1-${1}")]
    [InlineData(@"[""280"",""190533"",""1""]", "Sample For ${0}-${1}", "Sample For 280-190533")]
    [InlineData(@"[""280"",""190533"",""1""]", "Sample For ${0}-${1}-${3}", "Sample For 280-190533-${3}")]
    private async Task InterpolateOperation_WorksOnValidInput(string input, string format, string expected)
    {
        // Arrange
        var operation = new InterpolateOperation()
        {
            OperationDetail = format
        };

        // Act
        var result = await operation.Apply(input);

        // Assert
        Assert.False(result.WasFailure);
        Assert.Equal(expected, result.Value);
    }

    /// <summary>
    /// Tests that the <see cref="InterpolateOperation"/> works as expected when chained with another operation (using <see cref="MapOperation" />).
    /// </summary>
    [Fact]
    private async Task InterpolateOperation_ChainedWithMapOperation()
    {
        // Arrange
        var input = "280-190533-1";
        var interpolationPattern = "Sample For ${0}";
        var fieldName = "Test";
        var valueMappings = new List<ValueMap>()
        {
            new() { ImportedFieldName = fieldName, FromValue = $"Sample For {input}", ToValue = "Mapped" }
        };
        var expected = "Mapped";

        var interpolateOperation = new InterpolateOperation()
        {
            OperationDetail = interpolationPattern
        };

        var mapOperation = new MapOperation()
        {
            FieldName = fieldName,
            ValueMappings = valueMappings
        };

        // Act
        var result = await interpolateOperation.Apply(input);
        result = await mapOperation.Apply(result);

        // Assert
        Assert.False(result.WasFailure, result.ErrorMessage);
        Assert.Equal(expected, result.Value);
    }
}
