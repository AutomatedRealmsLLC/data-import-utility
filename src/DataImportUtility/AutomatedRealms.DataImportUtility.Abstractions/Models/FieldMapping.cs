using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

// Namespace will be AutomatedRealms.DataImportUtility.Abstractions.Models
// TransformationResult is in AutomatedRealms.DataImportUtility.Abstractions.Models
// MappingRuleBase is in AutomatedRealms.DataImportUtility.Abstractions
// RuleType is an enum in AutomatedRealms.DataImportUtility.Abstractions.Enums

using AutomatedRealms.DataImportUtility.Abstractions.Enums;

namespace AutomatedRealms.DataImportUtility.Abstractions.Models;

/// <summary>
/// Represents a mapping between a field and the mapping rules to use to get the value.
/// </summary>
public class FieldMapping
{
    private readonly Dictionary<string, List<ValidationResult>?> _valueValidationResults = [];

    /// <summary>
    /// The name of the field.
    /// </summary>
    public string? FieldName { get; init; } // Made nullable to avoid issues with 'required' in netstandard2.0

    /// <summary>
    /// The type of the field.
    /// </summary>
    [JsonIgnore]
    public Type FieldType
    {
        get => _fieldType;
        set
        {
            if (value == _fieldType) { return; }
            _fieldType = value;

            var fieldTypeString = _fieldType.ToString();
            if (FieldTypeString == fieldTypeString) { return; }
            FieldTypeString = value.FullName!.ToString();
        }
    }
    private Type _fieldType = typeof(object);

    /// <summary>
    /// The value type for the source field as a string.
    /// </summary>
    public string FieldTypeString
    {
        get => _fieldTypeString;
        set
        {
            if (value == _fieldTypeString) { return; }
            _fieldTypeString = value;

            var type = Type.GetType(value);
            if (type is null || type == FieldType) { return; }
            _fieldType = type;
        }
    }
    private string _fieldTypeString = typeof(object).ToString();

    /// <summary>
    /// The validation attributes for the field.
    /// </summary>
    [JsonIgnore]
    internal ImmutableArray<ValidationAttribute> ValidationAttributes { get; set; } = ImmutableArray<ValidationAttribute>.Empty; // Changed from []

    /// <summary>
    /// The validation results for the distinct values.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, List<ValidationResult>?> ValueValidationResults => _valueValidationResults;

    /// <summary>
    /// Whether the field has validation errors.
    /// </summary>
    [JsonIgnore]
    public bool HasValidationErrors => _valueValidationResults.Sum(x => x.Value?.Count(y => !string.IsNullOrWhiteSpace(y?.ErrorMessage)) ?? 0) > 0;

    /// <summary>
    /// The mapping rule to use to get the values.
    /// </summary>
    public MappingRuleBase? MappingRule { get; set; } // From AutomatedRealms.DataImportUtility.Abstractions

    /// <summary>
    /// The type of the mapping rule.
    /// </summary>
    public RuleType MappingRuleType => MappingRule?.RuleType ?? RuleType.IgnoreRule; // Use MappingRule.RuleType directly

    /// <summary>
    /// Whether the field is required to be mapped.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Whether to ignore the mapping.
    /// </summary>
    public bool IgnoreMapping => MappingRuleType == RuleType.IgnoreRule
        || (MappingRule?.IsEmpty ?? true);

    /// <summary>
    /// Applies the mapping rule to any available data in the child <see cref="MappingRule" />.
    /// </summary>
    /// <returns>The results of the transformation.</returns>
    // TransformationResult is in AutomatedRealms.DataImportUtility.Abstractions.Models
    public Task<IEnumerable<TransformationResult>> Apply() 
    {
        if (MappingRule == null)
        {
            return Task.FromResult(Enumerable.Empty<TransformationResult>());
        }
        // This version of Apply is problematic as MappingRuleBase.GetValue requires a DataRow.
        // Returning empty for now to allow compilation. This needs review.
        return Task.FromResult(Enumerable.Empty<TransformationResult>());
    }

