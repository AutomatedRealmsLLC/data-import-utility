using AutomatedRealms.DataImportUtility.Abstractions.Models;

using System.Text.Json.Serialization;

namespace AutomatedRealms.DataImportUtility.Abstractions;

/// <summary>
/// Base class for comparison operations
/// </summary>
public abstract class ComparisonOperationBase
{
    /// <summary>
    /// Gets the unique identifier for this type of comparison operation.
    /// This is used for serialization and deserialization.
    /// </summary>
    public string TypeId { get; protected set; }

    /// <summary>
    /// The display name of the operation
    /// </summary>
    [JsonIgnore] // Typically not needed for serialization if TypeId is primary identifier
    public abstract string DisplayName { get; }

    /// <summary>
    /// The description of the operation
    /// </summary>
    [JsonIgnore] // Typically not needed for serialization
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
    /// <param name="typeId">The unique identifier for this comparison operation type.</param>
    protected ComparisonOperationBase(string typeId)
    {
        if (string.IsNullOrWhiteSpace(typeId))
        {
            throw new ArgumentException("TypeId cannot be null or whitespace.", nameof(typeId));
        }
        TypeId = typeId;
    }

    /// <summary>
    /// Configures the operands for the comparison operation.
    /// Derived classes should override this to assign operands to their specific properties (e.g., LeftOperand, RightOperand, LowLimit, HighLimit)
    /// and perform any necessary validation (e.g., checking for null if an operand is required).
    /// </summary>
    /// <param name="leftOperand">The primary rule providing the value to be compared (usually the source field value).</param>
    /// <param name="rightOperand">The primary rule providing the value to compare against (e.g., a static value, another field's value).</param>
    /// <param name="secondaryRightOperand">An optional secondary rule, used by operations like 'Between' for the upper limit.</param>
    public virtual void ConfigureOperands(
        MappingRuleBase leftOperand,
        MappingRuleBase? rightOperand,
        MappingRuleBase? secondaryRightOperand)
    {
        LeftOperand = leftOperand ?? throw new ArgumentNullException(nameof(leftOperand), $"Left operand cannot be null for {TypeId}.");
        // Basic operations will use RightOperand. Range operations (like Between) will override to use LowLimit and HighLimit.
        // Operations that don't need a right operand (e.g., IsNull) will override and might ignore it or validate it as null.
        RightOperand = rightOperand;
        // Similarly, secondaryRightOperand is for specific cases like BetweenOperation's HighLimit.
        // Default behavior is to assign it to HighLimit, but most operations won't use it.
        // This can be refined in overrides.
        LowLimit = rightOperand; // Default for operations that might use LowLimit (e.g. GreaterThan, Between)
        HighLimit = secondaryRightOperand; // Default for operations that might use HighLimit (e.g. Between)
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
        if (LeftOperand is not null)
        {
            clone.LeftOperand = LeftOperand.Clone();
        }
        if (RightOperand is not null)
        {
            clone.RightOperand = RightOperand.Clone();
        }
        if (LowLimit is not null)
        {
            clone.LowLimit = LowLimit.Clone();
        }
        if (HighLimit is not null)
        {
            clone.HighLimit = HighLimit.Clone();
        }
        return clone;
    }
}
