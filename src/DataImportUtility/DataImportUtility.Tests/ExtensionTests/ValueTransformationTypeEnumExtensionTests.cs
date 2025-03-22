using DataImportUtility.Abstractions;
using DataImportUtility.ValueTransformations;

namespace DataImportUtility.Tests.ExtensionTests;

public class ValueTransformationTypeEnumExtensionTests
{
    /// <summary>
    /// Data for testing the ValueTransformationType enum extension methods.
    /// </summary>
    /// <remarks>
    /// The CreateNewInstance method used here must have the types manually tested.
    /// Everything else uses the created class instance to test the extension methods.
    /// </remarks>
    public static IEnumerable<object[]> ValueTransformationTypeWithInstanceData
    { get; } = Enum.GetValues<ValueTransformationType>()
        .Select(opType => new object[] { opType, opType.CreateNewInstance()! });

    /// <summary>
    /// Tests that the ValueTransformationType enum extension method <see cref="ValueTransformationTypeExtensions.CreateNewInstance(ValueTransformationType)"/> returns the correct ValueTransformationBase.
    /// </summary>
    /// <param name="opType">The ValueTransformationType to create a new instance of.</param>
    /// <param name="expectedType">The expected type of the new instance.</param>
    [Theory]
    [InlineData(ValueTransformationType.CalculateTransformation, typeof(CalculateTransformation))]
    [InlineData(ValueTransformationType.CombineFieldsTransformation, typeof(CombineFieldsTransformation))]
    [InlineData(ValueTransformationType.InterpolateTransformation, typeof(InterpolateTransformation))]
    [InlineData(ValueTransformationType.MapTransformation, typeof(MapTransformation))]
    [InlineData(ValueTransformationType.RegexMatchTransformation, typeof(RegexMatchTransformation))]
    [InlineData(ValueTransformationType.SubstringTransformation, typeof(SubstringTransformation))]
    public void CreateNewInstance_ShouldReturnCorrectRuleType(ValueTransformationType opType, Type expectedType)
    {
        // Arrange + Act
        var result = opType.CreateNewInstance();

        // Assert
        Assert.NotNull(result);
        Assert.IsType(expectedType, result);
    }

    /// <summary>
    /// Tests that the ValueTransformationType enum extension method <see cref="ValueTransformationTypeExtensions.GetClassType(ValueTransformationType)" /> returns the correct ValueTransformationType.
    /// </summary>
    /// <param name="opType">The ValueTransformationType to get the class type for.</param>
    /// <param name="expectedType">The expected type of the class.</param>
    [Theory]
    [MemberData(nameof(ValueTransformationTypeWithInstanceData))]
    public void GetClassType_ShouldReturnCorrectRuleType(ValueTransformationType opType, ValueTransformationBase operationInstance)
    {
        // Arrange
        var expectedType = operationInstance.GetType();

        // Act
        var result = opType.GetClassType();

        // Assert
        Assert.Equal(expectedType, result);
    }

    /// <summary>
    /// Tests that the ValueTransformationType enum extension method <see cref="ValueTransformationTypeExtensions.GetDescription(ValueTransformationType)" /> returns the correct description.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValueTransformationTypeWithInstanceData))]
    public void GetDescription_ShouldReturnCorrectDescription(ValueTransformationType opType, ValueTransformationBase operationInstance)
    {
        // Arrange
        var expected = operationInstance.Description;

        // Act
        var result = opType.GetDescription();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the ValueTransformationType enum extension method <see cref="ValueTransformationTypeExtensions.GetDisplayText(ValueTransformationType, bool)" /> returns the correct ValueTransformationType.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValueTransformationTypeWithInstanceData))]
    public void GetDisplayText_ShouldReturnCorrectRuleType(ValueTransformationType opType, ValueTransformationBase operationInstance)
    {
        // Arrange
        var expected = operationInstance.DisplayName;

        // Act
        var result = opType.GetDisplayText();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the ValueTransformationType enum extension method <see cref="ValueTransformationTypeExtensions.GetDisplayText(ValueTransformationType, bool)" /> returns the correct short version of the ValueTransformationType.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValueTransformationTypeWithInstanceData))]
    public void GetDisplayText_ShouldReturnCorrectShortRuleType(ValueTransformationType opType, ValueTransformationBase opInstance)
    {
        // Arrange
        var expected = opInstance.ShortName;

        // Act
        var result = opType.GetDisplayText(shortVersion: true);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the ValueTransformationType enum extension method <see cref="ValueTransformationTypeExtensions.GetEnumValue(ValueTransformationBase)" /> returns the correct ValueTransformationType.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValueTransformationTypeWithInstanceData))]
    public void GetEnumValue_ShouldReturnCorrectRuleType(ValueTransformationType opType, ValueTransformationBase operationInstance)
    {
        // Arrange
        var expected = opType;

        // Act
        var result = operationInstance.GetEnumValue();

        // Assert
        Assert.Equal(expected, result);
    }
}
