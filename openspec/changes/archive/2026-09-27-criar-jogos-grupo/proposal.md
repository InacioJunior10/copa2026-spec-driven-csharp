# Proposal

## Why

O PRD (seção 6) define a página Jogos como um dos 6 itens da navegação principal, mas o portal hoje só tem a Landing Page — não há como o usuário consultar a lista completa dos 104 jogos da Copa 2026. A Home já exibe uma prévia (2 dias de jogos sem placar), mas link "Ver todos" em `ProximosJogos.razor` aponta para `/jogos`, uma rota que ainda não existe (cai em `NotFound`). Esta mudança implementa essa página, consumindo exclusivamente os dados já persistidos por `data-persistence`.

## What Changes

- Cria a rota `/jogos` com 4 componentes novos em `Components/Pages/Jogos/`: `Jogos.razor` (página/rota), `JogosFiltro.razor` (filtro por grupo), `JogosDataHeader.razor` (cabeçalho de cada dia agrupado) e `JogoCard.razor` (card de um jogo).
- Lista todos os 104 jogos cadastrados (todas as fases), ordenados por data e hora, agrupados por dia.
- Filtro por grupo (A–L): ao selecionar um grupo, exibe somente os jogos da fase de grupos daquele grupo; jogos de mata-mata (sem grupo) só aparecem quando nenhum filtro está selecionado.
- Cada jogo exibido mostra seleção mandante e visitante (ou a vaga, quando ainda não definidas), grupo (quando aplicável), estádio, cidade, data e hora, e o placar oficial quando existir.
- Adiciona um botão "Ver Grupos" na página, apontando para `/grupos` (rota também ainda não implementada, fora do escopo desta mudança — mesmo padrão já aceito em `SimuladorPainel` apontando para `/simulador`).
- Estende `ILandingPageService`/`LandingPageService` (ou cria um serviço de dados equivalente dedicado a Jogos) para expor a listagem completa agrupada por dia e o filtro por grupo, sem que nenhum componente Razor acesse `AppDbContext` diretamente.
- Usa exclusivamente os dados oficiais carregados via Seed (nenhum dado fictício é introduzido por esta mudança).

## Capabilities

### New Capabilities
- `jogos-page`: comportamento observável da página Jogos — o que é listado, como é agrupado/ordenado, como o filtro por grupo se comporta e o que cada card de jogo exibe.

### Modified Capabilities
(nenhuma — `app-foundation` e `data-persistence` já expõem tudo que esta mudança consome; nenhum requisito existente muda de comportamento)

## Impact

- Novo código: `Components/Pages/Jogos/*.razor` (`Jogos`, `JogosFiltro`, `JogosDataHeader`, `JogoCard`), e um serviço de dados (novo ou extensão do existente em `Services/`) para a listagem/filtro de jogos.
- `Components/Layout/NavBar.razor` já linka `/jogos` — nenhuma mudança de navegação necessária.
- `ProximosJogos.razor` (Landing Page) já linka "Ver todos" para `/jogos` — passa a resolver de fato.
- Consome exclusivamente dados já persistidos por `data-persistence` (nenhuma migration ou mudança de schema).
- Reaproveita os estilos já vendorizados do protótipo em `wwwroot/css/site.css` (`.game-card`, `.day-group`, `.badge-group`, `.filters`/`.filter-btn`, etc.), já usados por `ProximosJogos.razor`.
- Fora do escopo: a página Grupos em si (o botão "Ver Grupos" aponta para uma rota ainda não implementada), edição/simulação de placares, e qualquer outra página da navegação principal.
