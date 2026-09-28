using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;
using PortalCopa26.Domain.Entities;
using PortalCopa26.Domain.Enums;
using PortalCopa26.Services.Classificacao;

namespace PortalCopa26.Services;

public class SimuladorService(IDbContextFactory<AppDbContext> dbContextFactory) : ISimuladorService
{
    public async Task<SimulacaoGrupo> GetGrupoSimuladoAsync(Guid visitanteId, char grupo)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var jogos = await context.Jogos
            .Where(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == grupo)
            .Include(j => j.Mandante)
            .Include(j => j.Visitante)
            .OrderBy(j => j.DataHora)
            .ToListAsync();

        var jogoIds = jogos.Select(j => j.Id).ToList();

        var placares = await context.SimulacoesJogos
            .Where(sj => sj.Simulacao.VisitanteId == visitanteId && jogoIds.Contains(sj.JogoId))
            .ToDictionaryAsync(sj => sj.JogoId);

        var jogoItems = jogos.Select(j =>
        {
            placares.TryGetValue(j.Id, out var placar);
            return new JogoSimuladoItem(
                j.Id,
                j.DataHora,
                j.Estadio,
                j.Cidade,
                j.Mandante!.Nome,
                j.Mandante.Codigo,
                j.Visitante!.Nome,
                j.Visitante.Codigo,
                placar?.GolsMandante,
                placar?.GolsVisitante);
        }).ToList();

        var classificacao = CalcularClassificacao(jogos, placares);

        var selecoesGrupo = await context.Selecoes
            .Where(s => s.Grupo.Letra == grupo)
            .OrderBy(s => s.Nome)
            .ToListAsync();

        var selecaoIds = selecoesGrupo.Select(s => s.Id).ToList();
        var rankings = await context.RankingsFifa
            .Where(r => r.SelecaoId != null && selecaoIds.Contains(r.SelecaoId!.Value))
            .ToListAsync();

        var selecoesItems = selecoesGrupo
            .Select(s => new SelecaoResumoItem(
                s.Nome,
                s.Codigo,
                rankings.FirstOrDefault(r => r.SelecaoId == s.Id)?.Posicao))
            .ToList();

        return new SimulacaoGrupo(grupo, jogoItems, classificacao, selecoesItems);
    }

    public async Task<ProgressoSimulacao> GetProgressoAsync(Guid visitanteId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var totalJogos = await context.Jogos.CountAsync(j => j.Fase == FaseJogo.Grupos);

        var jogosSimulados = await context.SimulacoesJogos
            .Where(sj => sj.Simulacao.VisitanteId == visitanteId && sj.Jogo.Fase == FaseJogo.Grupos)
            .CountAsync();

        return new ProgressoSimulacao(totalJogos, jogosSimulados);
    }

    public async Task SalvarPlacarAsync(Guid visitanteId, int jogoId, int golsMandante, int golsVisitante)
    {
        PlacarValidator.ValidarFaixa(golsMandante, golsVisitante);

        await using var context = await dbContextFactory.CreateDbContextAsync();

        var jogo = await context.Jogos.FindAsync(jogoId)
            ?? throw new InvalidOperationException($"Jogo {jogoId} não encontrado.");

        if (jogo.Fase != FaseJogo.Grupos)
        {
            throw new InvalidOperationException("Somente jogos da fase de grupos podem ser simulados.");
        }

        var simulacao = await context.Simulacoes.FirstOrDefaultAsync(s => s.VisitanteId == visitanteId);
        if (simulacao is null)
        {
            simulacao = new Simulacao
            {
                VisitanteId = visitanteId,
                CriadaEm = DateTime.UtcNow,
                AtualizadaEm = DateTime.UtcNow,
            };
            context.Simulacoes.Add(simulacao);
            await context.SaveChangesAsync();
        }

        var simulacaoJogo = await context.SimulacoesJogos
            .FirstOrDefaultAsync(sj => sj.SimulacaoId == simulacao.Id && sj.JogoId == jogoId);

        if (simulacaoJogo is null)
        {
            context.SimulacoesJogos.Add(new SimulacaoJogo
            {
                SimulacaoId = simulacao.Id,
                JogoId = jogoId,
                GolsMandante = golsMandante,
                GolsVisitante = golsVisitante,
            });
        }
        else
        {
            simulacaoJogo.GolsMandante = golsMandante;
            simulacaoJogo.GolsVisitante = golsVisitante;
        }

        simulacao.AtualizadaEm = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task RemoverPlacarAsync(Guid visitanteId, int jogoId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var simulacaoJogo = await context.SimulacoesJogos
            .Include(sj => sj.Simulacao)
            .FirstOrDefaultAsync(sj => sj.Simulacao.VisitanteId == visitanteId && sj.JogoId == jogoId);

        if (simulacaoJogo is null)
        {
            return;
        }

        context.SimulacoesJogos.Remove(simulacaoJogo);
        simulacaoJogo.Simulacao.AtualizadaEm = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task LimparGrupoAsync(Guid visitanteId, char grupo)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var simulacao = await context.Simulacoes.FirstOrDefaultAsync(s => s.VisitanteId == visitanteId);
        if (simulacao is null)
        {
            return;
        }

        var placaresDoGrupo = await context.SimulacoesJogos
            .Include(sj => sj.Jogo)
            .Where(sj => sj.SimulacaoId == simulacao.Id && sj.Jogo.Grupo!.Letra == grupo)
            .ToListAsync();

        context.SimulacoesJogos.RemoveRange(placaresDoGrupo);
        simulacao.AtualizadaEm = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task LimparTudoAsync(Guid visitanteId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var simulacao = await context.Simulacoes
            .Include(s => s.Jogos)
            .FirstOrDefaultAsync(s => s.VisitanteId == visitanteId);

        if (simulacao is null)
        {
            return;
        }

        context.SimulacoesJogos.RemoveRange(simulacao.Jogos);
        simulacao.AtualizadaEm = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    // D2: replica calcStandingsSim do protótipo — pts -> saldo de gols -> gols marcados, sem outros critérios.
    private static List<ClassificacaoItem> CalcularClassificacao(List<Jogo> jogos, Dictionary<int, SimulacaoJogo> placares)
    {
        var selecoesPorId = new Dictionary<int, Selecao>();
        foreach (var jogo in jogos)
        {
            selecoesPorId[jogo.Mandante!.Id] = jogo.Mandante!;
            selecoesPorId[jogo.Visitante!.Id] = jogo.Visitante!;
        }

        var acumulados = AcumuladorClassificacao.Acumular(
            jogos,
            jogo => placares.TryGetValue(jogo.Id, out var placar) ? (placar.GolsMandante, placar.GolsVisitante) : null);

        return selecoesPorId.Values
            .Select(selecao => (Selecao: selecao, Estatisticas: acumulados.GetValueOrDefault(selecao.Id) ?? new EstatisticasSelecao()))
            .OrderByDescending(x => x.Estatisticas.Pontos)
            .ThenByDescending(x => x.Estatisticas.SaldoGols)
            .ThenByDescending(x => x.Estatisticas.GolsPro)
            .Select((x, i) => new ClassificacaoItem(
                i + 1,
                x.Selecao.Nome,
                x.Selecao.Codigo,
                x.Estatisticas.Jogos,
                x.Estatisticas.Vitorias,
                x.Estatisticas.Empates,
                x.Estatisticas.Derrotas,
                x.Estatisticas.SaldoGols,
                x.Estatisticas.Pontos))
            .ToList();
    }
}
