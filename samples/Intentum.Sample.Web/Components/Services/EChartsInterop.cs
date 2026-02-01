using Microsoft.JSInterop;

namespace Intentum.Sample.Web.Components.Services;

/// <summary>
/// JS interop for ECharts (init, setOption, resize, dispose).
/// </summary>
public sealed class EChartsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly string _elementId;
    private bool _disposed;

    public EChartsInterop(IJSRuntime js, string elementId)
    {
        _js = js;
        _elementId = elementId;
    }

    public async ValueTask InitAsync()
    {
        await _js.InvokeVoidAsync("IntentumECharts.init", _elementId);
    }

    public async ValueTask SetOptionAsync(object option, CancellationToken ct = default)
    {
        await _js.InvokeVoidAsync("IntentumECharts.setOption", _elementId, option);
    }

    public async ValueTask ResizeAsync()
    {
        await _js.InvokeVoidAsync("IntentumECharts.resize", _elementId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        try
        {
            await _js.InvokeVoidAsync("IntentumECharts.dispose", _elementId);
        }
        finally
        {
            _disposed = true;
        }
    }
}
