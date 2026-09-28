using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;
using PortalCopa26.Domain.Entities;
using PortalCopa26.Domain.Enums;
using PortalCopa26.Services.Classificacao;

namespace PortalCopa26.Services;

public class GruposService(IDbContextFactory<AppDbContext> dbContextFactory, IJogosService jogosService) : IGruposService
{
    public Task<List<char>> GetLetrasGruposAsync() => jogosService.GetLetrasGruposAsync();

    public async Task<GrupoDados> GetGrupoAsync(char grupo)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var jogos = await context.Jogos
            .Where(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == grupo)
            .Include(j => j.Mandante)
            .Include(j => j.Visitante)
            .OrderBy(j => j.DataHora)
            .ToListAsync();

        var jogoItems = jogos.Select(j => new JogoItem(
            j.Id,
            j.Fase,
            j.DataHora,
            j.Estadio,
            j.Cidade,
            grupo,
            j.Mandante?.Nome,
            j.Mandante?.Codigo,
            j.Visitante?.Nome,
            j.Visitante?.Codigo,
            j.VagaMandante,
            j.VagaVisitante,
            j.GolsMandante,
            j.GolsVisitante)).ToList();

        var selecoesGrupo = await context.Selecoes
            .Where(s => s.Grupo.Letra == grupo)
            .OrderBy(s => s.Nome)
            .ToListAsync();

        var selecaoIds = selecoesGrupo.Select(s => s.Id).ToList();
        var rankings = await context.RankingsFifa
            .Where(r => r.SelecaoId != null && selecaoIds.Contains(r.SelecaoId!.Value))
            .ToListAsync();

        var selecoesItems = selecoesGrupo
            .Select(s => new SelecaoGrupoItem(
                s.Nome,
                s.Codigo,
                rankings.FirstOrDefault(r => r.SelecaoId == s.Id)?.Posicao))
            .ToList();

        var classificacao = CalcularClassificacaoOficial(selecoesGrupo, jogos, rankings);

