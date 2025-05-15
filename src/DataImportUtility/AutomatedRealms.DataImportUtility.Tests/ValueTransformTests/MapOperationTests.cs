using AutomatedRealms.DataImportUtility.Tests.SampleData;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;
using AutomatedRealms.DataImportUtility.Abstractions; // Added for ValueTransformationBase
using System.Collections.Generic; // Added for Dictionary
using System.Linq; // Added for ToDictionary

namespace AutomatedRealms.DataImportUtility.Tests.ValueTransformTests;

public class MapOperationTests
{
    /// <summary>
    /// Tests that the <see cref="MapTransformation"/> works as expected.
    /// </summary>
    [Theory]
    [MemberData(nameof(ImportDataObjects.ValidInputData), MemberType = typeof(ImportDataObjects))]
    private async Task MapOperation_WorksOnValidInput(string fieldName, string input, List<ValueMap> valueMappingsFromTestData, string expected)
    {
        // Arrange
        var operation = new MapTransformation()
        {
            Mappings = valueMappingsFromTestData
                .Where(vm => vm.FromValue is not null && vm.ToValue is not null) // Ensure keys and values are not null
                .GroupBy(vm => vm.FromValue) // Group by keys to handle duplicates
                .ToDictionary(
                    g => g.Key!, // Key is guaranteed not null from Where clause
                    g => g.First().ToValue! // Take first value for each group
                )
        };

        var initialResult = TransformationResult.Success(input, input?.GetType(), input, input?.GetType());

        // Act
        var result = await operation.ApplyTransformationAsync(initialResult);

        // Assert
        Assert.False(result.WasFailure, result.ErrorMessage ?? "Error message was null.");
        Assert.Equal(expected, result.CurrentValue);
    }
    
    /// <summary>
    /// Tests that the <see cref="MapTransformation"/> fails when applied to a collection.
    /// </summary>
    [Fact]
    private async Task MapOperation_FailsOnCollection()
    {
        // Arrange
        var collectionInput = ImportDataObjects.ValueCollectionForFailures;
        var valueMappings = new List<ValueMap>()
        {
            // ImportedFieldName is not directly used by MapTransformation, but FromValue and ToValue are
            new() { ImportedFieldName = "Test", FromValue = "1234567890", ToValue = "Mapped" }
        };

        var operation = new MapTransformation()
        {
            Mappings = valueMappings
                .Where(vm => vm.FromValue is not null && vm.ToValue is not null)
                .ToDictionary(vm => vm.FromValue!, vm => vm.ToValue!)
        };
        
        // When input is already a collection type
        var initialResult = TransformationResult.Success(
            originalValue: collectionInput, 
            originalValueType: collectionInput?.GetType(), 
            currentValue: collectionInput, 
            currentValueType: collectionInput?.GetType()
        );

        // Act
        var result = await operation.ApplyTransformationAsync(initialResult);

        // Assert
        Assert.True(result.WasFailure);
        Assert.Equal(ValueTransformationBase.OperationInvalidForCollectionsMessage, result.ErrorMessage);
    }

    /// <summary>
    /// Tests that the <see cref="MapTransformation"/> works as expected when chained with another operation (using <see cref="CalculateTransformation" />).
    /// </summary>
    [Fact]
    private async Task MapOperation_ChainedWithCalculateOperation()
    {
        // Arrange
        var input = "280-190533-1";
        var valueMappings = new List<ValueMap>()
        {
            new() { ImportedFieldName = "Test", FromValue = "280-190533-1", ToValue = "32" }
        };
        var formula = "${0} + 1.01";
        var decimalPlaces = 2;
        var expected = "33.01";

        var mapOperation = new MapTransformation()
        {
            Mappings = valueMappings
                .Where(vm => vm.FromValue is not null && vm.ToValue is not null)
                .ToDictionary(vm => vm.FromValue!, vm => vm.ToValue!)
        };

        var calculateOperation = new CalculateTransformation()
        {
            TransformationDetail = formula,
            DecimalPlaces = decimalPlaces
        };
        
        var initialResult = TransformationResult.Success(input, input?.GetType(), input, input?.GetType());

        // Act
        var result = await mapOperation.ApplyTransformationAsync(initialResult);
        if (!result.WasFailure) 
        {
            result = await calculateOperation.ApplyTransformationAsync(result);
        }

        // Assert
        Assert.False(result.WasFailure, result.ErrorMessage ?? "Error message was null.");
        Assert.Equal(expected, result.CurrentValue);
    }
}