    /// <summary>
    /// Applies the mapping rule to the provided data.
    /// </summary>
    /// <param name="data">The data to apply the mapping rule to.</param>
    /// <returns>The results of the transformation.</returns>
    // TransformationResult is in AutomatedRealms.DataImportUtility.Abstractions.Models
    public async Task<IEnumerable<TransformationResult?>> Apply(DataTable data) 
    {
        if (MappingRule == null)
        {
            return Enumerable.Empty<TransformationResult?>();
        }
        var results = new List<TransformationResult?>();
        foreach (DataRow row in data.Rows)
        {
            results.Add(await MappingRule.GetValue(row, this.FieldType).ConfigureAwait(false));
        }
        return results;
    }

    /// <summary>
    /// Applies the mapping rule to the provided data row.
    /// </summary>
    /// <param name="dataRow">The data row to apply the mapping rule to.</param>
    /// <returns>The results of the transformation.</returns>
    // TransformationResult is in AutomatedRealms.DataImportUtility.Abstractions.Models
    public async Task<TransformationResult?> Apply(DataRow dataRow) 
    {
        if (MappingRule == null)
        {
            return null;
        }
        return await MappingRule.GetValue(dataRow, this.FieldType).ConfigureAwait(false);
    }

    /// <summary>
    /// Clones the <see cref="FieldMapping" />.
    /// </summary>
    /// <returns>The cloned <see cref="FieldMapping" />.</returns>
    public FieldMapping Clone()
    {
        var forRet = (FieldMapping)MemberwiseClone();
        forRet.MappingRule = MappingRule?.Clone();
        return forRet;
    }

#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
    // TransformationResult is in AutomatedRealms.DataImportUtility.Abstractions.Models
    internal bool Validate(TransformationResult? transformedResult, out List<ValidationResult>? validationResults, bool useCache = true) 
#else
    // TransformationResult is in AutomatedRealms.DataImportUtility.Abstractions.Models
    internal bool Validate(TransformationResult? transformedResult, out List<ValidationResult>? validationResults, bool useCache = true) 
#endif
    {
        if (transformedResult?.CurrentValue is null) // Changed from transformedResult?.Value to transformedResult?.CurrentValue
        {
            validationResults = Required ? [new ValidationResult("The field is required.", [FieldName])] : null;
            if (validationResults is { Count: > 0 })
            {
                if (!_valueValidationResults.ContainsKey("<null>"))
                {
                    _valueValidationResults.Add("<null>", validationResults);
                }
                else
                {
                    _valueValidationResults["<null>"] = validationResults; // Or handle collision appropriately
                }
            }
            return validationResults is null;
        }

        // Ensure CurrentValue is converted to string for key, as Dictionary keys are strings.
        var valueKey = transformedResult.CurrentValue?.ToString() ?? "<null>";

        if (!useCache && _valueValidationResults.ContainsKey(valueKey))
        {
            _valueValidationResults.Remove(valueKey);
        }

        if (_valueValidationResults.TryGetValue(valueKey, out var results))
        {
            validationResults = results;
            return validationResults is null;
        }

        var valResults = new List<ValidationResult>();
        foreach (var validationAttribute in ValidationAttributes)
        {
            var isValid = validationAttribute.IsValid(transformedResult.CurrentValue); // Changed from transformedResult.Value
            if (!isValid)
            {
                valResults.Add(new ValidationResult(validationAttribute.FormatErrorMessage(FieldName), [FieldName]));
            }
        }

        validationResults = valResults.Count > 0 ? valResults : null;
        _valueValidationResults.Add(valueKey, validationResults);
        return validationResults is null;
    }

    /// <summary>
    /// Updates the validation results cache for the field mapping.
    /// </summary>
    public async Task UpdateValidationResults()
    {
        _valueValidationResults.Clear();
        if (IgnoreMapping || MappingRule is null)
        {
            if (Required)
            {
                if (!_valueValidationResults.ContainsKey("<null>"))
                {
                    _valueValidationResults.Add("<null>", [new ValidationResult("A value for this field is required.", [FieldName])]);
                }
                else
                {
                    _valueValidationResults["<null>"] = [new ValidationResult("A value for this field is required.", [FieldName])];
                }
            }
            else
            {
                _valueValidationResults.Clear();
            }
            return;
        }

        // The following call to Apply() (no-args) will return an empty list based on the current fix.
        // This means validation might not occur as expected here. This part needs review.
        var results = await Apply().ConfigureAwait(false); // Changed from MappingRule.Apply()
        foreach (var result in results)
        {
            Validate(result, out _, false);
        }
    }
}
