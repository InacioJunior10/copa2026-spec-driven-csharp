using PortalCopa26.Data.Seed;
using PortalCopa26.Domain.Enums;
using PortalCopa26.Services;
using PortalCopa26.Tests.Data;
using Xunit;

namespace PortalCopa26.Tests.Services;

public class SimuladorServiceTests : IDisposable
{
    private readonly SqliteInMemoryFixture _fixture = new();
    private readonly SimuladorService _service;

    public SimuladorServiceTests()
    {
        _service = new SimuladorService(_fixture);
    }

    public void Dispose() => _fixture.Dispose();

    private async Task SeedAsync()
    {
        using var context = _fixture.CreateContext();
        await SeedData.SeedAsync(context);
    }

    [Fact]
    public async Task GetGrupoSimuladoAsync_Sem_Simulacao_Retorna_Jogos_Sem_Placar_E_Classificacao_Zerada()
    {
        await SeedAsync();

        var grupo = await _service.GetGrupoSimuladoAsync(Guid.NewGuid(), 'A');

        Assert.Equal(6, grupo.Jogos.Count);
        Assert.All(grupo.Jogos, j => Assert.Null(j.GolsMandante));
        Assert.All(grupo.Jogos, j => Assert.Null(j.GolsVisitante));

        Assert.Equal(4, grupo.Classificacao.Count);
        Assert.All(grupo.Classificacao, c => Assert.Equal(0, c.Jogos));
        Assert.All(grupo.Classificacao, c => Assert.Equal(0, c.Pontos));
    }

