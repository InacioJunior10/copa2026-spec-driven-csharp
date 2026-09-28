using PortalCopa26.Domain.Enums;

namespace PortalCopa26.Services;

/// <summary>Um jogador convocado, com participações em Copas nula quando a fonte de dados não informa.</summary>
public record EquipeJogadorItem(string Nome, PosicaoJogador Posicao, int Idade, int Gols, int? ParticipacoesCopas);

/// <summary>Dados completos de uma seleção para a página Equipes: identificação, grupo, técnico, ranking (quando houver) e elenco.</summary>
public record EquipeItem(int SelecaoId, string Codigo, string Nome, char Grupo, string Tecnico, int? RankingPosicao, List<EquipeJogadorItem> Jogadores);

/// <summary>Acesso a dados das seleções e seus elencos para a página Equipes, sem expor o <c>AppDbContext</c> aos componentes Razor.</summary>
public interface ISelecaoService
{
    /// <summary>Todas as seleções participantes, com grupo, técnico, posição no Ranking FIFA (quando houver) e elenco completo.</summary>
    Task<List<EquipeItem>> GetEquipesAsync();
}
