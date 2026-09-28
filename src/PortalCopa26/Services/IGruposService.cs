namespace PortalCopa26.Services;

/// <summary>Uma linha da classificação oficial de um grupo, com gols pró/contra explícitos.</summary>
public record ClassificacaoOficialItem(
    int Posicao,
    string SelecaoNome,
    string SelecaoCodigo,
    int Jogos,
    int Vitorias,
    int Empates,
    int Derrotas,
    int GolsPro,
    int GolsContra,
    int Pontos)
{
    public int SaldoGols => GolsPro - GolsContra;
}

/// <summary>Uma seleção do grupo, com sua posição no ranking FIFA quando disponível.</summary>
public record SelecaoGrupoItem(string Nome, string Codigo, int? RankingPosicao);

/// <summary>
/// Dados completos de um grupo para a página Grupos: seleções, jogos (mesmo <see cref="JogoItem"/> já usado
/// por <see cref="IJogosService"/>, ver design.md D7 do change `criar-grupos-classificacao`) e classificação
/// oficial já calculada e desempatada.
/// </summary>
public record GrupoDados(
    char Grupo,
    List<SelecaoGrupoItem> Selecoes,
    List<JogoItem> Jogos,
    List<ClassificacaoOficialItem> Classificacao);

/// <summary>Acesso a dados e regras de cálculo para a página Grupos, sem expor o <c>AppDbContext</c> aos componentes Razor.</summary>
public interface IGruposService
{
    /// <summary>Letras dos grupos persistidos, ordenadas, para popular as abas.</summary>
    Task<List<char>> GetLetrasGruposAsync();

    /// <summary>Seleções, jogos e classificação oficial do grupo informado.</summary>
    Task<GrupoDados> GetGrupoAsync(char grupo);

    /// <summary>
    /// Registra ou atualiza o placar oficial de um jogo da fase de grupos. Rejeita placares fora da faixa
    /// 0–30 ou jogos que não sejam da fase de grupos.
    /// </summary>
    Task SalvarPlacarOficialAsync(int jogoId, int golsMandante, int golsVisitante);

    /// <summary>Remove o placar oficial de um jogo (volta a "sem placar"), usado quando o usuário apaga os campos.</summary>
    Task LimparPlacarOficialAsync(int jogoId);
}
