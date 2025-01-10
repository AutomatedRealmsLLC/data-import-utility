using Microsoft.AspNetCore.Components;

namespace DataImportUtility.Components.Abstractions;

/// <summary>
/// Represents a component to be used with the in the File Import Utility Compnent Library.
/// </summary>
/// <remarks>
/// The components in the library can use an optional <see cref="IDataFileMapperState" /> to share state between components. They will be associated with the context of the <see cref="IDataFileMapperState" /> if one is provided, otherwise they will be associated with each other using a unique State identifier.
/// </remarks>
public abstract class FileImportUtilityComponentBase : ComponentBase
{
    /// <summary>
    /// The <see cref="DataFileMapperState" /> to use.
    /// </summary>
    [CascadingParameter] protected IDataFileMapperState? DataFileMapperState { get; set; }
    /// <summary>
    /// Whether or not to apply the default CSS styling to the component.
    /// </summary>
    [Parameter] public bool ApplyDefaultCss { get; set; } = true;

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
    /// The unique identifier for the component instance.  It will be put in a <code>data-instance-id</code> html attribute on all elements within the component (but not child components).
    /// </summary>
    public string InstanceId { get; } = Guid.NewGuid().ToString()[^5..];

    /// <summary>
    /// The unique identifier for the component.
    /// </summary>
    protected string? MapperStateId => DataFileMapperState?.MapperStateId;
}
