namespace PortalCopa26.Data.Seed;

internal sealed record SelecaoSeedDto(
    string Codigo,
    string Nome,
    string GrupoLetra,
    string Tecnico,
    bool CabecaDeChave,
    List<JogadorSeedDto> Jogadores);

internal sealed record JogadorSeedDto(
    string Nome,
    int Idade,
    string Posicao,
    int Gols,
    int? ParticipacoesCopas);

internal sealed record JogoSeedDto(
    int Numero,
    string Fase,
    string Rotulo,
    string? GrupoLetra,
    DateTime DataHora,
    string Estadio,
    string Cidade,
    string? MandanteCodigo,
    string? VisitanteCodigo,
    string? VagaMandante,
    string? VagaVisitante);

internal sealed record RankingSeedDto(
    int Posicao,
    string? CodigoSelecao,
    string NomeSelecao,
    double Pontos);
