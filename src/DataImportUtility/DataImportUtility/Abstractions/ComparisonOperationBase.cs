using DataImportUtility.Models;

namespace DataImportUtility.Abstractions;

/// <summary>
/// Base class for comparison operations
/// </summary>
public abstract class ComparisonOperationBase
{
    /// <summary>
    /// The name of the enum member
    /// </summary>
    public abstract string EnumMemberName { get; }

    /// <summary>
    /// The display name of the operation
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// The description of the operation
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// The left operand for the comparison
    /// </summary>
    public virtual MappingRuleBase? LeftOperand { get; set; }

    /// <summary>
    /// The right operand for the comparison, if applicable
    /// </summary>
    public virtual MappingRuleBase? RightOperand { get; set; }

    /// <summary>
    /// Evaluates the comparison operation
    /// </summary>
    /// <returns>True if the comparison is successful, otherwise false</returns>
    public abstract Task<bool> Evaluate(TransformationResult result);
}
