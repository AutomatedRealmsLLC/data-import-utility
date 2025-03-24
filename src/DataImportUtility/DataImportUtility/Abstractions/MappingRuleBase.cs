using System.Collections.Immutable;
using System.Data;
using System.Text.Json.Serialization;

using DataImportUtility.CustomConverters;
using DataImportUtility.Helpers;
using DataImportUtility.Models;

namespace DataImportUtility.Abstractions;

/// <summary>
/// The base class for all rules.
/// </summary>
/// <remarks>
/// Base items can be added using another partial class. The concrete class should follow the naming convention of
/// &lt;Name&gt;Rule.
/// </remarks>
[JsonConverter(typeof(MappingRuleBaseConverter))]
public abstract partial class MappingRuleBase : IDisposable
{
    #region Properties
    /// <summary>
    /// The event that is raised when the definition of the rule changes.
    /// </summary>
    public event Func<Task>? OnDefinitionChanged;

    /// <summary>
    /// The value the generated enum member display in the <see cref="MappingRuleType" />.
    /// </summary>
    /// <remarks>
    /// The values for the enum members will be determined by sorting first by the 
    /// <see cref="EnumMemberOrder" />, then by the <see cref="EnumMemberName" />. This means that 
    /// any resulting duplicate orders will result in values that do not match the order value
    /// </remarks>
    [JsonIgnore]
    public virtual int EnumMemberOrder { get; }

    /// <summary>
    /// The member name to use for the <see cref="MappingRuleType"/>.
    /// </summary>
    /// <remarks>
    /// This should follow the rules used for Enum Member Names.
    /// </remarks>
    public abstract string EnumMemberName { get; }

    /// <summary>
    /// The name of the rule.
    /// </summary>
    [JsonIgnore]
    public abstract string DisplayName { get; }

    /// <summary>
    /// The short name to display for the rule.
    /// </summary>
    [JsonIgnore]
    public virtual string ShortName { get; } = string.Empty;

    /// <summary>
    /// Gets the description of the rule.
    /// </summary>
    [JsonIgnore]
    public abstract string Description { get; }

    /// <summary>
    /// The additional information for the rule.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RuleDetail
    {
        get => _ruleDetail;
        set
        {
            _ruleDetail = value;
            OnDefinitionChanged?.Invoke();
        }
    }
    private string? _ruleDetail;

    /// <summary>
    /// An indicator of whether the rule is empty.
    /// </summary>
    [JsonIgnore]
    public abstract bool IsEmpty { get; }

    /// <summary>
    /// The collection of fields and their transformations used in the composite output value.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonInclude]
    public ImmutableList<FieldTransformation> SourceFieldTranformations
    {
        get => _sourceFields;
        internal set
        {
            _sourceFields = value;
            OnDefinitionChanged?.Invoke();
        }
    }
    private ImmutableList<FieldTransformation> _sourceFields = [];

    /// <summary>
    /// The source fields that are in use. This is a subset of the <see cref="SourceFieldTranformations"/> that have a
    /// field name.
    /// </summary>
    [JsonIgnore]
    protected IEnumerable<FieldTransformation> SourceFieldsInUse => SourceFieldTranformations
        .Where(x => x.Field is { FieldName: not null });

    /// <summary>
    /// The maximum number of source fields that can be used in the rule.
    /// </summary>
    protected virtual ushort MaxSourceFields { get; } = ushort.MaxValue;
    #endregion Properties

    #region Apply Methods
    /// <summary>
    /// Applies the mapping rule to each transformed value contained in the <see cref="SourceFieldTranformations"/>.<see cref="FieldTransformation.Field"/>'s <see cref="ImportedRecordFieldDescriptor.ForTable"/> from the .
    /// </summary>
    public abstract Task<IEnumerable<TransformationResult>> Apply();

