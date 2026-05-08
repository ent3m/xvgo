using Microsoft.JSInterop;

namespace XVGO.Services;

internal interface ISvgOptimizer
{
    ValueTask<string> OptimizeAsync(string svgString, CancellationToken ct = default);
}

internal sealed class SvgOptimizer : ISvgOptimizer, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public SvgOptimizer(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/svg-optimizer.bundle.js").AsTask());
    }

    public async ValueTask<string> OptimizeAsync(string svgString, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(svgString, nameof(svgString));
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("optimizeSvg", ct, svgString);
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