        return new GrupoDados(grupo, selecoesItems, jogoItems, classificacao);
    }

    public async Task SalvarPlacarOficialAsync(int jogoId, int golsMandante, int golsVisitante)
    {
        PlacarValidator.ValidarFaixa(golsMandante, golsVisitante);

        await using var context = await dbContextFactory.CreateDbContextAsync();

        var jogo = await context.Jogos.FindAsync(jogoId)
            ?? throw new InvalidOperationException($"Jogo {jogoId} não encontrado.");

        if (jogo.Fase != FaseJogo.Grupos)
        {
            throw new InvalidOperationException("Somente jogos da fase de grupos podem ter placar oficial registrado por esta página.");
        }

        jogo.GolsMandante = golsMandante;
        jogo.GolsVisitante = golsVisitante;
        await context.SaveChangesAsync();
    }

    public async Task LimparPlacarOficialAsync(int jogoId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var jogo = await context.Jogos.FindAsync(jogoId)
            ?? throw new InvalidOperationException($"Jogo {jogoId} não encontrado.");

        jogo.GolsMandante = null;
        jogo.GolsVisitante = null;
        await context.SaveChangesAsync();
    }

    // D3: pontos -> saldo geral -> gols marcados geral -> confronto direto -> saldo no confronto direto -> Ranking FIFA.
    // Cartões (RN-01) não é aplicado por falta de fonte de dados (ver design.md D3/D_Non-Goals).
    private static List<ClassificacaoOficialItem> CalcularClassificacaoOficial(
        List<Selecao> selecoes, List<Jogo> jogos, List<RankingFifa> rankings)
    {
        var acumulados = AcumuladorClassificacao.Acumular(jogos, ObterPlacarOficial);

        var rankingPorSelecao = rankings
            .Where(r => r.SelecaoId is not null)
            .ToDictionary(r => r.SelecaoId!.Value, r => r.Posicao);

        var itens = selecoes
            .Select(s => (Selecao: s, Estatisticas: acumulados.GetValueOrDefault(s.Id) ?? new EstatisticasSelecao()))
            .ToList();

        var ordenados = OrdenarComDesempate(itens, jogos, rankingPorSelecao);

        return ordenados
            .Select((x, i) => new ClassificacaoOficialItem(
                i + 1,
                x.Selecao.Nome,
                x.Selecao.Codigo,
                x.Estatisticas.Jogos,
                x.Estatisticas.Vitorias,
                x.Estatisticas.Empates,
                x.Estatisticas.Derrotas,
                x.Estatisticas.GolsPro,
                x.Estatisticas.GolsContra,
                x.Estatisticas.Pontos))
            .ToList();
    }

    private static (int GolsMandante, int GolsVisitante)? ObterPlacarOficial(Jogo jogo) =>
        jogo.GolsMandante is int golsMandante && jogo.GolsVisitante is int golsVisitante
            ? (golsMandante, golsVisitante)
            : null;

    private static List<(Selecao Selecao, EstatisticasSelecao Estatisticas)> OrdenarComDesempate(
        List<(Selecao Selecao, EstatisticasSelecao Estatisticas)> itens,
        List<Jogo> jogos,
        Dictionary<int, int> rankingPorSelecao)
    {
        var ordenadosGeral = itens
            .OrderByDescending(x => x.Estatisticas.Pontos)
            .ThenByDescending(x => x.Estatisticas.SaldoGols)
            .ThenByDescending(x => x.Estatisticas.GolsPro)
            .ToList();

        var resultado = new List<(Selecao, EstatisticasSelecao)>();
        var indice = 0;
        while (indice < ordenadosGeral.Count)
        {
            var atual = ordenadosGeral[indice];
            var grupoEmpatado = new List<(Selecao Selecao, EstatisticasSelecao Estatisticas)> { atual };

            var proximo = indice + 1;
            while (proximo < ordenadosGeral.Count && SaoEmpatados(atual.Estatisticas, ordenadosGeral[proximo].Estatisticas))
            {
                grupoEmpatado.Add(ordenadosGeral[proximo]);
                proximo++;
            }

            resultado.AddRange(grupoEmpatado.Count == 1
                ? grupoEmpatado
                : DesempatarGrupo(grupoEmpatado, jogos, rankingPorSelecao));

            indice = proximo;
        }

        return resultado;
    }

    private static bool SaoEmpatados(EstatisticasSelecao a, EstatisticasSelecao b) =>
        a.Pontos == b.Pontos && a.SaldoGols == b.SaldoGols && a.GolsPro == b.GolsPro;

    // Confronto direto (D3): mini-liga só com os jogos entre as seleções empatadas, desempatada por
    // pontos-no-confronto-direto -> saldo-de-gols-no-confronto-direto -> Ranking FIFA (menor posição primeiro;
    // seleção sem Ranking FIFA fica depois de qualquer seleção ranqueada).
    private static List<(Selecao Selecao, EstatisticasSelecao Estatisticas)> DesempatarGrupo(
        List<(Selecao Selecao, EstatisticasSelecao Estatisticas)> empatados,
        List<Jogo> jogos,
        Dictionary<int, int> rankingPorSelecao)
    {
        var idsEmpatados = empatados.Select(x => x.Selecao.Id).ToHashSet();
        var jogosEntreEmpatados = jogos.Where(j =>
            j.MandanteId is int mandanteId && j.VisitanteId is int visitanteId &&
            idsEmpatados.Contains(mandanteId) && idsEmpatados.Contains(visitanteId));

        var miniAcumulados = AcumuladorClassificacao.Acumular(jogosEntreEmpatados, ObterPlacarOficial);

        return empatados
            .OrderByDescending(x => miniAcumulados.GetValueOrDefault(x.Selecao.Id)?.Pontos ?? 0)
            .ThenByDescending(x => miniAcumulados.GetValueOrDefault(x.Selecao.Id)?.SaldoGols ?? 0)
            .ThenBy(x => rankingPorSelecao.TryGetValue(x.Selecao.Id, out var posicao) ? posicao : int.MaxValue)
            .ToList();
    }
}
