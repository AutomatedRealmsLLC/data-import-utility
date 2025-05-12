using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json.Serialization;

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
    public string FieldName { get; init; } = string.Empty;

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
    public ImmutableArray<ValidationAttribute> ValidationAttributes { get; set; } = ImmutableArray<ValidationAttribute>.Empty;

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
    public MappingRuleBase? MappingRule { get; set; }

    /// <summary>
    /// The type of the mapping rule.
    /// </summary>
    public MappingRuleType MappingRuleType => MappingRule?.GetEnumValue() ?? MappingRuleType.IgnoreRule;

    /// <summary>
    /// Whether the field is required to be mapped.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Whether to ignore the mapping.
    /// </summary>
    public bool IgnoreMapping => MappingRuleType == MappingRuleType.IgnoreRule
        || (MappingRule?.IsEmpty ?? true);

    /// <summary>
    /// Applies the mapping rule to any available data in the child <see cref="MappingRule" />.
    /// </summary>
    /// <returns>The results of the transformation.</returns>
    public Task<IEnumerable<TransformationResult?>> Apply()
        => MappingRule?.Apply() ?? Task.FromResult(Enumerable.Empty<TransformationResult?>());

    /// <summary>
    /// Applies the mapping rule to the provided data.
    /// </summary>
    /// <param name="data">The data to apply the mapping rule to.</param>
    /// <returns>The results of the transformation.</returns>
    public Task<IEnumerable<TransformationResult?>> Apply(DataTable data)
        => MappingRule?.Apply(data) ?? Task.FromResult(Enumerable.Empty<TransformationResult?>());

    /// <summary>
    /// Applies the mapping rule to the provided data row.
    /// </summary>
    /// <param name="dataRow">The data row to apply the mapping rule to.</param>
    /// <returns>The results of the transformation.</returns>
    public Task<TransformationResult?> Apply(DataRow dataRow)
        => MappingRule?.Apply(dataRow) ?? Task.FromResult((TransformationResult?)null);

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
    internal bool Validate(TransformationResult? transformedResult, out List<ValidationResult>? validationResults, bool useCache = true)
#else
    internal bool Validate(TransformationResult? transformedResult, out List<ValidationResult>? validationResults, bool useCache = true)
#endif
    {
        if (transformedResult?.CurrentValue is null)
        {
            validationResults = Required ? [new ValidationResult("The field is required.", new List<string> { FieldName })] : null;
            if (validationResults != null && validationResults.Count > 0)
            {
                if (!_valueValidationResults.ContainsKey("<null>"))
                {
                    _valueValidationResults.Add("<null>", validationResults);
                }
            }
            return validationResults is null;
        }

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
            var isValid = validationAttribute.IsValid(transformedResult.CurrentValue);
            if (!isValid)
            {
                valResults.Add(new ValidationResult(validationAttribute.FormatErrorMessage(FieldName), new List<string> { FieldName }));
            }
        }

        validationResults = valResults.Count > 0 ? valResults : null;
        if (!_valueValidationResults.ContainsKey(valueKey))
        {
            _valueValidationResults.Add(valueKey, validationResults);
        }
        else
        {
            _valueValidationResults[valueKey] = validationResults;
        }
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
                    _valueValidationResults.Add("<null>", [new ValidationResult("A value for this field is required.", new List<string> { FieldName })]);
                }
            }
            else
            {
                _valueValidationResults.Clear();
            }
            return;
        }

        var results = await MappingRule.Apply();
        foreach (var result in results)
        {
            Validate(result, out _, false);
        }
    }
}
