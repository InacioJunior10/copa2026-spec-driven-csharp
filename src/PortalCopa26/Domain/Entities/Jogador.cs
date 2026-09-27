using PortalCopa26.Domain.Enums;

namespace PortalCopa26.Domain.Entities;

public class Jogador
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public PosicaoJogador Posicao { get; set; }
    public int Idade { get; set; }
    public int Gols { get; set; }

    /// <summary>Nulo quando a fonte de dados não informa participações em Copas.</summary>
    public int? ParticipacoesCopas { get; set; }

    public int SelecaoId { get; set; }
    public Selecao Selecao { get; set; } = null!;
}
