# Design

## Context

Motivação em `proposal.md`. Estado atual e restrições relevantes:

- `src/` está vazio. SDK .NET 10.0.302 e `dotnet-ef` 10.0.1 estão instalados.
- O protótipo tem **duas fontes de dados divergentes**:
  - `js/site.js`: códigos FIFA de 3 letras, `GRUPOS`, 72 jogos da fase de grupos, `RANKING` (63 itens, com `URU` duplicado e pontuações fora de ordem) e `SELECOES` com 62 seleções — 14 delas fora da Copa (ex.: ITA, JAM, VEN) e muitas com elencos de apenas 4 jogadores. É a única fonte com "participações em Copas".
  - `fontes/*.txt`: `copa2026_selecoes_jogadores.txt` com exatamente as 48 seleções e ~1238 jogadores (`Nome|idade|Posição|gols`, sem Copas); jogos de todas as fases (`jogos_primeira_fase`, `Jogos_Segunda_fase`, `oitavas`, `quartas`, `semifinal`, `terceiro_lugar`, `final`); `ranking_fifa.txt` (98 seleções, pontos com 2 casas decimais); `pais_tecnicos.txt`; `cabecas-chave.txt`; `cidades_sede_estadios.txt`.
  - Os nomes variam entre as fontes (ex.: "Tchéquia"/"República Tcheca", "República da Coreia"/"Coreia do Sul", "Côte d'Ivoire"/"Costa do Marfim", "Holanda"/"Países Baixos", "EUA"/"Estados Unidos", "RD Congo"/"República Democrática do Congo").
  - `fontes/ddd_ajustes_refinamentos1.txt` é de outro projeto e é ignorado.
- `copa2026_fases.txt` diz "total de 102 jogos", mas a soma das fases é 104 (72+16+8+4+2+1+1), valor que também aparece no PRD do protótipo. Vale 104.
- O simulador do protótipo cobre a fase de grupos, aceita placares de 0 a 30 e guarda uma única simulação por navegador (`localStorage`). Não há login (fora do escopo do PRD).

## Goals / Non-Goals

**Goals:**
- Projeto único cuja estrutura permita extrair Domain / Infrastructure / Application sem reescrever lógica.
- Modelo que já suporte as páginas planejadas (jogos de todas as fases, grupos, elencos, ranking, simulações) sem migrations corretivas logo em seguida.
- Seed reprodutível, verificável por contagem e idempotente.

**Non-Goals:**
- Cálculo de classificação (RN-01), melhores terceiros (RN-02) e mata-mata (RN-03) — ficam com Grupos/Simulador.
- Cartões/fair play, prorrogação e pênaltis no modelo.
- Entidade `Estadio` própria (estádio e cidade ficam como texto em `Jogo`; extrair depois se necessário).
- Qualquer página, componente de domínio, Chart.js ou chamadas à API da FIFA.

## Decisions

**D1. Layout da solução**
```
src/
  PortalCopa26.sln
  PortalCopa26/                 ← único projeto de aplicação (Blazor Web App)
    Components/                 ← template Blazor (páginas futuras)
    Domain/Entities/            ← POCOs, sem dependência de EF   → futuro PortalCopa26.Domain
    Domain/Enums/               ← FaseJogo, PosicaoJogador
    Data/AppDbContext.cs        ←                                 → futuro PortalCopa26.Infrastructure
    Data/Configurations/        ← IEntityTypeConfiguration<T> por entidade
    Data/Migrations/
    Data/Seed/                  ← SeedData.cs + arquivos JSON
    Services/                   ← futuros serviços de aplicação    → futuro PortalCopa26.Application
    Extensions/ServiceCollectionExtensions.cs  ← AddPortalCopaData(...)
tests/
  PortalCopa26.Tests/           ← xUnit
```
Alternativa: solução em camadas agora — rejeitada pelo PRD ("projeto único"). O projeto de testes não é uma camada da aplicação e não viola essa restrição. Mapeamento só por Fluent API (nenhum data annotation nas entidades) mantém `Domain/` pronto para virar um projeto sem referência ao EF.

**D2. Blazor Web App com interatividade Server**
Template `dotnet new blazor --interactivity Server` (interatividade por página). O Simulador precisa de interatividade e o gráfico do Ranking precisa de JSInterop, ambos suportados assim. Alternativa: WebAssembly/Auto — rejeitada: exigiria uma API para chegar ao SQLite, o que é complexidade desnecessária para um portal de projeto único.

**D3. `IDbContextFactory<AppDbContext>` em vez de `DbContext` scoped**
Em Blazor Server o escopo de DI dura o circuito inteiro; um `DbContext` scoped seria compartilhado por todas as operações de um usuário (problemas de concorrência e de rastreamento). Registrar `AddDbContextFactory` e criar contextos de vida curta é a recomendação da Microsoft para Blazor Server. Tudo fica encapsulado em `AddPortalCopaData(configuration)`.

**D4. Migrations + `Database.MigrateAsync()` no startup**
Migration inicial `InitialCreate`, aplicada na inicialização. Alternativa `EnsureCreated()` — rejeitada: não versiona o esquema e impede evoluir para as próximas mudanças com migrations.

