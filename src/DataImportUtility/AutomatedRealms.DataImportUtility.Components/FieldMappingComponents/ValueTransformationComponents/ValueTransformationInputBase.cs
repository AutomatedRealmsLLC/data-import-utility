using AutomatedRealms.DataImportUtility.Abstractions;

using Microsoft.AspNetCore.Components;

namespace AutomatedRealms.DataImportUtility.Components.FieldMappingComponents.ValueTransformationComponents;

/// <summary>
/// The base class for a value transformation input.
/// </summary>
public abstract class ValueTransformationInputBase<T> : ComponentBase
    where T : ValueTransformationBase
{
    /// <summary>
    /// The transformation to display the configuration for.
    /// </summary>
    [Parameter, EditorRequired] public T ValueTransformation { get; set; } = default!;
    /// <summary>
    /// The ID to use for the input element.
    /// </summary>
    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// Whether or not to apply the default CSS styling to the component.
    /// </summary>
    [Parameter] public bool ApplyDefaultCss { get; set; } = true;
    /// <summary>
    /// The callback for when the input is changed.
    /// </summary>
    [Parameter] public EventCallback OnAfterInput { get; set; }
    /// <summary>
    /// The index of the active preview row.
    /// </summary>
    [Parameter] public uint PreviewRowIndex { get; set; }

    /// <summary>
    /// The string to use for the default CSS class.
    /// </summary>
    /// <remarks>
    /// This should be applied to all top-level elements in a given component, as well as special elements that get global styles, such as:<br />
    /// <list type="bullet">
    ///     <listitem>Buttons</listitem>
    ///     <listitem>Inputs (text, checkmark, file, and number types specifically)</listitem>
    ///     <listitem>Select</listitem>
    ///     <listitem>Tables (the children tr, th, td, etc will only ever by styled inside of a table.o1l-library-style selector</listitem>
    ///     <listitem>Unordered/Ordered Lists</listitem>
    ///     <listitem>Divs with the <code>error-diplay</code> class.</listitem>
    ///     <listitem>Divs with the <code>o1l-modal</code> class (child elements will only be styled inside of a .o1l-modal.o1l-library-style selector).</listitem>
    /// </list>
    /// </remarks>
    protected string? DefaultCssClass => ApplyDefaultCss ? " o1l-library-style" : null;

    /// <summary>
    /// Handles the operation detail changed event.
    /// </summary>
    protected virtual Task HandleOperationDetailChanged() => OnAfterInput.InvokeAsync();
}