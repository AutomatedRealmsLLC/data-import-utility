using DataImportUtility.Tests.SampleData;

namespace AutomatedRealms.DataImportUtility.Tests.ValueTransformTests;

public class MapOperationTests
{
    /// <summary>
    /// Tests that the <see cref="MapTransformation"/> works as expected.
    /// </summary>
    [Theory]
    [MemberData(nameof(ImportDataObjects.ValidInputData), MemberType = typeof(ImportDataObjects))]
    private async Task MapOperation_WorksOnValidInput(string? fieldName, string input, List<ValueMap> valueMappings, string expected)
    {
        // Arrange
        var operation = new MapTransformation()
        {
            FieldName = fieldName,
            ValueMappings = valueMappings
        };

        // Act
        var result = await operation.Apply(input);

        // Assert
        Assert.False(result.WasFailure);
        Assert.Equal(expected, result.Value);
    }
    /// <summary>
    /// Tests that the <see cref="MapTransformation"/> fails when applied to a collection.
    /// </summary>
    [Fact]
    private async Task MapOperation_FailsOnCollection()
    {
        // Arrange
        var input = System.Text.Json.JsonSerializer.Serialize(ImportDataObjects.ValueCollectionForFailures);
        var fieldName = "Test";
        var valueMappings = new List<ValueMap>()
        {
            new() { ImportedFieldName = fieldName, FromValue = "1234567890", ToValue = "Mapped" }
        };

        var operation = new MapTransformation()
        {
            FieldName = fieldName,
            ValueMappings = valueMappings
        };

        // Act
        var result = await operation.Apply(input);

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
        var fieldName = "Test";
        var valueMappings = new List<ValueMap>()
        {
            new() { ImportedFieldName = fieldName, FromValue = "280-190533-1", ToValue = "32" }
        };
        var formula = "${0} + 1.01";
        var decimalPlaces = 2;
        var expected = "33.01";

        var mapOperation = new MapTransformation()
        {
            FieldName = fieldName,
            ValueMappings = valueMappings
        };

        var calculateOperation = new CalculateTransformation()
        {
            TransformationDetail = formula,
            DecimalPlaces = decimalPlaces
        };

        // Act
        var result = await mapOperation.Apply(input);
        result = await calculateOperation.Apply(result);

        // Assert
        Assert.False(result.WasFailure, result.ErrorMessage);
        Assert.Equal(expected, result.Value);
    }
}
