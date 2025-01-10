using DataImportUtility.TransformOperations;

namespace DataImportUtility.RuleDetailEngine;

/// <summary>
/// The code available to the scripting used to transform the value.
/// </summary>
public class Globals
{
    /// <summary>
    /// The instance of the <see cref="InterpolateOperation" />.
    /// </summary>
    public static InterpolateOperation InterpolationOperation { get; } = new();
}
