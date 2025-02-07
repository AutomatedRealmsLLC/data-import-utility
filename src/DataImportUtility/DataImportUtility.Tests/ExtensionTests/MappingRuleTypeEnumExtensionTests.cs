using DataImportUtility.Abstractions;
using DataImportUtility.Rules;

namespace DataImportUtility.Tests.ExtensionTests;

public class MappingRuleTypeEnumExtensionTests
{
    /// <summary>
    /// Data for testing the MappingRuleType enum extension methods.
    /// </summary>
    /// <remarks>
    /// The CreateNewInstance method used here must have the types manually tested.
    /// Everything else uses the created class instance to test the extension methods.
    /// </remarks>
    public static IEnumerable<object[]> MappingRuleTypeWithInstanceData
    { get; } = Enum.GetValues<MappingRuleType>()
        .Select(ruleType => new object[] { ruleType, ruleType.CreateNewInstance()! });

    /// <summary>
    /// Tests that the MappingRuleType enum extension method <see cref="MappingRuleTypeExtensions.CreateNewInstance(MappingRuleType)"/> returns the correct MappingRuleBase.
    /// </summary>
    /// <param name="ruleType">The MappingRuleType to create a new instance of.</param>
    /// <param name="expectedType">The expected type of the new instance.</param>
    [Theory]
    [InlineData(MappingRuleType.CopyRule, typeof(CopyRule))]
    [InlineData(MappingRuleType.IgnoreRule, typeof(IgnoreRule))]
    [InlineData(MappingRuleType.CombineFieldsRule, typeof(CombineFieldsRule))]
    public void CreateNewInstance_ShouldReturnCorrectRuleType(MappingRuleType ruleType, Type expectedType)
    {
        // Arrange + Act
        var result = ruleType.CreateNewInstance();

        // Assert
        Assert.NotNull(result);
        Assert.IsType(expectedType, result);
    }

    /// <summary>
    /// Tests that the MappingRuleType enum extension method <see cref="MappingRuleTypeExtensions.GetClassType(MappingRuleType)" /> returns the correct MappingRuleType.
    /// </summary>
    /// <param name="ruleType">The MappingRuleType to get the class type for.</param>
    /// <param name="expectedType">The expected type of the class.</param>
    [Theory]
    [MemberData(nameof(MappingRuleTypeWithInstanceData))]
    public void GetClassType_ShouldReturnCorrectRuleType(MappingRuleType ruleType, MappingRuleBase ruleInstance)
    {
        // Arrange
        var expected = ruleInstance.GetType();

        // Act
        var result = ruleType.GetClassType();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the MappingRuleType enum extension method <see cref="MappingRuleTypeExtensions.GetDescription(MappingRuleType)" /> returns the correct description.
    /// </summary>
    [Theory]
    [MemberData(nameof(MappingRuleTypeWithInstanceData))]
    public void GetDescription_ShouldReturnCorrectDescription(MappingRuleType ruleType, MappingRuleBase ruleInstance)
    {
        // Arrange
        var expected = ruleInstance.Description;

        // Act
        var result = ruleType.GetDescription();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the MappingRuleType enum extension method <see cref="MappingRuleTypeExtensions.GetDisplayText(MappingRuleType, bool)" /> returns the correct MappingRuleType.
    /// </summary>
    [Theory]
    [MemberData(nameof(MappingRuleTypeWithInstanceData))]
    public void GetDisplayText_ShouldReturnCorrectRuleType(MappingRuleType ruleType, MappingRuleBase ruleInstance)
    {
        // Arrange
        var expected = ruleInstance.DisplayName;

        // Act
        var result = ruleType.GetDisplayText();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the MappingRuleType enum extension method <see cref="MappingRuleTypeExtensions.GetDisplayText(MappingRuleType, bool)" /> returns the correct short version of the MappingRuleType.
    /// </summary>
    [Theory]
    [MemberData(nameof(MappingRuleTypeWithInstanceData))]
    public void GetDisplayText_ShouldReturnCorrectShortRuleType(MappingRuleType ruleType, MappingRuleBase ruleInstance)
    {
        // Arrange
        var expected = ruleInstance.ShortName;

        // Act
        var result = ruleType.GetDisplayText(shortVersion: true);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the MappingRuleType enum extension method <see cref="MappingRuleTypeExtensions.GetEnumValue(MappingRuleBase)" /> returns the correct MappingRuleType.
    /// </summary>
    [Theory]
    [MemberData(nameof(MappingRuleTypeWithInstanceData))]
    public void GetEnumValue_ShouldReturnCorrectRuleType(MappingRuleType ruleType, MappingRuleBase ruleInstance)
    {
        // Arrange
        var expected = ruleType;

        // Act
        var result = ruleInstance.GetEnumValue();

        // Assert
        Assert.Equal(expected, result);
    }
}
