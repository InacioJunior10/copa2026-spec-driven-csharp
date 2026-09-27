// Wrapper genérico de Chart.js, reutilizável por qualquer gráfico do portal
// (não específico do Ranking FIFA). Consumido via IJSRuntime pelo ChartInterop.
window.portalCopaCharts = (() => {
  const instances = new Map();

  /**
   * Renderiza (ou substitui) um gráfico de barras horizontais em um <canvas>.
   * @param {string} canvasId - id do elemento <canvas> de destino.
   * @param {string[]} labels - rótulos do eixo de categorias.
   * @param {number[]} data - valores numéricos, na mesma ordem dos labels.
   * @param {object} [options] - opções visuais opcionais:
   *   { label?: string, barColors?: string[], borderColors?: string[], minValue?: number }
   */
  function renderBarChart(canvasId, labels, data, options) {
    options = options || {};

    const existing = instances.get(canvasId);
    if (existing) {
      existing.destroy();
      instances.delete(canvasId);
    }

    const canvas = document.getElementById(canvasId);
    if (!canvas) {
      return;
    }

    const chart = new Chart(canvas.getContext('2d'), {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: options.label || '',
          data: data,
          backgroundColor: options.barColors || 'rgba(74,136,220,0.75)',
          borderColor: options.borderColors || '#4a88dc',
          borderWidth: 1.5,
          borderRadius: 5,
          borderSkipped: false,
        }],
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
        },
        scales: {
          x: {
            min: options.minValue,
            grid: { color: 'rgba(46,68,112,0.5)' },
            ticks: { color: '#9ab5d8', font: { size: 11 } },
          },
          y: {
            grid: { display: false },
            ticks: { color: '#f0f6ff', font: { size: 12 } },
          },
        },
      },
    });

    instances.set(canvasId, chart);
  }

  function destroyChart(canvasId) {
    const existing = instances.get(canvasId);
    if (existing) {
      existing.destroy();
      instances.delete(canvasId);
    }
  }

  return { renderBarChart, destroyChart };
})();
