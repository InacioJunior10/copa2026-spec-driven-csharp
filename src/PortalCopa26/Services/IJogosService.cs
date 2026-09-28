using PortalCopa26.Domain.Enums;

namespace PortalCopa26.Services;

/// <summary>Um jogo exibido na página Jogos, com placar oficial quando disputado.</summary>
public record JogoItem(
    int JogoId,
    FaseJogo Fase,
    DateTime DataHora,
    string Estadio,
    string Cidade,
    char? GrupoLetra,
    string? MandanteNome,
    string? MandanteCodigo,
    string? VisitanteNome,
    string? VisitanteCodigo,
    string? VagaMandante,
    string? VagaVisitante,
    int? GolsMandante,
    int? GolsVisitante);

/// <summary>Jogos de um mesmo dia, para a listagem agrupada da página Jogos.</summary>
public record JogosDia(DateOnly Data, List<JogoItem> Jogos);

/// <summary>Acesso a dados para a página Jogos, sem expor o <c>AppDbContext</c> aos componentes Razor.</summary>
public interface IJogosService
{
    /// <summary>Letras dos grupos persistidos, ordenadas, para popular o filtro.</summary>
    Task<List<char>> GetLetrasGruposAsync();

    /// <summary>
    /// Todos os jogos agrupados por dia, ordenados cronologicamente. Quando <paramref name="grupo"/>
    /// é informado, restringe aos jogos da fase de grupos daquele grupo; sem filtro, inclui todas as fases.
    /// </summary>
    Task<List<JogosDia>> GetJogosAgrupadosPorDiaAsync(char? grupo = null);
}