    /// <summary>
    /// Applies the mapping rule to the <see cref="TransformationResult" />.
    /// </summary>
    /// <param name="result">
    /// The result of the transformations applied (in order) to each of the fields involved in the mapping.
    /// </param>
    /// <returns>
    /// The result of applying the mapping rule to the values.
    /// </returns>
    /// <remarks>
    /// The provided <paramref name="result"/> should be the result of applying all of the 
    /// <see cref="SourceFieldTranformations" /> and consolidating their results into a single 
    /// <see cref="TransformationResult" />. The <see cref="TransformationResult.Value" /> should be a JSON
    /// serialized string of the values of the fields used in the mapping. Note that some rules may not
    /// be valid on collections of values. Use the 
    /// <see cref="TranformationResultHelpers.ErrorIfCollection(TransformationResult, string)" />
    /// for mapping rules that are not valid on collections when implementing this method.
    /// </remarks>
    public abstract Task<TransformationResult> Apply(TransformationResult result);

    /// <summary>
    /// Applies the mapping rule to the collection of <see cref="TransformationResult" />.
    /// </summary>
    /// <param name="results">
    /// The collection of input results with the transformations applied.
    /// </param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TransformationResult>> Apply(IEnumerable<TransformationResult> results)
    {
        var rowResults = new List<TransformationResult>();
        foreach (var result in results)
        {
            rowResults.Add(await Apply(result));
        }

        return rowResults;
    }

    /// <summary>
    /// Gets the transformed values using the <see cref="SourceFieldTranformations"/> in use on the imported records.
    /// </summary>
    /// <param name="importedRecords">
    /// The imported records to get the transformed values from.
    /// </param>
    /// <returns>
    /// The transformed values using the <see cref="SourceFieldTranformations" /> in use on the imported records.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the field transformations contain fields that are not in the imported records.
    /// </exception>
    public virtual async Task<IEnumerable<TransformationResult?>> Apply(DataTable importedRecords)
    {
        var useFieldTransforms = SourceFieldsInUse.ToList();

        if (useFieldTransforms.Count == 0)
        {
            return [];
        }

        // Make sure that all of the fields in the field transformations are in the imported
        // records columns
        var unmatchedFields = useFieldTransforms
            .Where(x => !importedRecords.Columns
                .OfType<DataColumn>()
                .Any(y => y.ColumnName == x.Field!.FieldName))
            .ToList();
        if (unmatchedFields.Count > 0)
        {
            throw new ArgumentException($"The field transformations contain fields that are not in the imported records. Fields are: {string.Join(", ", unmatchedFields.Select(x => x.Field!.FieldName))}");
        }

        var transformResults = new Dictionary<string, IEnumerable<TransformationResult>>();

        // We are making the potentially dangerous assumption that the values in the imported
        // records are in the same order as the field transformations
        var expectedValueCount = importedRecords.Rows.Count;

        var returnResults = new List<TransformationResult?>();
        foreach (var curRow in importedRecords.Rows.OfType<DataRow>())
        {
            returnResults.Add(await Apply(curRow));
        }

        foreach (var fieldTransform in useFieldTransforms)
        {
            var curResults = await fieldTransform.Apply(importedRecords) ?? [];
            var resultCount = curResults.Count();
            if (resultCount >= 0)
            {
                if (resultCount!= expectedValueCount)
                {
                    throw new ArgumentException($"The number of values in the imported records does not match the number of values in the field transformations. The number of values in the imported records is {importedRecords.Rows.Count} and the number of values in the field transformations is {resultCount}.");
                }
                transformResults.Add(fieldTransform.Field!.FieldName, curResults);
            }
        }

        if (transformResults.Count == 0)
        {
            return [];
        }

        return CombineResultsByField(transformResults);
    }

