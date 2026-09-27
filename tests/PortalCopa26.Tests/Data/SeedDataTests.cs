using PortalCopa26.Data.Seed;
using PortalCopa26.Domain.Enums;
using Xunit;

namespace PortalCopa26.Tests.Data;

public class SeedDataTests : IDisposable
{
    private readonly SqliteInMemoryFixture _fixture = new();

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task Banco_Vazio_E_Populado_Com_As_Contagens_Esperadas()
    {
        using var context = _fixture.CreateContext();

        await SeedData.SeedAsync(context);

        Assert.Equal(12, context.Grupos.Count());
        Assert.Equal(48, context.Selecoes.Count());
        Assert.Equal(104, context.Jogos.Count());
        Assert.Equal(72, context.Jogos.Count(j => j.Fase == FaseJogo.Grupos));
    }

    [Fact]
    public async Task Todos_Os_Grupos_Tem_Exatamente_Quatro_Selecoes()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);

        var contagemPorGrupo = context.Selecoes
            .GroupBy(s => s.GrupoId)
            .Select(g => g.Count())
            .ToList();

        Assert.Equal(12, contagemPorGrupo.Count);
        Assert.All(contagemPorGrupo, c => Assert.Equal(4, c));
    }

    [Fact]
    public async Task Toda_Selecao_Tem_Elenco_Nao_Vazio()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);

        var selecoesSemElenco = context.Selecoes
            .Where(s => !context.Jogadores.Any(j => j.SelecaoId == s.Id))
            .ToList();

        Assert.Empty(selecoesSemElenco);
    }

    [Fact]
    public async Task Jogos_Da_Fase_De_Grupos_Sao_Entre_Selecoes_Do_Mesmo_Grupo()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);

        var jogosDeGrupo = context.Jogos
            .Where(j => j.Fase == FaseJogo.Grupos)
            .ToList();

        Assert.Equal(72, jogosDeGrupo.Count);
        foreach (var jogo in jogosDeGrupo)
        {
            var mandante = context.Selecoes.Single(s => s.Id == jogo.MandanteId);
            var visitante = context.Selecoes.Single(s => s.Id == jogo.VisitanteId);
            Assert.Equal(jogo.GrupoId, mandante.GrupoId);
            Assert.Equal(jogo.GrupoId, visitante.GrupoId);
        }
    }

    [Fact]
    public async Task Brasil_Esta_No_Grupo_C_Com_Tecnico()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);

        var brasil = context.Selecoes.Single(s => s.Codigo == "BRA");
        var grupo = context.Grupos.Single(g => g.Id == brasil.GrupoId);

        Assert.Equal('C', grupo.Letra);
        Assert.False(string.IsNullOrWhiteSpace(brasil.Tecnico));
    }

    [Fact]
    public async Task Final_Nao_Tem_Selecoes_Definidas_E_Tem_As_Vagas_Das_Semifinais()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);

        var final = context.Jogos.Single(j => j.Fase == FaseJogo.Final);

        Assert.Null(final.MandanteId);
        Assert.Null(final.VisitanteId);
        Assert.Equal("Venc. Semifinal 1", final.VagaMandante);
        Assert.Equal("Venc. Semifinal 2", final.VagaVisitante);
    }

    [Fact]
    public async Task Jogos_Estao_Em_Ordem_Cronologica_Da_Abertura_A_Final()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);

        var datas = context.Jogos.OrderBy(j => j.Numero).Select(j => j.DataHora).ToList();

        Assert.Equal(new DateTime(2026, 6, 11, 16, 0, 0), datas.First());
        Assert.Equal(new DateTime(2026, 7, 19, 16, 0, 0), datas.Last());
        for (var i = 1; i < datas.Count; i++)
        {
            Assert.True(datas[i] >= datas[i - 1], $"Jogo {i + 1} está fora de ordem cronológica.");
        }
    }

    [Fact]
    public async Task Segunda_Execucao_Nao_Duplica_Dados()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);

        var grupos = context.Grupos.Count();
        var selecoes = context.Selecoes.Count();
        var jogadores = context.Jogadores.Count();
        var jogos = context.Jogos.Count();
        var ranking = context.RankingsFifa.Count();

        await SeedData.SeedAsync(context);

        Assert.Equal(grupos, context.Grupos.Count());
        Assert.Equal(selecoes, context.Selecoes.Count());
        Assert.Equal(jogadores, context.Jogadores.Count());
        Assert.Equal(jogos, context.Jogos.Count());
        Assert.Equal(ranking, context.RankingsFifa.Count());
    }
}
