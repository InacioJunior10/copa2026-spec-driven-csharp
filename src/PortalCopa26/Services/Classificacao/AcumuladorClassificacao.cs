using PortalCopa26.Domain.Entities;

namespace PortalCopa26.Services.Classificacao;

/// <summary>
/// Acumula estatísticas de classificação (jogos, V/E/D, gols pró/contra) por seleção, a partir de uma
/// lista de jogos e de uma função que resolve o placar de cada jogo. Um jogo sem placar (a função devolve
/// <c>null</c>) não conta nas estatísticas de nenhuma das duas seleções. Reaproveitado por `SimuladorService`
/// (placares simulados) e `GruposService` (placares oficiais) — ver design.md D2 do change `criar-grupos-classificacao`.
/// </summary>
public static class AcumuladorClassificacao
{
    public static Dictionary<int, EstatisticasSelecao> Acumular(
        IEnumerable<Jogo> jogos,
        Func<Jogo, (int GolsMandante, int GolsVisitante)?> obterPlacar)
    {
        var acumulados = new Dictionary<int, EstatisticasSelecao>();

        EstatisticasSelecao ObterOuCriar(int selecaoId)
        {
            if (!acumulados.TryGetValue(selecaoId, out var estatisticas))
            {
                estatisticas = new EstatisticasSelecao();
                acumulados[selecaoId] = estatisticas;
            }
            return estatisticas;
        }

        foreach (var jogo in jogos)
        {
            var placar = obterPlacar(jogo);
            if (placar is null || jogo.MandanteId is null || jogo.VisitanteId is null)
            {
                continue;
            }

            var mandante = ObterOuCriar(jogo.MandanteId.Value);
            var visitante = ObterOuCriar(jogo.VisitanteId.Value);

            var (golsMandante, golsVisitante) = placar.Value;

            mandante.Jogos++;
            visitante.Jogos++;
            mandante.GolsPro += golsMandante;
            mandante.GolsContra += golsVisitante;
            visitante.GolsPro += golsVisitante;
            visitante.GolsContra += golsMandante;

            if (golsMandante > golsVisitante)
            {
                mandante.Vitorias++;
                visitante.Derrotas++;
            }
            else if (golsMandante < golsVisitante)
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

        return acumulados;
    }
}