    /// <returns>
    /// The transformed values using the <see cref="SourceFieldTranformations"/> that are in use on the imported record
    /// data.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the field transformations contain fields that are not in the imported records.
    /// </exception>
    public virtual async Task<TransformationResult?> Apply(DataRow importedRecord)
    {
        var useFieldTransforms = SourceFieldsInUse.ToList();

        if (useFieldTransforms.Count == 0)
        {
            return new();
        }

        // Make sure that all of the fields in the field transformations are in the imported
        // records columns
        var unmatchedFields = useFieldTransforms
            .Where(x => !importedRecord.Table.Columns
                .OfType<DataColumn>()
                .Any(y => y.ColumnName == x.Field!.FieldName))
            .ToList();

        if (unmatchedFields.Count > 0)
        {
            throw new ArgumentException($"The field transformations contain fields that are not in the imported records. Fields are: {string.Join(", ", unmatchedFields.Select(x => x.Field!.FieldName))}");
        }

        var transformResults = new Dictionary<string, TransformationResult>();

        // We are making the potentially dangerous assumption that the values in the imported
        // records are in the same order as the field transformations
        foreach (var fieldTransform in useFieldTransforms)
        {
            var curResult = await fieldTransform.Apply(importedRecord);
            if (!curResult.WasFailure)
            {
                transformResults.Add(fieldTransform.Field!.FieldName, curResult);
            }
        }

        if (transformResults.Count == 0)
        {
            return new();
        }

        return CombineResultsByField(transformResults);
    }

    /// <summary>
    /// Applies the mapping rule to the values organized by imported field name.
    /// </summary>
    /// <param name="transformResults">
    /// The set of values as <see cref="TransformationResult"/>s for each field used in the mapping.
    /// </param>
    /// <returns>
    /// The result of applying the mapping rule to the values.
    /// </returns>
    public virtual TransformationResult? CombineResultsByField(Dictionary<string, TransformationResult> transformResults)
    {
        var transformedValues = transformResults.Keys.Select(key => transformResults[key].Value);
        var stringConcatResult = string.Join("", transformedValues);

        return new()
        {
            OriginalValue = System.Text.Json.JsonSerializer.Serialize(transformedValues),
            Value = stringConcatResult
        };
    }

    /// <summary>
    /// Applies the mapping rule to the values organized by imported field name.
    /// </summary>
    /// <param name="transformResults">
    /// The set of values as <see cref="TransformationResult"/>s for each field used in the mapping.
    /// </param>
    /// <returns>
    /// The result of applying the mapping rule to the values.
    /// </returns>
    public virtual IEnumerable<TransformationResult> CombineResultsByField(Dictionary<string, IEnumerable<TransformationResult>> transformResults)
    {
        var rowResults = new List<TransformationResult>();
        var recordCount = transformResults.First().Value.Count();
        for (int i = 0; i < recordCount; i++)
        {
            var transformedValues = transformResults.Keys.Select(key => transformResults[key].Skip(i).First().Value);
            var curStringConcatResult = string.Join("", transformedValues);

            rowResults.Add(
                new()
                {
                    OriginalValue = System.Text.Json.JsonSerializer.Serialize(transformedValues),
                    Value = curStringConcatResult
                });
        }

        return rowResults;
    }
    #endregion Apply Methods

    #region SourceFields Methods
    /// <summary>
    /// Adds a new <see cref="FieldTransformation"/> to the <see cref="SourceFieldTranformations"/> 
    /// collection for the specified <see cref="ImportedRecordFieldDescriptor"/>.
    /// </summary>
    /// <param name="fieldDescriptor">
    /// The <see cref="ImportedRecordFieldDescriptor"/> to add a <see cref="FieldTransformation"/> for.
    /// </param>
    /// <returns>The added <see cref="FieldTransformation"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the maximum number of source fields for the rule has been reached.
    /// </exception>
    public virtual FieldTransformation AddFieldTransformation(ImportedRecordFieldDescriptor? fieldDescriptor = null)
        => AddFieldTransformation(new FieldTransformation(fieldDescriptor));

    /// <summary>
    /// Adds a new <see cref="FieldTransformation"/> to the <see cref="SourceFieldTranformations"/> collection.
    /// </summary>
    /// <param name="fieldTransformation">The <see cref="FieldTransformation"/> to add.</param>
    /// <returns>The added <see cref="FieldTransformation"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the maximum number of source fields for the rule has been reached.
    /// </exception>
    public virtual FieldTransformation AddFieldTransformation(FieldTransformation fieldTransformation)
    {
        if (SourceFieldTranformations.Count >= MaxSourceFields)
        {
            throw new InvalidOperationException($"The maximum number of source fields for the rule is {MaxSourceFields}.");
        }

        var sourceFields = SourceFieldTranformations.ToList();
        sourceFields.Add(fieldTransformation);
        SourceFieldTranformations = [.. sourceFields];
        return fieldTransformation;
    }

