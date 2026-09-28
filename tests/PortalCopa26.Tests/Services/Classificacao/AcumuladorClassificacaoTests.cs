using PortalCopa26.Domain.Entities;
using PortalCopa26.Domain.Enums;
using PortalCopa26.Services.Classificacao;
using Xunit;

namespace PortalCopa26.Tests.Services.Classificacao;

public class AcumuladorClassificacaoTests
{
    private static Jogo CriarJogo(int mandanteId, int visitanteId) => new()
    {
        Fase = FaseJogo.Grupos,
        MandanteId = mandanteId,
        VisitanteId = visitanteId,
    };

    [Fact]
    public void Jogo_Sem_Placar_Nao_Conta_Para_Nenhuma_Selecao()
    {
        var jogo = CriarJogo(1, 2);

        var resultado = AcumuladorClassificacao.Acumular([jogo], _ => null);

        Assert.Empty(resultado);
    }

    [Fact]
    public void Vitoria_Contabiliza_Vitoria_Para_Mandante_E_Derrota_Para_Visitante()
    {
        var jogo = CriarJogo(1, 2);

        var resultado = AcumuladorClassificacao.Acumular([jogo], _ => (2, 1));

        var mandante = resultado[1];
        var visitante = resultado[2];

        Assert.Equal(1, mandante.Jogos);
        Assert.Equal(1, mandante.Vitorias);
        Assert.Equal(0, mandante.Empates);
        Assert.Equal(0, mandante.Derrotas);
        Assert.Equal(2, mandante.GolsPro);
        Assert.Equal(1, mandante.GolsContra);
        Assert.Equal(1, mandante.SaldoGols);
        Assert.Equal(3, mandante.Pontos);

        Assert.Equal(1, visitante.Jogos);
        Assert.Equal(0, visitante.Vitorias);
        Assert.Equal(0, visitante.Empates);
        Assert.Equal(1, visitante.Derrotas);
        Assert.Equal(1, visitante.GolsPro);
        Assert.Equal(2, visitante.GolsContra);
        Assert.Equal(-1, visitante.SaldoGols);
        Assert.Equal(0, visitante.Pontos);
    }

    [Fact]
    public void Empate_Contabiliza_Empate_Para_Ambas_As_Selecoes()
    {
        var jogo = CriarJogo(1, 2);

        var resultado = AcumuladorClassificacao.Acumular([jogo], _ => (1, 1));

        Assert.Equal(1, resultado[1].Empates);
        Assert.Equal(1, resultado[1].Pontos);
        Assert.Equal(1, resultado[2].Empates);
        Assert.Equal(1, resultado[2].Pontos);
    }

    [Fact]
    public void Acumula_Gols_Pro_Contra_E_Saldo_Ao_Longo_De_Varios_Jogos()
    {
        var jogo1 = CriarJogo(1, 2);
        var jogo2 = CriarJogo(3, 1);

        var placares = new Dictionary<Jogo, (int, int)>
        {
            [jogo1] = (3, 0), // seleção 1 (mandante) vence
            [jogo2] = (2, 1), // seleção 3 (mandante) vence; seleção 1 (visitante) perde
        };

        var resultado = AcumuladorClassificacao.Acumular([jogo1, jogo2], j => placares[j]);

        var selecao1 = resultado[1];
        Assert.Equal(2, selecao1.Jogos);
        Assert.Equal(1, selecao1.Vitorias);
        Assert.Equal(1, selecao1.Derrotas);
        Assert.Equal(4, selecao1.GolsPro); // 3 (mandante jogo1) + 1 (visitante jogo2)
        Assert.Equal(2, selecao1.GolsContra); // 0 (mandante jogo1) + 2 (visitante jogo2)
        Assert.Equal(2, selecao1.SaldoGols);
        Assert.Equal(3, selecao1.Pontos);
    }
}
