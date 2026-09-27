# Proposal

## Why

O PortalCopa26 existe hoje apenas como protótipo estático validado (`D:\Desenvolvimento\IA\copa2026\prototipo`), com os dados embutidos em `js/site.js` e em arquivos-texto em `fontes/`. O PRD (`CLAUDE.md`) define a evolução para Blazor Web App (.NET 10) com EF Core e SQLite. Nenhuma página (Home, Jogos, Grupos, Equipes, Ranking, Simulador) pode ser construída sem antes existir a fundação: solução, projeto, modelo de dados persistido e carga inicial dos dados da Copa.

## What Changes

- Cria a solução `PortalCopa26` em `src/`, com um único projeto de aplicação Blazor Web App (.NET 10, interatividade Server) organizado em pastas que espelham camadas futuras (Domain / Data / Services), sem múltiplos projetos de aplicação.
- Adiciona um projeto de testes automatizados para validar a fundação (modelo, seed e idempotência).
- Configura EF Core com SQLite: `AppDbContext`, configurações de mapeamento, migration inicial e aplicação automática das migrations na inicialização.
- Cria as 7 entidades do escopo: `Grupo`, `Selecao`, `Jogador`, `Jogo`, `RankingFifa`, `Simulacao`, `SimulacaoJogo`.
- `Jogo` passa a cobrir **todas as fases** (104 jogos: 72 da fase de grupos + 32 do mata-mata), com suporte a confrontos ainda indefinidos (ex.: "Venc. Oitavas 1").
- `Simulacao`/`SimulacaoJogo` persistem placares simulados separadamente do placar oficial, vinculados a um identificador anônimo de visitante (não há login).
- Implementa a carga inicial (SeedData) idempotente a partir dos dados do protótipo: 12 grupos, 48 seleções (com técnico), elencos completos, 104 jogos e o ranking FIFA.
- Registra na injeção de dependência os serviços de dados necessários às próximas mudanças.
- Nenhuma página de domínio é implementada; a aplicação apenas sobe com o layout padrão do template.

## Capabilities

### New Capabilities
- `app-foundation`: solução/projeto Blazor Web App executável, organização interna preparada para migração em camadas e serviços de dados disponíveis via injeção de dependência.
- `data-persistence`: modelo de dados da Copa 2026 persistido em SQLite, carga inicial idempotente dos dados oficiais e armazenamento de simulações independente dos resultados oficiais.

### Modified Capabilities
(nenhuma — não há specs existentes)

## Impact

- Código novo em `src/` (solução, projeto `PortalCopa26`, projeto de testes) — o diretório está vazio hoje.
- Dependências novas: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`; xUnit para testes. SDK .NET 10 e `dotnet-ef` 10 já estão instalados na máquina.
- Arquivos de seed derivados do protótipo passam a fazer parte do repositório; o protótipo em si não é alterado.
- O arquivo do banco SQLite é local e não deve ser versionado.
- Sem integração externa: bandeiras/logo via API da FIFA e o gráfico do ranking com Chart.js/JSInterop ficam para as mudanças das páginas.
