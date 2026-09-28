using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;

namespace PortalCopa26.Services;

public class SelecaoService(IDbContextFactory<AppDbContext> dbContextFactory) : ISelecaoService
{
    public async Task<List<EquipeItem>> GetEquipesAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        var selecoes = await context.Selecoes
            .Include(s => s.Grupo)
            .Include(s => s.Jogadores)
            .OrderBy(s => s.Nome)
            .ToListAsync();

        var selecaoIds = selecoes.Select(s => s.Id).ToList();
        var rankings = await context.RankingsFifa
            .Where(r => r.SelecaoId != null && selecaoIds.Contains(r.SelecaoId!.Value))
            .ToListAsync();

        return selecoes
            .Select(s => new EquipeItem(
                s.Id,
                s.Codigo,
                s.Nome,
                s.Grupo.Letra,
                s.Tecnico,
                rankings.FirstOrDefault(r => r.SelecaoId == s.Id)?.Posicao,
                s.Jogadores
                    .Select(j => new EquipeJogadorItem(j.Nome, j.Posicao, j.Idade, j.Gols, j.ParticipacoesCopas))
                    .ToList()))
            .ToList();
    }
}
