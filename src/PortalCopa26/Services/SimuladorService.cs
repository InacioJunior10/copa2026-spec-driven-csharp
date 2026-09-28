using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;
using PortalCopa26.Domain.Entities;
using PortalCopa26.Domain.Enums;

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
        if (golsMandante is < 0 or > 30 || golsVisitante is < 0 or > 30)
        {
            throw new InvalidOperationException("Placar simulado deve estar entre 0 e 30.");
        }

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
        var acumulados = new Dictionary<int, Acumulado>();

        Acumulado ObterOuCriar(Selecao selecao)
        {
            if (!acumulados.TryGetValue(selecao.Id, out var acumulado))
            {
                acumulado = new Acumulado(selecao.Nome, selecao.Codigo);
                acumulados[selecao.Id] = acumulado;
            }
            return acumulado;
        }

        foreach (var jogo in jogos)
        {
            var mandante = ObterOuCriar(jogo.Mandante!);
            var visitante = ObterOuCriar(jogo.Visitante!);

            if (!placares.TryGetValue(jogo.Id, out var placar))
            {
                continue;
            }

            mandante.Jogos++;
            visitante.Jogos++;
            mandante.GolsPro += placar.GolsMandante;
            mandante.GolsContra += placar.GolsVisitante;
            visitante.GolsPro += placar.GolsVisitante;
            visitante.GolsContra += placar.GolsMandante;

            if (placar.GolsMandante > placar.GolsVisitante)
            {
                mandante.Vitorias++;
                visitante.Derrotas++;
            }
            else if (placar.GolsMandante < placar.GolsVisitante)
            {
                visitante.Vitorias++;
                mandante.Derrotas++;
            }
            else
            {
                mandante.Empates++;
                visitante.Empates++;
            }
        }

        return acumulados.Values
            .OrderByDescending(a => a.Pontos)
            .ThenByDescending(a => a.SaldoGols)
            .ThenByDescending(a => a.GolsPro)
            .Select((a, i) => new ClassificacaoItem(
                i + 1,
                a.Nome,
                a.Codigo,
                a.Jogos,
                a.Vitorias,
                a.Empates,
                a.Derrotas,
                a.SaldoGols,
                a.Pontos))
            .ToList();
    }

    private sealed class Acumulado(string nome, string codigo)
    {
        public string Nome { get; } = nome;
        public string Codigo { get; } = codigo;
        public int Jogos { get; set; }
        public int Vitorias { get; set; }
        public int Empates { get; set; }
        public int Derrotas { get; set; }
        public int GolsPro { get; set; }
        public int GolsContra { get; set; }
        public int SaldoGols => GolsPro - GolsContra;
        public int Pontos => Vitorias * 3 + Empates;
    }
}
