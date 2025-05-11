using AutomatedRealms.DataImportUtility.Abstractions.Models;
using System.Threading.Tasks;

namespace AutomatedRealms.DataImportUtility.Abstractions;

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
    /// The left operand for the comparison. This is a rule that provides the value.
    /// </summary>
    public virtual MappingRuleBase? LeftOperand { get; set; }

    /// <summary>
    /// The right operand for the comparison, if applicable. This is a rule that provides the value.
    /// </summary>
    public virtual MappingRuleBase? RightOperand { get; set; }

    /// <summary>
    /// The lower limit for range comparisons (e.g., 'IsBetween'). This is a rule that provides the value.
    /// </summary>
    public virtual MappingRuleBase? LowLimit { get; set; }

    /// <summary>
    /// The upper limit for range comparisons (e.g., 'IsBetween'). This is a rule that provides the value.
    /// </summary>
    public virtual MappingRuleBase? HighLimit { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ComparisonOperationBase"/> class.
    /// </summary>
    protected ComparisonOperationBase()
    {
        // Operands are intended to be set via properties by the configuring rule.
    }

    /// <summary>
    /// Evaluates the comparison operation using the provided transformation result context.
    /// The context (e.g., DataRow) is used by the operand rules to resolve their values.
    /// </summary>
    /// <param name="result">The transformation result context, which includes the DataRow for operand resolution.</param>
    /// <returns>True if the comparison is successful, otherwise false.</returns>
    public abstract Task<bool> Evaluate(TransformationResult result);

    /// <summary>
    /// Creates a deep clone of this comparison operation.
    /// </summary>
    /// <returns>A new instance with the same properties.</returns>
    public virtual ComparisonOperationBase Clone()
    {
        var clone = (ComparisonOperationBase)MemberwiseClone();
        if (LeftOperand != null)
        {
            clone.LeftOperand = (MappingRuleBase)LeftOperand.Clone();
        }
        if (RightOperand != null)
        {
            clone.RightOperand = (MappingRuleBase)RightOperand.Clone();
        }
        if (LowLimit != null)
        {
            clone.LowLimit = (MappingRuleBase)LowLimit.Clone();
        }
        if (HighLimit != null)
        {
            clone.HighLimit = (MappingRuleBase)HighLimit.Clone();
        }
        return clone;
    }
}
