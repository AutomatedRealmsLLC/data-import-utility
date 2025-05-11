using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AutomatedRealms.DataImportUtility.Abstractions.Models
{
    /// <summary>
    /// Base class for all mapping rules.
    /// </summary>
    public abstract class MappingRuleBase
    {
        /// <summary>
        /// Gets the type of the mapping rule.
        /// </summary>
        /// <returns>The mapping rule type.</returns>
        public abstract MappingRuleType GetEnumValue();

        /// <summary>
        /// Indicates whether the mapping rule is empty or not configured.
        /// </summary>
        public virtual bool IsEmpty => false; // Default to false, overrides will specify actual logic

        /// <summary>
        /// Applies the mapping rule.
        /// </summary>
        /// <returns>A collection of transformation results.</returns>
        public abstract Task<IEnumerable<TransformationResult?>> Apply();

        /// <summary>
        /// Applies the mapping rule to the provided data table.
        /// </summary>
        /// <param name="data">The data table.</param>
        /// <returns>A collection of transformation results.</returns>
        public abstract Task<IEnumerable<TransformationResult?>> Apply(DataTable data);

        /// <summary>
        /// Applies the mapping rule to the provided data row.
        /// </summary>
        /// <param name="dataRow">The data row.</param>
        /// <returns>A transformation result.</returns>
        public abstract Task<TransformationResult?> Apply(DataRow dataRow);

        /// <summary>
        /// Clones the mapping rule.
        /// </summary>
        /// <returns>A clone of the mapping rule.</returns>
        public abstract MappingRuleBase Clone();
    }
}
