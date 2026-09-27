namespace PortalCopa26.Services.Charts;

/// <summary>Opções visuais opcionais para <see cref="IChartInterop.RenderBarChartAsync"/>.</summary>
public record ChartBarOptions(string? Label = null, string[]? BarColors = null, string[]? BorderColors = null, double? MinValue = null);

/// <summary>
/// Wrapper de interoperabilidade com Chart.js (D5): genérico, para ser reutilizado por
/// qualquer gráfico do portal — o Ranking FIFA é o primeiro consumidor, não o único.
/// </summary>
public interface IChartInterop
{
    ValueTask RenderBarChartAsync(string canvasId, IReadOnlyList<string> labels, IReadOnlyList<double> data, ChartBarOptions? options = null);

    ValueTask DestroyChartAsync(string canvasId);
}
