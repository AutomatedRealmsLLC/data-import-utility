using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DataImportUtility.Components.JsInterop;

/// <summary>
/// The JavaScript module for the file mapper.
/// </summary>
/// <param name="jsRuntime">The JS runtime this instance uses.</param>
public class FileMapperJsModule(IJSRuntime jsRuntime) : JsModuleBase(jsRuntime, "./_content/RMITSFileImportUtility.Components/js/file-mapper-funcs.js")
{
    /// <summary>
    /// Scrolls the element into view.
    /// </summary>
    /// <param name="elementIdOrSelector">The element selector.</param>
    /// <returns></returns>
    public ValueTask ScrollElementIntoView(string elementIdOrSelector)
        => InvokeVoidAsync("scrollElementIntoView", elementIdOrSelector);

    /// <summary>
    /// Scrolls the element into view.
    /// </summary>
    /// <param name="elemRef">The element reference.</param>
    public ValueTask ScrollElementIntoView(ElementReference elemRef)
        => InvokeVoidAsync("scrollElementIntoView", elemRef);

    /// <summary>
    /// Synchronizes the scrolling of two elements.
    /// </summary>
    /// <param name="sourceElementIdOrSelector">The element selector to synchronize with.</param>
    /// <param name="targetElementIdOrSelector">The element selector of the element to scroll when the <paramref name="sourceElementIdOrSelector"/> is scrolled.</param>
    /// <param name="syncVertical">Whether to synchronize the vertical scrolling.</param>
    /// <param name="syncHorizontal">Whether to synchronize the horizontal scrolling.</param>
    public ValueTask SynchronizeElementScrolling(string sourceElementIdOrSelector, string targetElementIdOrSelector, bool syncVertical = true, bool syncHorizontal = false)
        => InvokeVoidAsync("synchronizeElementScrolling", sourceElementIdOrSelector, targetElementIdOrSelector, syncVertical, syncHorizontal);

    /// <summary>
    /// Synchronizes the scrolling of two elements.
    /// </summary>
    /// <param name="sourceElemRef">The element reference to synchronize with.</param>
    /// <param name="targetElemRef">The element reference of the element to scroll when the <paramref name="sourceElemRef"/> is scrolled.</param>
    /// <param name="syncVertical">Whether to synchronize the vertical scrolling.</param>
    /// <param name="syncHorizontal">Whether to synchronize the horizontal scrolling.</param>
    public ValueTask SynchronizeElementScrolling(ElementReference sourceElemRef, ElementReference targetElemRef, bool syncVertical = true, bool syncHorizontal = false)
        => InvokeVoidAsync("synchronizeElementScrolling", sourceElemRef, targetElemRef, syncVertical, syncHorizontal);

    /// <summary>
    /// Removes the synchronization of the scrolling.
    /// </summary>
    /// <paramref name="sourceElementIdOrSelector">The element selector of the element to remove the scroll synchronization from.</paramref>
    public ValueTask RemoveScrollSynchronization(string sourceElementIdOrSelector)
        => InvokeVoidAsync("removeScrollSynchronization", sourceElementIdOrSelector);

    /// <summary>
    /// Removes the synchronization of the scrolling. 
    /// </summary>
    /// <paramref name="sourceElemRef">The element reference of the element to remove the scroll synchronization from.</paramref>
    public ValueTask RemoveScrollSynchronization(ElementReference sourceElemRef)
        => InvokeVoidAsync("removeScrollSynchronization", sourceElemRef);

    /// <summary>
    /// Synchronizes the scrolling of two elements.
    /// </summary>
    /// <param name="sourceTableIdOrSelector">
    /// The element selector of the element to forward mouse events from.
    /// </param>
    /// <param name="targetTableIdOrSelector">
    /// The element selector of the element to forward mouse events to.
    /// </param>
    public ValueTask SynchronizeTableRowHover(string sourceTableIdOrSelector, string targetTableIdOrSelector)
        => InvokeVoidAsync("synchronizeTableRowHover", sourceTableIdOrSelector, targetTableIdOrSelector);

