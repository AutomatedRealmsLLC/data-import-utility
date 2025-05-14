namespace AutomatedRealms.DataImportUtility.Abstractions;

// Forward declaration, actual content might depend on ComparisonOperationBase
// and specific operation needs.
/// <summary>
/// Defines a contract for comparison operations.
/// </summary>
public interface IComparisonOperation
{
    /// <summary>
    /// Gets the unique identifier for the operation.
    /// </summary>
    string OperationName { get; }
    /// <summary>
    /// Gets the display name of the operation.
    /// </summary>
    string DisplayName { get; }
    /// <summary>
    /// Gets the short name of the operation.
    /// </summary>
    string ShortName { get; }
    /// <summary>
    /// Gets a description providing additional details or context.
    /// </summary>
    string Description { get; }
    /// <summary>
    /// Gets the type of the operation.
    /// </summary>
    /// <param name="inputValue">Value to be compared.</param>
    /// <param name="comparisonValues">Values to be compared.</param>
    /// <returns>The type of the operation.</returns>
    bool Evaluate(object? inputValue, params object?[] comparisonValues);
    // Consider if it needs to be generic or work with TransformationResult
}
