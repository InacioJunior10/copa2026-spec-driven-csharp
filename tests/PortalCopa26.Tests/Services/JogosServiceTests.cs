using PortalCopa26.Data.Seed;
using PortalCopa26.Domain.Enums;
using PortalCopa26.Services;
using PortalCopa26.Tests.Data;
using Xunit;

namespace PortalCopa26.Tests.Services;

public class JogosServiceTests : IDisposable
{
    private readonly SqliteInMemoryFixture _fixture = new();
    private readonly JogosService _service;

    public JogosServiceTests()
    {
        _service = new JogosService(_fixture);
    }

    public void Dispose() => _fixture.Dispose();

    private async Task SeedAsync()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);
    }

    [Fact]
    public async Task GetLetrasGruposAsync_Retorna_As_Letras_Dos_12_Grupos_Semeados()
    {
        await SeedAsync();

        var letras = await _service.GetLetrasGruposAsync();

        Assert.Equal(12, letras.Count);
        Assert.Equal(letras.OrderBy(l => l), letras);
        Assert.Equal("ABCDEFGHIJKL", new string(letras.ToArray()));
    }

    [Fact]
    public async Task GetJogosAgrupadosPorDiaAsync_Sem_Filtro_Retorna_Os_104_Jogos_Ordenados_Cronologicamente()
    {
        await SeedAsync();

        var dias = await _service.GetJogosAgrupadosPorDiaAsync();

        var todosOsJogos = dias.SelectMany(d => d.Jogos).ToList();
        Assert.Equal(104, todosOsJogos.Count);

        for (var i = 1; i < dias.Count; i++)
        {
            Assert.True(dias[i].Data > dias[i - 1].Data);
        }

        Assert.Equal(new DateOnly(2026, 6, 11), dias[0].Data);
    }

    [Fact]
    public async Task GetJogosAgrupadosPorDiaAsync_Com_Filtro_Retorna_Apenas_Jogos_Da_Fase_De_Grupos_Do_Grupo()
    {
        await SeedAsync();

        var dias = await _service.GetJogosAgrupadosPorDiaAsync('C');
        var jogos = dias.SelectMany(d => d.Jogos).ToList();

        Assert.NotEmpty(jogos);
        Assert.All(jogos, j => Assert.Equal(FaseJogo.Grupos, j.Fase));
        Assert.All(jogos, j => Assert.Equal('C', j.GrupoLetra));
    }

    [Fact]
    public async Task GetJogosAgrupadosPorDiaAsync_Com_Grupo_Sem_Jogos_Correspondentes_Retorna_Vazio()
    {
        await SeedAsync();

        var dias = await _service.GetJogosAgrupadosPorDiaAsync('Z');

        Assert.Empty(dias);
    }

    [Fact]
    public async Task GetJogosAgrupadosPorDiaAsync_Retorna_Placar_Oficial_Quando_Definido()
    {
        await SeedAsync();

        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            var jogo = context.Jogos.OrderBy(j => j.DataHora).First();
            jogo.GolsMandante = 2;
            jogo.GolsVisitante = 1;
            jogoId = jogo.Id;
            await context.SaveChangesAsync();
        }

        var dias = await _service.GetJogosAgrupadosPorDiaAsync();
        var jogoComPlacar = dias.SelectMany(d => d.Jogos).Single(j => j.JogoId == jogoId);

        Assert.Equal(2, jogoComPlacar.GolsMandante);
        Assert.Equal(1, jogoComPlacar.GolsVisitante);
    }
}
