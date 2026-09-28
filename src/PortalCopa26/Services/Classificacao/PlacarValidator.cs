namespace PortalCopa26.Services.Classificacao;

/// <summary>Validação de faixa (0–30) reaproveitada pelos serviços que gravam placar (simulado ou oficial).</summary>
public static class PlacarValidator
{
    public const int GolsMinimo = 0;
    public const int GolsMaximo = 30;

    public static void ValidarFaixa(int golsMandante, int golsVisitante)
    {
        if (golsMandante is < GolsMinimo or > GolsMaximo || golsVisitante is < GolsMinimo or > GolsMaximo)
        {
            throw new InvalidOperationException($"Placar deve estar entre {GolsMinimo} e {GolsMaximo}.");
        }
    }
}
