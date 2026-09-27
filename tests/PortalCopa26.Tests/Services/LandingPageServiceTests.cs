using PortalCopa26.Data.Seed;
using PortalCopa26.Domain.Enums;
using PortalCopa26.Services;
using PortalCopa26.Tests.Data;
using Xunit;

namespace PortalCopa26.Tests.Services;

public class LandingPageServiceTests : IDisposable
{
    private readonly SqliteInMemoryFixture _fixture = new();
    private readonly LandingPageService _service;

    public LandingPageServiceTests()
    {
        _service = new LandingPageService(_fixture);
    }

    public void Dispose() => _fixture.Dispose();

    private async Task SeedAsync()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);
    }

    [Fact]
    public async Task GetEstatisticasAsync_Retorna_Contagens_Esperadas()
    {
        await SeedAsync();

        var estatisticas = await _service.GetEstatisticasAsync();

        Assert.Equal(48, estatisticas.Selecoes);
        Assert.Equal(12, estatisticas.Grupos);
        Assert.Equal(104, estatisticas.Jogos);
        Assert.Equal(16, estatisticas.Estadios);
    }

    [Fact]
    public async Task GetProximosJogosAsync_Retorna_Apenas_Jogos_Sem_Placar_Dos_2_Dias_Mais_Proximos()
    {
        await SeedAsync();

        var dias = await _service.GetProximosJogosAsync();

        Assert.Equal(2, dias.Count);
        Assert.True(dias[0].Data < dias[1].Data);

        // Todos os jogos da fase de grupos foram semeados sem placar oficial,
        // então os 2 primeiros dias com jogo devem ser os 2 primeiros dias do torneio.
        Assert.Equal(new DateOnly(2026, 6, 11), dias[0].Data);

        var todosOsJogos = dias.SelectMany(d => d.Jogos).ToList();
        Assert.All(todosOsJogos, j => Assert.True(j.MandanteCodigo is not null || j.VagaMandante is not null));
    }

    [Fact]
    public async Task GetProximosJogosAsync_Retorna_Vazio_Quando_Todos_Os_Jogos_Tem_Placar_Oficial()
    {
        await SeedAsync();

        using (var context = _fixture.CreateContext())
        {
            foreach (var jogo in context.Jogos)
            {
                jogo.GolsMandante = 0;
                jogo.GolsVisitante = 0;
            }
            await context.SaveChangesAsync();
        }

        var dias = await _service.GetProximosJogosAsync();

        Assert.Empty(dias);
    }

    [Fact]
    public async Task GetTopRankingAsync_Retorna_As_10_Melhores_Posicoes_Em_Ordem_Crescente()
    {
        await SeedAsync();

        var top10 = await _service.GetTopRankingAsync();

        Assert.Equal(10, top10.Count);
        for (var i = 1; i < top10.Count; i++)
        {
            Assert.True(top10[i].Posicao > top10[i - 1].Posicao);
        }
        Assert.Equal(1, top10[0].Posicao);
    }
}
