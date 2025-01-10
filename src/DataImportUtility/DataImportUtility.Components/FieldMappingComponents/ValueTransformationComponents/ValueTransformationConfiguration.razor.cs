using Microsoft.AspNetCore.Components;

using DataImportUtility.Abstractions;
using DataImportUtility.Components.Abstractions;
using DataImportUtility.Models;

namespace DataImportUtility.Components.FieldMappingComponents.ValueTransformationComponents;

/// <summary>
/// Displays the configuration <see cref="ValueTransformationBase" /> to transform a value using.
/// </summary>
public partial class ValueTransformationConfiguration : FileImportUtilityComponentBase
{
    private readonly List<ValueTransformationBase> _createdTransformations = [];

    /// <summary>
    /// The output target to display the configuration for.
    /// </summary>
    [Parameter, EditorRequired, AllowNull] public FieldTransformation FieldTransformation { get; set; }
    /// <summary>
    /// The callback for when the output target is changed.
    /// </summary>
    [Parameter] public EventCallback<FieldTransformation> FieldTransformationChanged { get; set; }
    /// <summary>
    /// The index of the transformation rule.
    /// </summary>
    [Parameter, EditorRequired] public int TransformationRuleIndex { get; set; }
    /// <summary>
    /// The tranformation result coming in from previous transformation operations.
    /// </summary>
    [Parameter, EditorRequired, AllowNull] public TransformationResult CurrentTransformationResult { get; set; }
    /// <summary>
    /// The callback for when a new transformation result is available.
    /// </summary>
    [Parameter] public EventCallback<TransformationResult> NewTransformationResultAvailable { get; set; }

    private ValueTransformationType _selectedTransformationType = ValueTransformationType.SubstringOperation;

    [AllowNull]
    private ValueTransformationBase _selectedTransformation;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _selectedTransformation = FieldTransformation.ValueTransformations[TransformationRuleIndex];
        _selectedTransformationType = _selectedTransformation.GetEnumValue();
    }

    private Task HandleTransformationChanged()
    {
        // check for an existing instance of the selected transformation type
        var transformOp = _createdTransformations.FirstOrDefault(x => x.GetType() == _selectedTransformationType.GetClassType());

        // Create a new instance of the selected transformation type and grab the current detail from the existing operation
        transformOp ??= _selectedTransformationType.CreateNewInstance()!;

        FieldTransformation.ReplaceTransformation(_selectedTransformation, transformOp);

        _selectedTransformation = transformOp;

        return NotifyTransformationChanged();
    }

    private Task NotifyTransformationChanged()
        => FieldTransformationChanged.InvokeAsync(FieldTransformation);
}