    /// <summary>
    /// Replaces the provided <see cref="FieldTransformation"/> with the new 
    /// <see cref="FieldTransformation"/>.
    /// </summary>
    public virtual void ReplaceFieldTransformation(FieldTransformation oldFieldTransformation, FieldTransformation newFieldTransformation)
    {
        var sourceFields = SourceFieldTranformations.ToList();
        var index = sourceFields.IndexOf(oldFieldTransformation);
        sourceFields[index] = newFieldTransformation;
        SourceFieldTranformations = [.. sourceFields];
    }

    /// <summary>
    /// Replace the <see cref="FieldTransformation"/> at the specified index with the provided 
    /// <see cref="FieldTransformation"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the index is less than 0 or greater than the number of elements in the collection.
    /// </exception>
    public virtual void ReplaceFieldTransformation(uint index, FieldTransformation fieldTransformation)
    {
        if (index == 0 || index >= SourceFieldTranformations.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "The index must be greater than or equal to 0 and less than the number of elements in the collection.");
        }

        if (index > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "The index must be less than or equal to int.MaxValue.");
        }

        var sourceFields = SourceFieldTranformations.ToList();
        sourceFields[(int)index] = fieldTransformation;
        SourceFieldTranformations = [.. sourceFields];
    }

    /// <summary>
    /// Removes the <see cref="FieldTransformation"/> from the <see cref="SourceFieldTranformations"/> collection.
    /// </summary>
    /// <param name="fieldTransformation">The <see cref="FieldTransformation"/> to remove.</param>
    public virtual void RemoveFieldTransformation(FieldTransformation fieldTransformation)
    {
        var sourceFields = SourceFieldTranformations.ToList();
        sourceFields.Remove(fieldTransformation);
        SourceFieldTranformations = [.. sourceFields];
    }

    /// <summary>
    /// Removes the <see cref="FieldTransformation"/> from the <see cref="SourceFieldTranformations"/> collection.
    /// </summary>
    /// <param name="index">The index of the <see cref="FieldTransformation"/> to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the index is less than 0 or greater than the number of elements in the collection.
    /// </exception>
    public virtual void RemoveFieldTransformation(int index)
    {
        if (index < 0 || index >= SourceFieldTranformations.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "The index must be greater than or equal to 0 and less than the number of elements in the collection.");
        }

        var sourceFields = SourceFieldTranformations.ToList();
        sourceFields.RemoveAt(index);
        SourceFieldTranformations = [.. sourceFields];
    }

    /// <summary>
    /// Removes all <see cref="FieldTransformation"/> from the <see cref="SourceFieldTranformations"/> collection.
    /// </summary>
    public virtual void ClearFieldTransformations()
        => SourceFieldTranformations = [];
    #endregion SourceFields Methods

    #region Clone Methods
    /// <summary>
    /// Clones the rule.
    /// </summary>
    public virtual MappingRuleBase Clone()
    {
        var forRet = (MappingRuleBase)MemberwiseClone();
        forRet.OnDefinitionChanged = null;
        forRet.SourceFieldTranformations = SourceFieldTranformations.Select(x => x.Clone()).ToImmutableList();
        return forRet;
    }

    /// <summary>
    /// Clones the rule.
    /// </summary>
    /// <typeparam name="TRuleType">The type of the rule to clone.</typeparam>
    /// <returns>A new rule that is a deep clone of the original.</returns>
    public virtual TRuleType Clone<TRuleType>() where TRuleType : MappingRuleBase
        => (TRuleType)Clone();
    #endregion Clone Methods

    /// <inheritdoc />
    public virtual void Dispose()
    {
        OnDefinitionChanged = null;
    }
}