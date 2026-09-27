namespace PortalCopa26.Domain.Entities;

/// <summary>Uma rodada de simulação pertencente a um visitante anônimo (sem login).</summary>
public class Simulacao
{
    public int Id { get; set; }
    public Guid VisitanteId { get; set; }
    public DateTime CriadaEm { get; set; }
    public DateTime AtualizadaEm { get; set; }

    public List<SimulacaoJogo> Jogos { get; set; } = [];
}
