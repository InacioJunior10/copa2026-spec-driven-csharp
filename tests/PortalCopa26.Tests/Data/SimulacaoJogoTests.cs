using Microsoft.EntityFrameworkCore;
using PortalCopa26.Domain.Entities;
using PortalCopa26.Domain.Enums;
using Xunit;

namespace PortalCopa26.Tests.Data;

public class SimulacaoJogoTests : IDisposable
{
    // Uma fixture nova por teste (não IClassFixture): xUnit cria uma instância
    // desta classe por método de teste, garantindo bancos isolados entre eles.
    private readonly SqliteInMemoryFixture _fixture = new();

    public void Dispose() => _fixture.Dispose();

    private static Jogo NovoJogoComSelecoes(AppDbContextSeedResult seed)
    {
        return new Jogo
        {
            Numero = 1,
            Fase = FaseJogo.Grupos,
            Rotulo = "Grupo A - Jogo 1",
            GrupoId = seed.Grupo.Id,
            DataHora = new DateTime(2026, 6, 11, 16, 0, 0),
            Estadio = "Estadio Azteca",
            Cidade = "Cidade do México",
            MandanteId = seed.Mandante.Id,
            VisitanteId = seed.Visitante.Id,
            GolsMandante = null,
            GolsVisitante = null,
        };
    }

    private static AppDbContextSeedResult SeedGrupoESelecoes(PortalCopa26.Data.AppDbContext context)
    {
        var grupo = new Grupo { Letra = 'A' };
        context.Grupos.Add(grupo);
        context.SaveChanges();

        var mandante = new Selecao { Codigo = "MEX", Nome = "México", Tecnico = "Javier Aguirre", GrupoId = grupo.Id };
        var visitante = new Selecao { Codigo = "RSA", Nome = "África do Sul", Tecnico = "Hugo Broos", GrupoId = grupo.Id };
        context.Selecoes.AddRange(mandante, visitante);
        context.SaveChanges();

        return new AppDbContextSeedResult(grupo, mandante, visitante);
    }

    private sealed record AppDbContextSeedResult(Grupo Grupo, Selecao Mandante, Selecao Visitante);

    [Fact]
    public void Simular_Nao_Altera_Placar_Oficial_Do_Jogo()
    {
        using var context = _fixture.CreateContext();
        var seed = SeedGrupoESelecoes(context);
        var jogo = NovoJogoComSelecoes(seed);
        context.Jogos.Add(jogo);
        context.SaveChanges();

        var simulacao = new Simulacao { VisitanteId = Guid.NewGuid(), CriadaEm = DateTime.UtcNow, AtualizadaEm = DateTime.UtcNow };
        context.Simulacoes.Add(simulacao);
        context.SaveChanges();

        context.SimulacoesJogos.Add(new SimulacaoJogo
        {
            SimulacaoId = simulacao.Id,
            JogoId = jogo.Id,
            GolsMandante = 3,
            GolsVisitante = 1,
        });
        context.SaveChanges();

        var jogoRecarregado = context.Jogos.Single(j => j.Id == jogo.Id);
        Assert.Null(jogoRecarregado.GolsMandante);
        Assert.Null(jogoRecarregado.GolsVisitante);
    }

    [Fact]
    public void Simulacoes_De_Visitantes_Diferentes_Sao_Isoladas()
    {
        using var context = _fixture.CreateContext();
        var seed = SeedGrupoESelecoes(context);
        var jogo = NovoJogoComSelecoes(seed);
        context.Jogos.Add(jogo);
        context.SaveChanges();

        var simulacaoA = new Simulacao { VisitanteId = Guid.NewGuid(), CriadaEm = DateTime.UtcNow, AtualizadaEm = DateTime.UtcNow };
        var simulacaoB = new Simulacao { VisitanteId = Guid.NewGuid(), CriadaEm = DateTime.UtcNow, AtualizadaEm = DateTime.UtcNow };
        context.Simulacoes.AddRange(simulacaoA, simulacaoB);
        context.SaveChanges();

        context.SimulacoesJogos.Add(new SimulacaoJogo { SimulacaoId = simulacaoA.Id, JogoId = jogo.Id, GolsMandante = 2, GolsVisitante = 0 });
        context.SimulacoesJogos.Add(new SimulacaoJogo { SimulacaoId = simulacaoB.Id, JogoId = jogo.Id, GolsMandante = 1, GolsVisitante = 1 });
        context.SaveChanges();

        var placarA = context.SimulacoesJogos.Single(sj => sj.SimulacaoId == simulacaoA.Id);
        var placarB = context.SimulacoesJogos.Single(sj => sj.SimulacaoId == simulacaoB.Id);

        Assert.Equal((2, 0), (placarA.GolsMandante, placarA.GolsVisitante));
        Assert.Equal((1, 1), (placarB.GolsMandante, placarB.GolsVisitante));
    }

    [Fact]
    public void Rejeita_Placar_Duplicado_Para_O_Mesmo_Jogo_Na_Mesma_Simulacao()
    {
        using var context = _fixture.CreateContext();
        var seed = SeedGrupoESelecoes(context);
        var jogo = NovoJogoComSelecoes(seed);
        context.Jogos.Add(jogo);
        context.SaveChanges();

        var simulacao = new Simulacao { VisitanteId = Guid.NewGuid(), CriadaEm = DateTime.UtcNow, AtualizadaEm = DateTime.UtcNow };
        context.Simulacoes.Add(simulacao);
        context.SaveChanges();

        context.SimulacoesJogos.Add(new SimulacaoJogo { SimulacaoId = simulacao.Id, JogoId = jogo.Id, GolsMandante = 1, GolsVisitante = 0 });
        context.SaveChanges();

        context.SimulacoesJogos.Add(new SimulacaoJogo { SimulacaoId = simulacao.Id, JogoId = jogo.Id, GolsMandante = 2, GolsVisitante = 2 });

        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(31, 0)]
    [InlineData(0, 31)]
    public void Rejeita_Gols_Simulados_Fora_Da_Faixa_0_A_30(int golsMandante, int golsVisitante)
    {
        using var context = _fixture.CreateContext();
        var seed = SeedGrupoESelecoes(context);
        var jogo = NovoJogoComSelecoes(seed);
        context.Jogos.Add(jogo);
        context.SaveChanges();

        var simulacao = new Simulacao { VisitanteId = Guid.NewGuid(), CriadaEm = DateTime.UtcNow, AtualizadaEm = DateTime.UtcNow };
        context.Simulacoes.Add(simulacao);
        context.SaveChanges();

        context.SimulacoesJogos.Add(new SimulacaoJogo
        {
            SimulacaoId = simulacao.Id,
            JogoId = jogo.Id,
            GolsMandante = golsMandante,
            GolsVisitante = golsVisitante,
        });

        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }
}
