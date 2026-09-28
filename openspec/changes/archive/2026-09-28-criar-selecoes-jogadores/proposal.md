# Proposal

## Why

O PortalCopa26 já persiste seleções, elencos, técnicos e Ranking FIFA (capacidade `data-persistence`), mas ainda não existe nenhuma página para consultá-los. O menu principal (`NavBar`) já linka para `/equipes`, porém a rota não existe. O protótipo `equipes.html` já valida o layout e o comportamento esperados: grade de 48 seleções com busca e filtro por grupo, e um modal com o elenco convocado ao selecionar uma seleção.

## What Changes

- Cria a página Equipes (`/equipes`), acessível pelo menu principal, exibindo as 48 seleções da Copa 2026 em cartões com bandeira, nome, grupo e código FIFA.
- Adiciona busca por nome/código da seleção e filtro por grupo (Todos, Grp A–Grp L), ambos client-side e combináveis, sem recarregar a página.
- Ao selecionar uma seleção, abre um modal com bandeira, nome, grupo, técnico, posição no Ranking FIFA (quando a seleção estiver ranqueada) e a lista de jogadores convocados (nome, posição, idade, gols pela seleção e participações em Copas, quando conhecidas).
- Introduz `SelecaoService`/`ISelecaoService` como único ponto de acesso a `Selecao`, `Jogador` e `RankingFifa` para esta página, sem acesso direto ao `AppDbContext` a partir de componentes Razor.
- Reaproveita as classes CSS já existentes em `wwwroot/css/site.css` (`team-grid`, `team-card`, `modal-overlay`, `filter-btn`, `search-bar`, `pos`) e o padrão visual das páginas Grupos/Jogos.

## Capabilities

### New Capabilities
- `equipes-page`: comportamento observável da página Equipes — listagem de seleções, busca, filtro por grupo e modal de elenco.

### Modified Capabilities
_Nenhuma. O modelo de dados (`Selecao`, `Jogador`, `RankingFifa`) e a carga inicial já estão cobertos por `data-persistence` e não mudam._

## Impact

- Novo: `Services/ISelecaoService.cs`, `Services/SelecaoService.cs`.
- Novo: `Components/Pages/Equipes/Equipes.razor` e componentes de apoio (`CartaoTime.razor`, `FiltroTime.razor`, `ModalTime.razor`, `TabelaElencoTime.razor`).
- Registro do novo serviço em `Program.cs` (padrão dos demais `I*Service`).
- Nenhuma migração de banco: entidades e dados já existem via `data-persistence`.
- Assunção registrada em `design.md`: o critério de aceitação "26 jogadores por seleção" é tratado como "exibir o elenco completo de cada seleção, conforme a fonte oficial `copa2026_selecoes_jogadores.txt`" — a fonte tem elencos com 22 a 26 jogadores por seleção, e a página SHALL exibir a quantidade real de cada uma, sem completar com dados fictícios (proibido por CLAUDE.md).
