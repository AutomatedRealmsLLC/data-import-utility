using DataImportUtility.Models;

namespace DataImportUtility.Tests.ModelTests;

public class TransformationResultTests
{
    /// <summary>
    /// Test to check if the WasFailure property is true when the ErrorMessages property is not empty
    /// </summary>
    [Fact]
    public void WasFailure_WhenErrorMessagesIsNotEmpty_ShouldReturnTrue()
    {
        // Arrange
        var result = new TransformationResult() { ErrorMessage = "Test Error Message" };

        // Act
        var wasFailure = result.WasFailure;

        // Assert
        Assert.True(wasFailure);
    }

    /// <summary>
    /// Test to check if the WasFailure property is false when the ErrorMessages property is empty
    /// </summary>
    [Fact]
    public void WasFailure_WhenErrorMessagesIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var result = new TransformationResult();

        // Act
        var wasFailure = result.WasFailure;

        // Assert
        Assert.False(wasFailure);
    }
}
