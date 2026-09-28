using PortalCopa26.Data.Seed;
using PortalCopa26.Services;
using PortalCopa26.Tests.Data;
using Xunit;

namespace PortalCopa26.Tests.Services;

public class SelecaoServiceTests : IDisposable
{
    private readonly SqliteInMemoryFixture _fixture = new();
    private readonly SelecaoService _service;

    public SelecaoServiceTests()
    {
        _service = new SelecaoService(_fixture);
    }

    public void Dispose() => _fixture.Dispose();

    private async Task SeedAsync()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);
    }

    [Fact]
    public async Task GetEquipesAsync_Retorna_48_Selecoes_Com_Grupo_Tecnico_E_Elenco()
    {
        await SeedAsync();

        var equipes = await _service.GetEquipesAsync();

        Assert.Equal(48, equipes.Count);
        Assert.All(equipes, e => Assert.False(string.IsNullOrWhiteSpace(e.Tecnico)));
        Assert.All(equipes, e => Assert.NotEmpty(e.Jogadores));
    }

    [Fact]
    public async Task GetEquipesAsync_Retorna_Elenco_Esperado_Do_Brasil()
    {
        await SeedAsync();

        var equipes = await _service.GetEquipesAsync();
        var brasil = equipes.Single(e => e.Codigo == "BRA");

        Assert.Equal("Brasil", brasil.Nome);
        Assert.Equal('C', brasil.Grupo);
        Assert.Equal("Carlo Ancelotti", brasil.Tecnico);
        Assert.Equal(23, brasil.Jogadores.Count);
        Assert.Contains(brasil.Jogadores, j => j.Nome == "Alisson" && j.Idade == 33 && j.Gols == 0 && j.ParticipacoesCopas == null);
    }

    [Fact]
    public async Task GetEquipesAsync_Selecao_Ranqueada_Retorna_RankingPosicao_Preenchido()
    {
        await SeedAsync();

        var equipes = await _service.GetEquipesAsync();
        var brasil = equipes.Single(e => e.Codigo == "BRA");

        Assert.Equal(6, brasil.RankingPosicao);
    }

    [Fact]
    public async Task GetEquipesAsync_Selecao_Sem_Ranking_Fifa_Retorna_RankingPosicao_Nulo()
    {
        await SeedAsync();

        var equipes = await _service.GetEquipesAsync();
        var novaZelandia = equipes.Single(e => e.Codigo == "NZL");

        Assert.Null(novaZelandia.RankingPosicao);
    }
}
