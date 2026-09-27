namespace PortalCopa26.Domain.Entities;

/// <summary>Placar simulado de um jogo dentro de uma <see cref="Simulacao"/>, nunca escrito no placar oficial do <see cref="Jogo"/>.</summary>
public class SimulacaoJogo
{
    public int Id { get; set; }

    public int SimulacaoId { get; set; }
    public Simulacao Simulacao { get; set; } = null!;

    public int JogoId { get; set; }
    public Jogo Jogo { get; set; } = null!;

    public int GolsMandante { get; set; }
    public int GolsVisitante { get; set; }
}
