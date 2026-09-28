namespace PortalCopa26.Services;

/// <summary>Um jogo da fase de grupos com o placar simulado pelo visitante, quando informado.</summary>
public record JogoSimuladoItem(
    int JogoId,
    DateTime DataHora,
    string Estadio,
    string Cidade,
    string MandanteNome,
    string MandanteCodigo,
    string VisitanteNome,
    string VisitanteCodigo,
    int? GolsMandante,
    int? GolsVisitante);

/// <summary>Uma linha da classificação simulada de um grupo.</summary>
public record ClassificacaoItem(
    int Posicao,
    string SelecaoNome,
    string SelecaoCodigo,
    int Jogos,
    int Vitorias,
    int Empates,
    int Derrotas,
    int SaldoGols,
    int Pontos);

/// <summary>Uma seleção do grupo ativo, com sua posição no ranking FIFA quando disponível.</summary>
public record SelecaoResumoItem(string Nome, string Codigo, int? RankingPosicao);

/// <summary>Dados completos de um grupo para a página Simulador: jogos, classificação e seleções.</summary>
public record SimulacaoGrupo(
    char Grupo,
    List<JogoSimuladoItem> Jogos,
    List<ClassificacaoItem> Classificacao,
    List<SelecaoResumoItem> Selecoes);

/// <summary>Progresso da simulação de um visitante: total de jogos da fase de grupos e quantos já têm placar.</summary>
public record ProgressoSimulacao(int TotalJogos, int JogosSimulados);

/// <summary>Acesso a dados e regras de cálculo para a página Simulador, sem expor o <c>AppDbContext</c> aos componentes Razor.</summary>
public interface ISimuladorService
{
    /// <summary>Jogos, classificação simulada e seleções do grupo informado, para o visitante informado.</summary>
    Task<SimulacaoGrupo> GetGrupoSimuladoAsync(Guid visitanteId, char grupo);

    /// <summary>Total de jogos da fase de grupos e quantos já têm placar simulado pelo visitante, em todos os grupos.</summary>
    Task<ProgressoSimulacao> GetProgressoAsync(Guid visitanteId);

    /// <summary>Grava (cria ou atualiza) o placar simulado de um jogo da fase de grupos para o visitante.</summary>
    Task SalvarPlacarAsync(Guid visitanteId, int jogoId, int golsMandante, int golsVisitante);

    /// <summary>Remove o placar simulado de um jogo (volta a "sem placar").</summary>
    Task RemoverPlacarAsync(Guid visitanteId, int jogoId);

    /// <summary>Remove todos os placares simulados do grupo informado, para o visitante.</summary>
    Task LimparGrupoAsync(Guid visitanteId, char grupo);

    /// <summary>Remove todos os placares simulados do visitante, em todos os grupos.</summary>
    Task LimparTudoAsync(Guid visitanteId);
}
