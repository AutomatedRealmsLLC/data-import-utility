using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;

namespace AutomatedRealms.DataImportUtility.Tests.ValueTransformTests;

public class RegexMatchOperationTests
{
    /// <summary>
    /// Tests that the <see cref="RegexMatchTransformation"/> works as expected.
    /// </summary>
    [Theory]
    [InlineData("280-190533-1", @"\d+", @"[""280"",""190533"",""1""]")]
    [InlineData("abc-190533-d", @"\d+", "190533")]
    [InlineData("2801905331", @"\D+", "")]
    private async Task RegexMatchOperation_WorksOnValidInput(string input, string regExPattern, string expected)
    {
        // Arrange
        var operation = new RegexMatchTransformation()
        {
            TransformationDetail = regExPattern
        };

        // Act
        var result = await operation.Transform(input, typeof(string));

        // Assert
        Assert.False(result.WasFailure);
        Assert.Equal(expected, result.CurrentValue);
    }

    /// <summary>
    /// Tests that the <see cref="RegexMatchTransformation"/> works as expected when chained with another operation (using <see cref="InterpolateTransformation" />).
    /// </summary>
    [Fact]
    private async Task RegexMatchOperation_ChainedWithInterpolateOperation()
    {
        // Arrange
        var input = "280-190533-1";
        var regExPattern = @"\d+";
        var interpolationPattern = "Sample For ${0}|${1}";
        var expected = "Sample For 280|190533";

        var regExOperation = new RegexMatchTransformation()
        {
            TransformationDetail = regExPattern
        };

        var interpolateOperation = new InterpolateTransformation()
        {
            TransformationDetail = interpolationPattern
        };

        // Act
        var regexResult = await regExOperation.Transform(input, typeof(string));
        
        // Convert the JSON array result to an actual string array object
        // that the interpolate transformation can properly use
        var resultAsArray = System.Text.Json.JsonSerializer.Deserialize<string[]>(regexResult.CurrentValue!.ToString());
        
        var intermediateResult = TransformationResult.Success(
            originalValue: regexResult.OriginalValue,
            originalValueType: regexResult.OriginalValueType,
            currentValue: resultAsArray,
            currentValueType: typeof(string[]),
            appliedTransformations: regexResult.AppliedTransformations,
            record: regexResult.Record,
            tableDefinition: regexResult.TableDefinition,
            sourceRecordContext: regexResult.SourceRecordContext,
            targetFieldType: regexResult.TargetFieldType
        );
        
        var result = await interpolateOperation.ApplyTransformationAsync(intermediateResult);

        // Assert
        Assert.False(result.WasFailure, result.ErrorMessage);
        Assert.Equal(expected, result.CurrentValue);
    }
}