    /// <summary>
    /// Synchronizes the scrolling of two elements.
    /// </summary>
    /// <param name="sourceTableElemRef">
    /// The element reference of the element to forward mouse events from.
    /// </param> 
    /// <param name="targetTableElemRef">
    /// The element reference of the element to forward mouse events to.
    /// </param>
    public ValueTask SynchronizeTableRowHover(ElementReference sourceTableElemRef, ElementReference targetTableElemRef)
        => InvokeVoidAsync("synchronizeTableRowHover", sourceTableElemRef, targetTableElemRef);

    /// <summary>
    /// Removes the synchronization of the scrolling.
    /// </summary>
    /// <paramref name="sourceTableIdOrSelector">
    /// The element selector of the element to remove the scroll synchronization from.
    /// </paramref>
    public ValueTask RemoveTableRowHoverSynchronization(string sourceTableIdOrSelector)
        => InvokeVoidAsync("removeTableRowHoverSynchronization", sourceTableIdOrSelector);

    /// <summary>
    /// Removes the synchronization of the scrolling.
    /// </summary>
    /// <paramref name="sourceTableElemRef">
    /// The element selector of the element to remove the scroll synchronization from.
    /// </paramref>
    public ValueTask RemoveTableRowHoverSynchronization(ElementReference sourceTableElemRef)
        => InvokeVoidAsync("removeTableRowHoverSynchronization", sourceTableElemRef);

    /// <summary>
    /// Synchronizes the scrolling of two elements.
    /// </summary>
    /// <param name="sourceElementIdOrSelector">The element selector to synchronize with.</param>
    /// <param name="targetElementIdOrSelector">The element selector of the element to scroll when the <paramref name="sourceElementIdOrSelector"/> is scrolled.</param>
    public ValueTask SynchronizeElementMouseEvents(string sourceElementIdOrSelector, string targetElementIdOrSelector)
        => InvokeVoidAsync("synchronizeElementMouseEvents", sourceElementIdOrSelector, targetElementIdOrSelector);

    /// <summary>
    /// Synchronizes the scrolling of two elements.
    /// </summary>
    /// <param name="targetElemRef">The element reference of the element to scroll when the <paramref name="sourceElemRef"/> is scrolled.</param>
    /// <param name="sourceElemRef">The element reference to synchronize with.</param>
    public ValueTask SynchronizeElementMouseEvents(ElementReference sourceElemRef, ElementReference targetElemRef)
        => InvokeVoidAsync("synchronizeElementMouseEvents", sourceElemRef, targetElemRef);

    /// <summary>
    /// Removes the synchronization of the scrolling.
    /// </summary>
    /// <paramref name="sourceElementIdOrSelector">The element selector of the element to remove the scroll synchronization from.</paramref>
    public ValueTask RemoveScrollMouseEventsSynchronization(string sourceElementIdOrSelector)
        => InvokeVoidAsync("removeScrollMouseEventsSynchronization", sourceElementIdOrSelector);

    /// <summary>
    /// Removes the synchronization of the scrolling. 
    /// </summary>
    /// <paramref name="sourceElemRef">The element reference of the element to remove the scroll synchronization from.</paramref>
    public ValueTask RemoveScrollMouseEventsSynchronization(ElementReference sourceElemRef)
        => InvokeVoidAsync("removeScrollMouseEventsSynchronization", sourceElemRef);

    /// <summary>
    /// Gets whether an element exists in the DOM.
    /// </summary>
    /// <param name="elementIdOrSelector">The element selector.</param>
    /// <returns>Whether the element exists.</returns>
    public ValueTask<bool> ElementExists(string elementIdOrSelector)
        => InvokeAsync<bool>("elementExists", elementIdOrSelector);

    /// <summary>
    /// Gets whether an element exists in the DOM.
    /// </summary>
    /// <param name="elemRef">The element reference.</param>
    /// <returns>Whether the element exists.</returns>
    public ValueTask<bool> ElementExists(ElementReference elemRef)
        => InvokeAsync<bool>("elementExists", elemRef);
}
