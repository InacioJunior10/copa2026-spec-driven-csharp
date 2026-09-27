using PortalCopa26.Domain.Enums;

namespace PortalCopa26.Domain.Entities;

public class Jogo
{
    public int Id { get; set; }

    /// <summary>Numeração interna por ordem cronológica (1–104); não é a numeração oficial da FIFA.</summary>
    public int Numero { get; set; }

    public FaseJogo Fase { get; set; }

    /// <summary>Rótulo da fase/confronto (ex.: "Oitavas 1"), usado para resolver referências como "Venc. Oitavas 1".</summary>
    public string Rotulo { get; set; } = string.Empty;

    /// <summary>Preenchido apenas na fase de grupos.</summary>
    public int? GrupoId { get; set; }
    public Grupo? Grupo { get; set; }

    /// <summary>Data e hora no horário de Brasília.</summary>
    public DateTime DataHora { get; set; }

    public string Estadio { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;

    /// <summary>Nulo enquanto o confronto do mata-mata não está definido.</summary>
    public int? MandanteId { get; set; }
    public Selecao? Mandante { get; set; }

    public int? VisitanteId { get; set; }
    public Selecao? Visitante { get; set; }

    /// <summary>Texto da vaga quando o mandante ainda não está definido (ex.: "Venc. Oitavas 1").</summary>
    public string? VagaMandante { get; set; }

    /// <summary>Texto da vaga quando o visitante ainda não está definido.</summary>
    public string? VagaVisitante { get; set; }

    /// <summary>Placar oficial; nulo até o jogo ser disputado.</summary>
    public int? GolsMandante { get; set; }
    public int? GolsVisitante { get; set; }
}
