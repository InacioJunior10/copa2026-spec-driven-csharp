namespace PortalCopa26.Services.Classificacao;

/// <summary>Estatísticas acumuladas de uma seleção a partir de um conjunto de jogos com placar.</summary>
public sealed class EstatisticasSelecao
{
    public int Jogos { get; set; }
    public int Vitorias { get; set; }
    public int Empates { get; set; }
    public int Derrotas { get; set; }
    public int GolsPro { get; set; }
    public int GolsContra { get; set; }
    public int SaldoGols => GolsPro - GolsContra;
    public int Pontos => Vitorias * 3 + Empates;
}
