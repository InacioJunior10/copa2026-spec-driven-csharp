using Microsoft.JSInterop;

namespace PortalCopa26.Services.Charts;

public class ChartInterop(IJSRuntime jsRuntime) : IChartInterop
{
    public ValueTask RenderBarChartAsync(string canvasId, IReadOnlyList<string> labels, IReadOnlyList<double> data, ChartBarOptions? options = null)
    {
        return jsRuntime.InvokeVoidAsync("portalCopaCharts.renderBarChart", canvasId, labels, data, options);
    }

    public ValueTask DestroyChartAsync(string canvasId)
    {
        return jsRuntime.InvokeVoidAsync("portalCopaCharts.destroyChart", canvasId);
    }
}
