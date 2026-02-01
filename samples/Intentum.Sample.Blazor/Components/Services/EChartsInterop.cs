using Microsoft.JSInterop;

namespace Intentum.Sample.Blazor.Components.Services;

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

    /// <summary>
    /// Initializes ECharts on the element. Returns false if element not found or ECharts not loaded.
    /// </summary>
    public async ValueTask<bool> InitAsync()
    {
        return await _js.InvokeAsync<bool>("IntentumECharts.init", _elementId);
    }

    public async ValueTask SetOptionAsync(object option, CancellationToken ct = default)
    {
        await _js.InvokeVoidAsync("IntentumECharts.setOption", _elementId, option);
    }

    /// <summary>
    /// Sets heatmap option with cell labels formatted as percentage (e.g. 63%).
    /// </summary>
    public async ValueTask SetHeatmapOptionAsync(object option, CancellationToken ct = default)
    {
        await _js.InvokeVoidAsync("IntentumECharts.setHeatmapOption", _elementId, option);
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
        catch (JSDisconnectedException)
        {
            // Circuit kapandığında JS interop kullanılamaz; dispose atlanır.
        }
        catch (OperationCanceledException)
        {
            // İptal edilmişse sessizce geç.
        }
        finally
        {
            _disposed = true;
        }
    }
}
