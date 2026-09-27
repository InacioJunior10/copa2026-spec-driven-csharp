namespace PortalCopa26.Domain.Entities;

public class RankingFifa
{
    public int Id { get; set; }
    public int Posicao { get; set; }
    public string CodigoSelecao { get; set; } = string.Empty;
    public string NomeSelecao { get; set; } = string.Empty;
    public double Pontos { get; set; }

    /// <summary>Preenchido apenas quando a seleção ranqueada também participa da Copa 2026.</summary>
    public int? SelecaoId { get; set; }
    public Selecao? Selecao { get; set; }
}
