using System;
using System.Threading.Tasks;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;
using AutomatedRealms.DataImportUtility.Tests.SampleData; // Corrected namespace
using Xunit;

namespace AutomatedRealms.DataImportUtility.Tests.ValueTransformTests;

public class InterpolateOperationTests
{
    [Theory]
    [MemberData(nameof(ImportDataObjects.InterpolateOperationHappyPathInputs), MemberType = typeof(ImportDataObjects))]
    public async Task InterpolateOperation_HappyPath(string _, string pattern, string sourceValue, string expectedValue) // Assuming description is first and unused
    {
        var operation = new InterpolateTransformation
        {
            TransformationDetail = pattern
        };

        // The Transform method takes the source value and target type.
        // It internally creates an initial TransformationResult.
        var result = await operation.Transform(sourceValue, typeof(string));

        Assert.False(result.WasFailure); // Success means WasFailure is false
        Assert.Equal(expectedValue, result.CurrentValue);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(ImportDataObjects.InterpolateOperationInvalidPatternInputs), MemberType = typeof(ImportDataObjects))]
    public async Task InterpolateOperation_InvalidPattern(string _, string pattern, string sourceValue, string expectedErrorMessage) // Assuming description is first and unused
    {
        var operation = new InterpolateTransformation
        {
            TransformationDetail = pattern
        };

        var result = await operation.Transform(sourceValue, typeof(string));

        Assert.True(result.WasFailure); // Failure means WasFailure is true
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains(expectedErrorMessage, result.ErrorMessage);
    }
}
