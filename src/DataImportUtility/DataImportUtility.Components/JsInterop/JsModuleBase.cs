// Adapted from: https://github.com/SteveSandersonMS/AudioBrowser/blob/main/MediaFilesAPI/Util/JSModule.cs
using Microsoft.JSInterop;

namespace AutomatedRealms.DataImportUtility.Components.JsInterop;

/// <summary>
/// Helper for loading any JavaScript (ES6) module and calling its exports
/// </summary>
public abstract class JsModuleBase(IJSRuntime jsRuntime, string moduleUrl) : IAsyncDisposable
{
    /// <summary>
    /// The JS runtime
    /// </summary>
    protected readonly IJSRuntime _jsRuntime = jsRuntime;
    private Task<IJSObjectReference>? _moduleTask;

    /// <summary>
    /// The task that loads the module
    /// </summary>
    protected Task<IJSObjectReference> ModuleTask => _moduleTask ??= _jsRuntime.InvokeAsync<IJSObjectReference>("import", moduleUrl).AsTask();

    /// <inheritdoc cref="JSObjectReferenceExtensions.InvokeVoidAsync(IJSObjectReference, string, object[])" />
    protected async ValueTask InvokeVoidAsync(string identifier, params object[]? args)
        => await (await ModuleTask).InvokeVoidAsync(identifier, args);

    /// <inheritdoc cref="JSObjectReferenceExtensions.InvokeAsync{TValue}(IJSObjectReference, string, object[])" />
    protected async ValueTask<T> InvokeAsync<T>(string identifier, params object[]? args)
        => await (await ModuleTask).InvokeAsync<T>(identifier, args);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
        => await (await ModuleTask).DisposeAsync();
}
