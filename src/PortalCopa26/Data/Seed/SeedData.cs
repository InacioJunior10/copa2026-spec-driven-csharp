using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PortalCopa26.Domain.Entities;
using PortalCopa26.Domain.Enums;

namespace PortalCopa26.Data.Seed;

/// <summary>
/// D8: carrega os dados iniciais da Copa 2026 (grupos, seleções, jogadores, jogos e ranking FIFA)
/// a partir dos JSON gerados do protótipo. Roda dentro de uma transação e só insere
/// se o banco ainda estiver vazio (idempotente: reinícios não duplicam dados).
/// </summary>
public static class SeedData
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Grupos.AnyAsync())
        {
            return;
        }

        var seedDir = Path.Combine(AppContext.BaseDirectory, "Data", "Seed");
        var selecoesDto = await ReadJsonAsync<List<SelecaoSeedDto>>(Path.Combine(seedDir, "selecoes.json"));
        var jogosDto = await ReadJsonAsync<List<JogoSeedDto>>(Path.Combine(seedDir, "jogos.json"));
        var rankingDto = await ReadJsonAsync<List<RankingSeedDto>>(Path.Combine(seedDir, "ranking.json"));

        await using var transaction = await context.Database.BeginTransactionAsync();

        var gruposPorLetra = selecoesDto
            .Select(s => s.GrupoLetra)
            .Distinct()
            .OrderBy(letra => letra, StringComparer.Ordinal)
            .ToDictionary(letra => letra, letra => new Grupo { Letra = letra[0] });
        context.Grupos.AddRange(gruposPorLetra.Values);
        await context.SaveChangesAsync();

        var selecoesPorCodigo = new Dictionary<string, Selecao>();
        foreach (var dto in selecoesDto)
        {
            var selecao = new Selecao
            {
                Codigo = dto.Codigo,
                Nome = dto.Nome,
                Tecnico = dto.Tecnico,
                CabecaDeChave = dto.CabecaDeChave,
                GrupoId = gruposPorLetra[dto.GrupoLetra].Id,
                Jogadores = dto.Jogadores.Select(j => new Jogador
                {
                    Nome = j.Nome,
                    Idade = j.Idade,
                    Posicao = Enum.Parse<PosicaoJogador>(j.Posicao),
                    Gols = j.Gols,
                    ParticipacoesCopas = j.ParticipacoesCopas,
                }).ToList(),
            };
            selecoesPorCodigo[dto.Codigo] = selecao;
        }
        context.Selecoes.AddRange(selecoesPorCodigo.Values);
        await context.SaveChangesAsync();

        var jogos = jogosDto.Select(dto => new Jogo
        {
            Numero = dto.Numero,
            Fase = Enum.Parse<FaseJogo>(dto.Fase),
            Rotulo = dto.Rotulo,
            GrupoId = dto.GrupoLetra is not null ? gruposPorLetra[dto.GrupoLetra].Id : null,
            DataHora = dto.DataHora,
            Estadio = dto.Estadio,
            Cidade = dto.Cidade,
            MandanteId = dto.MandanteCodigo is not null ? selecoesPorCodigo[dto.MandanteCodigo].Id : null,
            VisitanteId = dto.VisitanteCodigo is not null ? selecoesPorCodigo[dto.VisitanteCodigo].Id : null,
            VagaMandante = dto.VagaMandante,
            VagaVisitante = dto.VagaVisitante,
        });
        context.Jogos.AddRange(jogos);
        await context.SaveChangesAsync();

        var rankings = rankingDto.Select(dto => new RankingFifa
        {
            Posicao = dto.Posicao,
            CodigoSelecao = dto.CodigoSelecao ?? DeriveCodigoFallback(dto.NomeSelecao),
            NomeSelecao = dto.NomeSelecao,
            Pontos = dto.Pontos,
            SelecaoId = dto.CodigoSelecao is not null && selecoesPorCodigo.TryGetValue(dto.CodigoSelecao, out var selecao)
                ? selecao.Id
                : null,
        });
        context.RankingsFifa.AddRange(rankings);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    private static async Task<T> ReadJsonAsync<T>(string path)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"Arquivo de seed vazio ou inválido: {path}");
    }

    /// <summary>
    /// Para seleções do ranking FIFA que não têm código de origem (fora da Copa 2026
    /// e sem código nas fontes disponíveis): deriva um código de exibição de 3 letras
    /// a partir do nome, sem qualquer pretensão de ser o código oficial da FIFA.
    /// </summary>
    private static string DeriveCodigoFallback(string nome)
    {
        var formD = nome.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && char.IsLetter(c))
            {
                sb.Append(char.ToUpperInvariant(c));
            }
        }

        var letras = sb.ToString();
        return letras.Length >= 3 ? letras[..3] : letras.PadRight(3, 'X');
    }
}