    [Fact]
    public async Task SalvarPlacarAsync_Reflete_Corretamente_Na_Classificacao()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            var jogo = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'A');
            jogoId = jogo.Id;
        }

        await _service.SalvarPlacarAsync(visitanteId, jogoId, 2, 1);

        var grupo = await _service.GetGrupoSimuladoAsync(visitanteId, 'A');
        var jogoSimulado = grupo.Jogos.Single(j => j.JogoId == jogoId);
        Assert.Equal(2, jogoSimulado.GolsMandante);
        Assert.Equal(1, jogoSimulado.GolsVisitante);

        var vencedor = grupo.Classificacao.Single(c => c.SelecaoCodigo == jogoSimulado.MandanteCodigo);
        var perdedor = grupo.Classificacao.Single(c => c.SelecaoCodigo == jogoSimulado.VisitanteCodigo);

        Assert.Equal(1, vencedor.Jogos);
        Assert.Equal(1, vencedor.Vitorias);
        Assert.Equal(3, vencedor.Pontos);
        Assert.Equal(1, vencedor.SaldoGols);

        Assert.Equal(1, perdedor.Jogos);
        Assert.Equal(1, perdedor.Derrotas);
        Assert.Equal(0, perdedor.Pontos);
        Assert.Equal(-1, perdedor.SaldoGols);
    }

    [Fact]
    public async Task Classificacao_Desempata_Por_Saldo_De_Gols_E_Depois_Por_Gols_Marcados()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        var grupoInicial = await _service.GetGrupoSimuladoAsync(visitanteId, 'B');

        // Empata todos os jogos do grupo 1x1 - todos os 4 times terminam com pontos e saldo iguais.
        foreach (var jogo in grupoInicial.Jogos)
        {
            await _service.SalvarPlacarAsync(visitanteId, jogo.JogoId, 1, 1);
        }

        var grupoEmpatado = await _service.GetGrupoSimuladoAsync(visitanteId, 'B');
        Assert.Equal([1, 2, 3, 4], grupoEmpatado.Classificacao.Select(c => c.Posicao));
        Assert.All(grupoEmpatado.Classificacao, c => Assert.Equal(0, c.SaldoGols));

        // Dá uma vitória de goleada ao mandante do primeiro jogo, quebrando o empate por saldo.
        var primeiroJogo = grupoInicial.Jogos[0];
        await _service.SalvarPlacarAsync(visitanteId, primeiroJogo.JogoId, 3, 1);

        var grupoAtualizado = await _service.GetGrupoSimuladoAsync(visitanteId, 'B');
        var mandanteDoAjuste = grupoAtualizado.Classificacao.Single(c => c.SelecaoCodigo == primeiroJogo.MandanteCodigo);

        Assert.Equal(2, mandanteDoAjuste.SaldoGols);
        Assert.Equal(1, mandanteDoAjuste.Posicao);
    }

    [Fact]
    public async Task Jogo_Sem_Placar_Nao_Conta_Nas_Estatisticas()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            jogoId = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'C').Id;
        }

        await _service.SalvarPlacarAsync(visitanteId, jogoId, 1, 0);

        var grupo = await _service.GetGrupoSimuladoAsync(visitanteId, 'C');

        var jogosSemPlacar = grupo.Jogos.Where(j => j.JogoId != jogoId).ToList();
        Assert.All(jogosSemPlacar, j => Assert.Null(j.GolsMandante));

        var totalJogosContabilizados = grupo.Classificacao.Sum(c => c.Jogos);
        Assert.Equal(2, totalJogosContabilizados); // só o jogo simulado conta, para os 2 times envolvidos
    }

    [Fact]
    public async Task SalvarPlacarAsync_Cria_Simulacao_Na_Primeira_Chamada_E_Reaproveita_Na_Segunda()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        int jogo1Id, jogo2Id;
        using (var context = _fixture.CreateContext())
        {
            var jogosGrupoD = context.Jogos.Where(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'D').Take(2).ToList();
            jogo1Id = jogosGrupoD[0].Id;
            jogo2Id = jogosGrupoD[1].Id;
        }

        await _service.SalvarPlacarAsync(visitanteId, jogo1Id, 1, 1);
        await _service.SalvarPlacarAsync(visitanteId, jogo2Id, 2, 0);

        using var verifyContext = _fixture.CreateContext();
        var simulacoes = verifyContext.Simulacoes.Where(s => s.VisitanteId == visitanteId).ToList();
        Assert.Single(simulacoes);
    }

    [Fact]
    public async Task SalvarPlacarAsync_Chamado_Duas_Vezes_Para_O_Mesmo_Jogo_Atualiza_Em_Vez_De_Duplicar()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            jogoId = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'E').Id;
        }

        await _service.SalvarPlacarAsync(visitanteId, jogoId, 1, 1);
        await _service.SalvarPlacarAsync(visitanteId, jogoId, 3, 2);

        using var verifyContext = _fixture.CreateContext();
        var placares = verifyContext.SimulacoesJogos.Where(sj => sj.JogoId == jogoId).ToList();
        Assert.Single(placares);
        Assert.Equal(3, placares[0].GolsMandante);
        Assert.Equal(2, placares[0].GolsVisitante);
    }

    [Fact]
    public async Task RemoverPlacarAsync_Remove_O_Placar_E_Jogo_Volta_A_Nao_Contar()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        int jogoId;
        using (var context = _fixture.CreateContext())
        {
            jogoId = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'F').Id;
        }

        await _service.SalvarPlacarAsync(visitanteId, jogoId, 1, 0);
        await _service.RemoverPlacarAsync(visitanteId, jogoId);

        var grupo = await _service.GetGrupoSimuladoAsync(visitanteId, 'F');
        var jogo = grupo.Jogos.Single(j => j.JogoId == jogoId);

        Assert.Null(jogo.GolsMandante);
        Assert.Equal(0, grupo.Classificacao.Sum(c => c.Jogos));
    }

    [Fact]
    public async Task LimparGrupoAsync_Remove_So_Os_Placares_Do_Grupo_Informado()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        int jogoGrupoG, jogoGrupoH;
        using (var context = _fixture.CreateContext())
        {
            jogoGrupoG = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'G').Id;
            jogoGrupoH = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'H').Id;
        }

        await _service.SalvarPlacarAsync(visitanteId, jogoGrupoG, 1, 0);
        await _service.SalvarPlacarAsync(visitanteId, jogoGrupoH, 2, 2);

        await _service.LimparGrupoAsync(visitanteId, 'G');

        var grupoG = await _service.GetGrupoSimuladoAsync(visitanteId, 'G');
        var grupoH = await _service.GetGrupoSimuladoAsync(visitanteId, 'H');

        Assert.Equal(0, grupoG.Classificacao.Sum(c => c.Jogos));
        Assert.Equal(2, grupoH.Classificacao.Sum(c => c.Jogos));
    }

    [Fact]
    public async Task LimparTudoAsync_Remove_Todos_Os_Placares_Do_Visitante()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        int jogoGrupoI, jogoGrupoJ;
        using (var context = _fixture.CreateContext())
        {
            jogoGrupoI = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'I').Id;
            jogoGrupoJ = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'J').Id;
        }

        await _service.SalvarPlacarAsync(visitanteId, jogoGrupoI, 1, 0);
        await _service.SalvarPlacarAsync(visitanteId, jogoGrupoJ, 2, 2);

        await _service.LimparTudoAsync(visitanteId);

        var grupoI = await _service.GetGrupoSimuladoAsync(visitanteId, 'I');
        var grupoJ = await _service.GetGrupoSimuladoAsync(visitanteId, 'J');

        Assert.Equal(0, grupoI.Classificacao.Sum(c => c.Jogos));
        Assert.Equal(0, grupoJ.Classificacao.Sum(c => c.Jogos));
    }

    [Fact]
    public async Task SalvarPlacarAsync_Rejeita_Jogo_De_Mata_Mata()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        int jogoMataMataId;
        using (var context = _fixture.CreateContext())
        {
            jogoMataMataId = context.Jogos.First(j => j.Fase != FaseJogo.Grupos).Id;
        }

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SalvarPlacarAsync(visitanteId, jogoMataMataId, 1, 0));
    }

    [Fact]
    public async Task GetProgressoAsync_Sem_Simulacao_Retorna_Zero_Simulados()
    {
        await SeedAsync();

        int totalJogosGrupos;
        using (var context = _fixture.CreateContext())
        {
            totalJogosGrupos = context.Jogos.Count(j => j.Fase == FaseJogo.Grupos);
        }

        var progresso = await _service.GetProgressoAsync(Guid.NewGuid());

        Assert.Equal(totalJogosGrupos, progresso.TotalJogos);
        Assert.Equal(0, progresso.JogosSimulados);
    }

    [Fact]
    public async Task GetProgressoAsync_Conta_Jogos_Simulados_Em_Todos_Os_Grupos()
    {
        await SeedAsync();
        var visitanteId = Guid.NewGuid();

        int jogoGrupoA, jogoGrupoB;
        using (var context = _fixture.CreateContext())
        {
            jogoGrupoA = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'A').Id;
            jogoGrupoB = context.Jogos.First(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == 'B').Id;
        }

        await _service.SalvarPlacarAsync(visitanteId, jogoGrupoA, 1, 0);
        await _service.SalvarPlacarAsync(visitanteId, jogoGrupoB, 2, 2);

        var progresso = await _service.GetProgressoAsync(visitanteId);

        Assert.Equal(2, progresso.JogosSimulados);
    }

    [Fact]
    public async Task GetGrupoSimuladoAsync_Retorna_Selecoes_Do_Grupo_Com_Posicao_No_Ranking()
    {
        await SeedAsync();

        var grupo = await _service.GetGrupoSimuladoAsync(Guid.NewGuid(), 'A');

        Assert.Equal(4, grupo.Selecoes.Count);
        Assert.Contains(grupo.Selecoes, s => s.RankingPosicao is not null);
    }
}
