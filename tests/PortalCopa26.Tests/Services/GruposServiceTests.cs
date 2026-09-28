using PortalCopa26.Data.Seed;
using PortalCopa26.Domain.Entities;
using PortalCopa26.Domain.Enums;
using PortalCopa26.Services;
using PortalCopa26.Tests.Data;
using Xunit;

namespace PortalCopa26.Tests.Services;

public class GruposServiceTests : IDisposable
{
    private readonly SqliteInMemoryFixture _fixture = new();
    private readonly JogosService _jogosService;
    private readonly GruposService _service;

    public GruposServiceTests()
    {
        _jogosService = new JogosService(_fixture);
        _service = new GruposService(_fixture, _jogosService);
    }

    public void Dispose() => _fixture.Dispose();

    private async Task SeedAsync()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);
    }

    /// <summary>
    /// Cria um grupo isolado (fora dos dados oficiais) com 4 seleções e os jogos informados, para dar
    /// controle total sobre o placar de cada confronto nos testes de desempate. `letra` deve ser uma letra
    /// não usada pelos 12 grupos oficiais (ex.: 'Z', 'Y', 'X').
    /// </summary>
    private async Task<Dictionary<string, int>> SeedGrupoPersonalizadoAsync(
        char letra,
        string[] codigosSelecoes,
        IEnumerable<(string MandanteCodigo, string VisitanteCodigo, int GolsMandante, int GolsVisitante)> jogos,
        Dictionary<string, int>? rankingPorCodigo = null)
    {
        using var context = _fixture.CreateContext();

        var grupo = new Grupo { Letra = letra };
        context.Grupos.Add(grupo);
        await context.SaveChangesAsync();

        var selecoesPorCodigo = new Dictionary<string, Selecao>();
        foreach (var codigo in codigosSelecoes)
        {
            var selecao = new Selecao { Codigo = codigo, Nome = codigo, Tecnico = "Técnico", GrupoId = grupo.Id };
            context.Selecoes.Add(selecao);
            selecoesPorCodigo[codigo] = selecao;
        }
        await context.SaveChangesAsync();

        var numeroBase = 9000 + letra;
        var numero = numeroBase;
        foreach (var (mandanteCodigo, visitanteCodigo, golsMandante, golsVisitante) in jogos)
        {
            context.Jogos.Add(new Jogo
            {
                Numero = numero++,
                Fase = FaseJogo.Grupos,
                Rotulo = $"Grupo {letra}",
                GrupoId = grupo.Id,
                DataHora = new DateTime(2026, 6, 1),
                Estadio = "Estádio Teste",
                Cidade = "Cidade Teste",
                MandanteId = selecoesPorCodigo[mandanteCodigo].Id,
                VisitanteId = selecoesPorCodigo[visitanteCodigo].Id,
                GolsMandante = golsMandante,
                GolsVisitante = golsVisitante,
            });
        }
        await context.SaveChangesAsync();

        if (rankingPorCodigo is not null)
        {
            foreach (var (codigo, posicao) in rankingPorCodigo)
            {
                context.RankingsFifa.Add(new RankingFifa
                {
                    Posicao = posicao,
                    CodigoSelecao = codigo,
                    NomeSelecao = codigo,
                    Pontos = 2000 - posicao,
                    SelecaoId = selecoesPorCodigo[codigo].Id,
                });
            }
            await context.SaveChangesAsync();
        }

        return selecoesPorCodigo.ToDictionary(kv => kv.Key, kv => kv.Value.Id);
    }

    [Fact]
    public async Task GetGrupoAsync_Retorna_Jogos_E_Selecoes_Do_Grupo()
    {
        await SeedAsync();

        var grupo = await _service.GetGrupoAsync('A');

        Assert.Equal(6, grupo.Jogos.Count);
        Assert.Equal(4, grupo.Selecoes.Count);
        Assert.Equal(4, grupo.Classificacao.Count);
    }

    [Fact]
    public async Task GetGrupoAsync_Sem_Placar_Oficial_Retorna_Classificacao_Zerada()
    {
        await SeedAsync();

        var grupo = await _service.GetGrupoAsync('A');

        Assert.All(grupo.Jogos, j => Assert.Null(j.GolsMandante));
        Assert.All(grupo.Classificacao, c => Assert.Equal(0, c.Jogos));
        Assert.All(grupo.Classificacao, c => Assert.Equal(0, c.Pontos));
    }

    [Fact]
    public async Task Classificacao_Ordena_Por_Pontos_Saldo_E_Gols_Marcados_Gerais()
    {
        // T4 vence os 3 jogos que disputa (1º), T1/T2 empatam entre si em tudo (ver próximo teste
        // para o desempate por confronto direto), T3 perde os que disputa.
        await SeedGrupoPersonalizadoAsync('Z', ["T1", "T2", "T3", "T4"],
        [
            ("T1", "T2", 0, 0),
            ("T3", "T4", 0, 3),
            ("T1", "T3", 2, 0),
            ("T2", "T4", 0, 2),
            ("T1", "T4", 0, 2),
            ("T2", "T3", 2, 0),
        ]);

        var grupo = await _service.GetGrupoAsync('Z');

        var t4 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T4");
        var t3 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T3");
        Assert.Equal(1, t4.Posicao);
        Assert.Equal(9, t4.Pontos);
        Assert.Equal(4, t3.Posicao);
        Assert.Equal(0, t3.Pontos);
    }

    [Fact]
    public async Task Desempate_Por_Confronto_Direto_Quando_Pontos_Saldo_E_Gols_Gerais_Empatam()
    {
        // T1 e T2 terminam com pontos, saldo geral e gols marcados geral idênticos (4 pts, GD 0, GF 1),
        // mas T1 venceu o confronto direto entre eles (1-0).
        var ids = await SeedGrupoPersonalizadoAsync('Y', ["T1", "T2", "T3", "T4"],
        [
            ("T1", "T2", 1, 0), // confronto direto: T1 vence
            ("T1", "T3", 0, 0),
            ("T1", "T4", 0, 1),
            ("T2", "T3", 1, 0),
            ("T2", "T4", 0, 0),
            ("T3", "T4", 0, 0),
        ]);

        var grupo = await _service.GetGrupoAsync('Y');

        var t1 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T1");
        var t2 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T2");

        Assert.Equal(t1.Pontos, t2.Pontos);
        Assert.Equal(t1.SaldoGols, t2.SaldoGols);
        Assert.Equal(t1.GolsPro, t2.GolsPro);
        Assert.True(t1.Posicao < t2.Posicao, "T1 venceu o confronto direto e deveria aparecer antes de T2.");
    }

    [Fact]
    public async Task Desempate_Por_Saldo_De_Gols_No_Confronto_Direto_Quando_Confronto_Direto_Tambem_Empata()
    {
        // Empate triangular clássico entre T1, T2 e T3: cada um vence um e perde outro dentro do trio
        // (3 pontos cada no mini-confronto direto), mas com margens de gol diferentes, desempatando
        // por saldo de gols no confronto direto (T1 > T2 > T3). T4 perde os 3 jogos que disputa.
        await SeedGrupoPersonalizadoAsync('X', ["T1", "T2", "T3", "T4"],
        [
            ("T1", "T2", 2, 0), // confronto direto do trio
            ("T2", "T3", 2, 0), // confronto direto do trio
            ("T3", "T1", 1, 0), // confronto direto do trio
            ("T1", "T4", 2, 1),
            ("T2", "T4", 2, 0),
            ("T3", "T4", 3, 0),
        ]);

        var grupo = await _service.GetGrupoAsync('X');

        var t1 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T1");
        var t2 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T2");
        var t3 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T3");

        Assert.Equal(t1.Pontos, t2.Pontos);
        Assert.Equal(t2.Pontos, t3.Pontos);
        Assert.Equal(t1.SaldoGols, t2.SaldoGols);
        Assert.Equal(t2.SaldoGols, t3.SaldoGols);

        Assert.Equal(1, t1.Posicao);
        Assert.Equal(2, t2.Posicao);
        Assert.Equal(3, t3.Posicao);
    }

    [Fact]
    public async Task Desempate_Final_Por_Ranking_Fifa_Quando_Tudo_Mais_Empata()
    {
        // T1 e T2 empatam em tudo, inclusive no confronto direto (0-0) — só o Ranking FIFA decide.
        await SeedGrupoPersonalizadoAsync('W', ["T1", "T2", "T3", "T4"],
        [
            ("T1", "T2", 0, 0),
            ("T1", "T3", 0, 0),
            ("T1", "T4", 0, 0),
            ("T2", "T3", 0, 0),
            ("T2", "T4", 0, 0),
            ("T3", "T4", 0, 0),
        ],
        rankingPorCodigo: new Dictionary<string, int> { ["T1"] = 5, ["T2"] = 50 });

        var grupo = await _service.GetGrupoAsync('W');

        var t1 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T1");
        var t2 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T2");

        Assert.Equal(t1.Pontos, t2.Pontos);
        Assert.Equal(t1.SaldoGols, t2.SaldoGols);
        Assert.True(t1.Posicao < t2.Posicao, "T1 tem melhor posição no Ranking FIFA e deveria aparecer antes de T2.");
    }

    [Fact]
    public async Task Desempate_Final_Quando_Uma_Selecao_Nao_Consta_Do_Ranking_Fifa()
    {
        // T1 e T2 empatam em tudo; só T1 tem entrada no Ranking FIFA.
        await SeedGrupoPersonalizadoAsync('V', ["T1", "T2", "T3", "T4"],
        [
            ("T1", "T2", 0, 0),
            ("T1", "T3", 0, 0),
            ("T1", "T4", 0, 0),
            ("T2", "T3", 0, 0),
            ("T2", "T4", 0, 0),
            ("T3", "T4", 0, 0),
        ],
        rankingPorCodigo: new Dictionary<string, int> { ["T1"] = 10 });

        var grupo = await _service.GetGrupoAsync('V');

        var t1 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T1");
        var t2 = grupo.Classificacao.Single(c => c.SelecaoCodigo == "T2");

        Assert.True(t1.Posicao < t2.Posicao, "T1 consta do Ranking FIFA e T2 não; T1 deveria aparecer antes.");
    }

    [Fact]
    public async Task SalvarPlacarOficialAsync_Registra_Placar_De_Jogo_Sem_Resultado()
    {
        await SeedAsync();
        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            jogoId = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'A').Id;
        }

        await _service.SalvarPlacarOficialAsync(jogoId, 2, 1);

        var grupo = await _service.GetGrupoAsync('A');
        var jogo = grupo.Jogos.Single(j => j.JogoId == jogoId);
        Assert.Equal(2, jogo.GolsMandante);
        Assert.Equal(1, jogo.GolsVisitante);
        Assert.Equal(1, grupo.Classificacao.Sum(c => c.Jogos) / 2);
    }

    [Fact]
    public async Task SalvarPlacarOficialAsync_Atualiza_Placar_Ja_Registrado()
    {
        await SeedAsync();
        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            jogoId = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'B').Id;
        }

        await _service.SalvarPlacarOficialAsync(jogoId, 1, 1);
        await _service.SalvarPlacarOficialAsync(jogoId, 3, 0);

        using var verifyContext = _fixture.CreateContext();
        var jogo = verifyContext.Jogos.Single(j => j.Id == jogoId);
        Assert.Equal(3, jogo.GolsMandante);
        Assert.Equal(0, jogo.GolsVisitante);
    }

    [Fact]
    public async Task LimparPlacarOficialAsync_Volta_Jogo_Para_Sem_Placar()
    {
        await SeedAsync();
        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            jogoId = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'B').Id;
        }

        await _service.SalvarPlacarOficialAsync(jogoId, 2, 1);
        await _service.LimparPlacarOficialAsync(jogoId);

        using var verifyContext = _fixture.CreateContext();
        var jogo = verifyContext.Jogos.Single(j => j.Id == jogoId);
        Assert.Null(jogo.GolsMandante);
        Assert.Null(jogo.GolsVisitante);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 31)]
    public async Task SalvarPlacarOficialAsync_Rejeita_Placar_Fora_Da_Faixa(int golsMandante, int golsVisitante)
    {
        await SeedAsync();
        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            jogoId = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'C').Id;
        }

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SalvarPlacarOficialAsync(jogoId, golsMandante, golsVisitante));

        using var verifyContext = _fixture.CreateContext();
        var jogo = verifyContext.Jogos.Single(j => j.Id == jogoId);
        Assert.Null(jogo.GolsMandante);
        Assert.Null(jogo.GolsVisitante);
    }

    [Fact]
    public async Task SalvarPlacarOficialAsync_Rejeita_Jogo_Que_Nao_E_Da_Fase_De_Grupos()
    {
        await SeedAsync();
        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            jogoId = context.Jogos.First(j => j.Fase != FaseJogo.Grupos).Id;
        }

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SalvarPlacarOficialAsync(jogoId, 1, 0));
    }

    [Fact]
    public async Task SalvarPlacarOficialAsync_Nao_Altera_Simulacoes_Existentes()
    {
        await SeedAsync();
        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            jogoId = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'D').Id;
        }

        var simuladorService = new SimuladorService(_fixture);
        var visitanteId = Guid.NewGuid();
        await simuladorService.SalvarPlacarAsync(visitanteId, jogoId, 1, 1);

        await _service.SalvarPlacarOficialAsync(jogoId, 3, 0);

        using var verifyContext = _fixture.CreateContext();
        var simulacaoJogo = verifyContext.SimulacoesJogos.Single(sj => sj.JogoId == jogoId);
        Assert.Equal(1, simulacaoJogo.GolsMandante);
        Assert.Equal(1, simulacaoJogo.GolsVisitante);

        var jogoOficial = verifyContext.Jogos.Single(j => j.Id == jogoId);
        Assert.Equal(3, jogoOficial.GolsMandante);
        Assert.Equal(0, jogoOficial.GolsVisitante);
    }
}
