using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;
using PortalCopa26.Domain.Enums;

namespace PortalCopa26.Services;

public class JogosService(IDbContextFactory<AppDbContext> dbContextFactory) : IJogosService
{
    public async Task<List<char>> GetLetrasGruposAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        return await context.Grupos
            .OrderBy(g => g.Letra)
            .Select(g => g.Letra)
            .ToListAsync();
    }

    public async Task<List<JogosDia>> GetJogosAgrupadosPorDiaAsync(char? grupo = null)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var query = context.Jogos.AsQueryable();
        if (grupo is not null)
        {
            query = query.Where(j => j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == grupo.Value);
        }

        // D4: materializa antes de agrupar por dia — o provider SQLite não tem suporte
        // garantido para GroupBy sobre DateTime.Date traduzido para SQL.
        var jogos = await query
            .Include(j => j.Grupo)
            .Include(j => j.Mandante)
            .Include(j => j.Visitante)
            .OrderBy(j => j.DataHora)
            .ToListAsync();

        return jogos
            .GroupBy(j => j.DataHora.Date)
            .OrderBy(g => g.Key)
            .Select(g => new JogosDia(
                DateOnly.FromDateTime(g.Key),
                g.Select(j => new JogoItem(
                    j.Id,
                    j.Fase,
                    j.DataHora,
                    j.Estadio,
                    j.Cidade,
                    j.Grupo?.Letra,
                    j.Mandante?.Nome,
                    j.Mandante?.Codigo,
                    j.Visitante?.Nome,
                    j.Visitante?.Codigo,
                    j.VagaMandante,
                    j.VagaVisitante,
                    j.GolsMandante,
                    j.GolsVisitante)).ToList()))
            .ToList();
    }
}
