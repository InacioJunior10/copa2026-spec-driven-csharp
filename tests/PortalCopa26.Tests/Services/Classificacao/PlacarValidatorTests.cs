using PortalCopa26.Services.Classificacao;
using Xunit;

namespace PortalCopa26.Tests.Services.Classificacao;

public class PlacarValidatorTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(30, 30)]
    [InlineData(2, 1)]
    public void Faixa_Valida_Nao_Lanca(int golsMandante, int golsVisitante)
    {
        var exception = Record.Exception(() => PlacarValidator.ValidarFaixa(golsMandante, golsVisitante));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(31, 0)]
    [InlineData(0, 31)]
    public void Faixa_Invalida_Lanca_InvalidOperationException(int golsMandante, int golsVisitante)
    {
        Assert.Throws<InvalidOperationException>(() => PlacarValidator.ValidarFaixa(golsMandante, golsVisitante));
    }
}
