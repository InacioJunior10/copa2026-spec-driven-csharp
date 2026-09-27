namespace PortalCopa26.Services;

/// <summary>Números gerais da Copa exibidos na Landing Page.</summary>
public record EstatisticasCopa(int Selecoes, int Grupos, int Jogos, int Estadios);

/// <summary>Um jogo ainda sem placar oficial, com os dados necessários para exibição.</summary>
public record ProximoJogoItem(
    int JogoId,
    DateTime DataHora,
    string Estadio,
    string Cidade,
    char? GrupoLetra,
    string? MandanteNome,
    string? MandanteCodigo,
    string? VisitanteNome,
    string? VisitanteCodigo,
    string? VagaMandante,
    string? VagaVisitante);

/// <summary>Jogos sem placar oficial de um mesmo dia.</summary>
public record ProximosJogosDia(DateOnly Data, List<ProximoJogoItem> Jogos);

/// <summary>Uma entrada do ranking FIFA para exibição (gráfico ou lista).</summary>
public record RankingItem(int Posicao, string NomeSelecao, string CodigoSelecao, double Pontos);

/// <summary>Acesso a dados para a Landing Page, sem expor o <c>AppDbContext</c> aos componentes Razor.</summary>
public interface ILandingPageService
{
    Task<EstatisticasCopa> GetEstatisticasAsync();

    /// <summary>Jogos sem placar oficial, agrupados pelos 2 dias cronologicamente mais próximos com jogo agendado.</summary>
    Task<List<ProximosJogosDia>> GetProximosJogosAsync();

    Task<List<RankingItem>> GetTopRankingAsync(int quantidade = 10);
}
