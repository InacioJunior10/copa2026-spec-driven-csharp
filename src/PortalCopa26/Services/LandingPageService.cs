using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;

namespace PortalCopa26.Services;

public class LandingPageService(IDbContextFactory<AppDbContext> dbContextFactory) : ILandingPageService
{
    public async Task<EstatisticasCopa> GetEstatisticasAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var selecoes = await context.Selecoes.CountAsync();
        var grupos = await context.Grupos.CountAsync();
        var jogos = await context.Jogos.CountAsync();
        var estadios = await context.Jogos.Select(j => j.Estadio).Distinct().CountAsync();

        return new EstatisticasCopa(selecoes, grupos, jogos, estadios);
    }

    public async Task<List<ProximosJogosDia>> GetProximosJogosAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        // D3: materializa antes de agrupar por dia — o provider SQLite não tem suporte
        // garantido para GroupBy sobre DateTime.Date traduzido para SQL.
        var jogosSemPlacar = await context.Jogos
            .Where(j => j.GolsMandante == null && j.GolsVisitante == null)
            .Include(j => j.Grupo)
            .Include(j => j.Mandante)
            .Include(j => j.Visitante)
            .OrderBy(j => j.DataHora)
            .ToListAsync();

        return jogosSemPlacar
            .GroupBy(j => j.DataHora.Date)
            .OrderBy(g => g.Key)
            .Take(2)
            .Select(g => new ProximosJogosDia(
                DateOnly.FromDateTime(g.Key),
                g.Select(j => new ProximoJogoItem(
                    j.Id,
                    j.DataHora,
                    j.Estadio,
                    j.Cidade,
                    j.Grupo?.Letra,
                    j.Mandante?.Nome,
                    j.Mandante?.Codigo,
                    j.Visitante?.Nome,
                    j.Visitante?.Codigo,
                    j.VagaMandante,
                    j.VagaVisitante)).ToList()))
            .ToList();
    }

    public async Task<List<RankingItem>> GetTopRankingAsync(int quantidade = 10)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        return await context.RankingsFifa
            .OrderBy(r => r.Posicao)
            .Take(quantidade)
            .Select(r => new RankingItem(r.Posicao, r.NomeSelecao, r.CodigoSelecao, r.Pontos))
            .ToListAsync();
    }
}
