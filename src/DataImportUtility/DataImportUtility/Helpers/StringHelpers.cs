using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataImportUtility.Helpers;

/// <summary>
/// Provides helper methods for working with strings.
/// </summary>
public static partial class StringHelpers
{
    /// <summary>
    /// Converts the string to a version that can be used in comparisons.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>
    /// The converted value.
    /// </returns>
    public static string? ToStandardComparisonString(this string? value)
        => value?.Trim().ToUpper().Replace("_", "").Replace("-", "").Replace(" ", "");

    /// <summary>
    /// Converts the string to a camel case version.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <returns>
    /// The converted value.
    /// </returns>
    //[return: NotNullIfNotNull(nameof(val))]
    public static string? ToCamelCase(this string? val)
        => val is null or { Length: < 2 }
            ? val
            : char.ToLowerInvariant(val[0]) + string.Join("", val.Skip(1));

    /// <summary>
    /// Determines if the rule or operation detail contains a placeholder.
    /// </summary>
    /// <param name="operationDetail">The rule or operation detail to check.</param>
    /// <returns>
    /// True if the rule or operation detail contains a placeholder; otherwise, false.
    /// </returns>
    public static bool OperationHasPlaceholder(this string? operationDetail)
        => Regex.IsMatch(operationDetail ?? string.Empty, @"\$\{(\d+)\}");
        //=> PlaceholderRegex().IsMatch(operationDetail ?? string.Empty);

    /// <summary>
    /// Gets the matches for the placeholders in the rule or operation detail.
    /// </summary>
    /// <param name="operationDetail">The rule or operation detail to check.</param>
    /// <returns>
    /// The matches for the placeholders in the rule or operation detail.
    /// </returns>
    public static MatchCollection GetPlaceholderMatches(this string? operationDetail)
        => Regex.Matches(operationDetail ?? string.Empty, @"\$\{(\d+)\}");
        //=> PlaceholderRegex().Matches(operationDetail ?? string.Empty);

    /// <summary>
    /// Tries to get the <see cref="MatchCollection"/> for the placeholders in the rule or operation detail.
    /// </summary>
    /// <param name="operationDetail">The rule or operation detail to check.</param>
    /// <param name="matches">The object to store the matches in.</param>
    /// <returns>
    /// True if the rule or operation detail contains at least 1 placeholder; otherwise, false.
    /// </returns>
    public static bool TryGetPlaceholderMatches(this string? operationDetail, out MatchCollection matches)
        => (matches = Regex.Matches(operationDetail ?? string.Empty, @"\$\{(\d+)\}")).Count > 0;
        //=> (matches = PlaceholderRegex().Matches(operationDetail ?? string.Empty)).Count > 0;

    //[GeneratedRegex(@"\$\{(\d+)\}")]
    //public static partial Regex PlaceholderRegex();
}