**D5. Modelo de dados**
- `Grupo`: `Id`, `Letra` (char, única).
- `Selecao`: `Id`, `Codigo` (3 letras, único), `Nome`, `Tecnico`, `CabecaDeChave` (bool), `GrupoId` → `Grupo`.
- `Jogador`: `Id`, `Nome`, `Posicao` (enum `Goleiro|Defensor|MeioCampista|Atacante`), `Idade`, `Gols`, `ParticipacoesCopas` (int?, nulo = desconhecido), `SelecaoId`.
- `Jogo`: `Id`, `Numero` (1–104, único, numeração interna por ordem cronológica), `Fase` (enum `Grupos|SegundaFase|Oitavas|Quartas|Semifinal|TerceiroLugar|Final`), `Rotulo` (ex.: "Oitavas 1", para resolver referências "Venc. Oitavas 1"), `GrupoId?`, `DataHora` (DateTime, horário de Brasília), `Estadio`, `Cidade`, `MandanteId?`, `VisitanteId?`, `VagaMandante?`, `VagaVisitante?` (texto da vaga quando indefinida), `GolsMandante?`, `GolsVisitante?` (placar oficial).
  - Dois FKs para `Selecao` com `DeleteBehavior.Restrict` (evita múltiplos caminhos de cascata).
  - Alternativa: `Jogo` só com a fase de grupos (como no `site.js`) — rejeitada: a página Jogos exibe "todos os jogos da Copa" e a inclusão depois exigiria migration e seed corretivos.
- `RankingFifa`: `Id`, `Posicao`, `CodigoSelecao`, `NomeSelecao`, `Pontos` (double), `SelecaoId?` (preenchido quando a seleção está na Copa).
  - FK opcional porque o ranking inclui seleções fora da Copa (ex.: Itália).
  - `double` e não `decimal`: o provider SQLite do EF não ordena/compara `decimal` no banco, e a ordenação por pontos é necessária para o Ranking/Chart.js.
- `Simulacao`: `Id`, `VisitanteId` (Guid, indexado), `CriadaEm`, `AtualizadaEm`.
- `SimulacaoJogo`: `Id`, `SimulacaoId` (cascade), `JogoId` (restrict), `GolsMandante`, `GolsVisitante`; índice único (`SimulacaoId`, `JogoId`); `CHECK` 0–30 nos gols.
  - `VisitanteId` substitui o `localStorage` do protótipo: a mudança do Simulador decidirá como emiti-lo (ex.: cookie). Alternativa: uma simulação global única — rejeitada: todos os visitantes sobrescreveriam os placares uns dos outros.

**D6. Fonte de verdade do seed**
- Seleções, grupos, cabeças de chave, técnicos, elencos, jogos de todas as fases e ranking: `fontes/*.txt` (completas e restritas às 48 seleções).
- Códigos FIFA e `ParticipacoesCopas`: `js/site.js`, casados por código/nome; jogador sem correspondência fica com `ParticipacoesCopas = null`.
- Estádio/cidade: nomes oficiais de `cidades_sede_estadios.txt` (ex.: "Estadio Akron"), a partir dos nomes genéricos de `jogos_primeira_fase.txt` (ex.: "Estádio de Guadalajara").
- Alternativa: seed só do `site.js` — rejeitada: elencos incompletos, seleções fora da Copa e ranking inconsistente.

**D7. Formato do seed: JSON embarcado + `SeedData` em runtime**
Converter uma única vez as fontes em `Data/Seed/*.json` (`selecoes.json` com grupo/técnico/jogadores, `jogos.json`, `ranking.json`), versionados e lidos por `SeedData` com `System.Text.Json`. Um dicionário explícito nome→código FIFA resolve as variações de nome durante a conversão e falha em nome não mapeado. Alternativas: `HasData()` em migration — rejeitada (~1300 registros com relações tornariam a migration enorme e acoplada ao seed); literais C# — rejeitada (arquivo gigante e difícil de revisar). O script de conversão fica fora da aplicação; o JSON é o artefato revisado.

**D8. Idempotência**
`SeedData.SeedAsync(context)` roda após `MigrateAsync`, dentro de uma transação, e só insere se `Grupos` estiver vazio (a carga é tudo-ou-nada, então uma tabela sentinela basta). Não sobrescreve dados existentes.

**D9. Configuração**
`ConnectionStrings:PortalCopa26 = "Data Source=portalcopa26.db"` em `appsettings.json`, relativo ao content root; `*.db`, `*.db-shm` e `*.db-wal` no `.gitignore`.

**D10. Testes**
xUnit com SQLite in-memory (`DataSource=:memory:` com conexão mantida aberta), para exercitar as mesmas constraints (FK, índice único, CHECK) do banco real. Alternativa: provider InMemory do EF — rejeitado: não aplica constraints relacionais.

## Risks / Trade-offs

- [Divergência de nomes entre fontes gera seleção/jogador sem casamento] → Dicionário explícito nome→código que falha na conversão para nome desconhecido; testes validam 48 seleções, 4 por grupo e jogos da fase de grupos entre seleções do mesmo grupo.
- [`ParticipacoesCopas` desconhecido para a maioria dos jogadores] → Campo nulo e exibido como "—" nas páginas futuras; completar depois por atualização do JSON de seed (ver Open Questions).
- [Horários em horário de Brasília sem fuso armazenado] → Convenção documentada na entidade; aceitável porque o público é brasileiro. Converter para UTC exigiria migration se o portal passar a exibir outros fusos.
- [Numeração de jogo interna, não a oficial da FIFA] → `Numero` é apenas uma chave estável de ordenação/seed; as fontes não trazem a numeração oficial.
- [Seed em runtime: alterar o JSON não atualiza um banco já populado] → Aceitável em desenvolvimento: para recarregar, apagar o arquivo `.db` (passo documentado).
- [Separação só por pastas pode se degradar] → Teste/revisão garante que `Domain/` não referencia `Microsoft.EntityFrameworkCore`.

## Migration Plan

Projeto novo, sem dados em produção. Rollback = reverter o commit e apagar `portalcopa26.db`.

## Open Questions

- Fonte para completar "participações em Copas" dos jogadores que não estão no `site.js`. Não altera modelo nem tarefas: é só atualizar o JSON de seed depois.
