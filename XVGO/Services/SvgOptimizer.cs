using Microsoft.JSInterop;
using XVGO.Models;

namespace XVGO.Services;

internal interface ISvgOptimizer
{
    ValueTask<string> OptimizeAsync(string svgString, CancellationToken ct = default);
}

internal sealed class SvgOptimizer(IJSRuntime jsRuntime, ConversionSettings settings) : ISvgOptimizer, IAsyncDisposable
{
    private readonly Lazy<ValueTask<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/svg-optimizer.bundle.js"));
    private readonly ConversionSettings _settings = settings;

    public async ValueTask<string> OptimizeAsync(string svgString, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(svgString, nameof(svgString));
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("optimizeSvg", ct, svgString, _settings.ForceMerge);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
