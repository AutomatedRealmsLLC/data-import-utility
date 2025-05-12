using System.Text.RegularExpressions;

namespace AutomatedRealms.DataImportUtility.Tests.ExtensionTests;

public class StringHelperExtensionTests
{
    /// <summary>
    /// Gets the data for the tests that require a string with placeholders and the expected matches.
    /// </summary>
    public static IEnumerable<object[]> GetPlaceholderMatchesData { get; } =
    [
        ["Test ${0}", new[] { "${0}" }],
        ["Test ${0} ${1}", new[] { "${0}", "${1}" }],
        ["Test", Array.Empty<string>()]
    ];

    /// <summary>
    /// Tests that the <see cref="StringHelpers.ToStandardComparisonString(string?)"/> method returns the expected value.
    /// </summary>
    [Theory]
    [InlineData("Test", "TEST")]
    [InlineData("Test 1", "TEST1")]
    [InlineData("Test-1", "TEST1")]
    [InlineData("Test_1", "TEST1")]
    [InlineData(" Test 1-2 ", "TEST12")]
    public void ToStandardComparisonString_ShouldReturnExpectedValue(string input, string expected)
    {
        // Act
        var result = input.ToStandardComparisonString();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the <see cref="StringHelpers.ToStandardComparisonString(string?)"/> method returns the expected value when the input is null.
    /// </summary>
    [Fact]
    public void ToStandardComparisonString_ShouldReturnNullWhenInputIsNull()
    {
        // Arrange
        string? input = null;

        // Act
        var result = input.ToStandardComparisonString();

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that the <see cref="StringHelpers.OperationHasPlaceholder(string)"/> method returns true when the input contains a placeholder.
    /// </summary>
    [Theory]
    [InlineData("Test ${0}", true)]
    [InlineData("Test", false)]
    [InlineData("Test ${0", false)]
    [InlineData("Test ${0t}", false)]
    public void OperationHasPlaceHolder_ShouldReturnTrueWhenInputContainsPlaceHolder(string input, bool expected)
    {
        // Act
        var result = input.OperationHasPlaceholder();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the <see cref="StringHelpers.GetPlaceholderMatches(string)"/> method returns the expected value.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetPlaceholderMatchesData))]
    public void GetPlaceholderMatches_ShouldReturnExpectedValue(string input, string[] expected)
    {
        // Act
        var result = input.GetPlaceholderMatches()
            .OfType<Match>()
            .Select(x => x.Value)
            .ToArray();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that the <see cref="StringHelpers.TryGetPlaceholderMatches(string, out MatchCollection)"/> method returns the expected value.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetPlaceholderMatchesData))]
    public void TryGetPlaceholderMatches_ShouldReturnExpectedValue(string input, string[] expected)
    {
        // Act
        var result = input.TryGetPlaceholderMatches(out var matches);
        var matchesArray = matches
            .OfType<Match>()
            .Select(x => x.Value)
            .ToArray();

        // Assert
        Assert.Equal(expected.Length > 0, result);
        Assert.Equal(expected, matchesArray);
    }

}
