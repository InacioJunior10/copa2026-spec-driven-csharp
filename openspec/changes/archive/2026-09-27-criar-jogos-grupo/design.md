# Design

## Context

Ver `proposal.md` - Why. Estado atual relevante:

- `data-persistence` já expõe `Jogo` (com `Fase`, `GrupoId`/`Grupo`, `DataHora`, `Estadio`, `Cidade`, `Mandante`/`Visitante`, `VagaMandante`/`VagaVisitante`, `GolsMandante`/`GolsVisitante`) e `Grupo` (com `Letra` e suas `Selecoes`), acessados via `IDbContextFactory<AppDbContext>` (D3 de `app-foundation`).
- A Landing Page já tem um padrão de agrupamento por dia em memória (`LandingPageService.GetProximosJogosAsync`, D3 do design de `landing-page`): materializa a query com `ToListAsync()` antes de agrupar por `DataHora.Date`, porque o provider SQLite do EF Core não garante suporte a `GroupBy` sobre `DateTime.Date` traduzido para SQL. Esta mudança lista todos os 104 jogos (não só os sem placar), então o mesmo cuidado se aplica, mas a volumetria continua pequena — materializar tudo em memória antes de agrupar/filtrar não é um problema de performance.
- `ProximosJogos.razor` já renderiza cards de jogo com as classes vendorizadas do protótipo em `wwwroot/css/site.css` (`.game-card`, `.team-side`, `.game-center`, `.badge-group`, `.day-group`, `.day-label`, `.flag-sm`), e `site.css` já define `.filters`/`.filter-btn` (não usados ainda por nenhum componente).
- `NavBar.razor` já linka `/jogos`; a rota simplesmente não existe hoje (cai em `NotFound`).
- Não existe entidade `Estadio`; estádio e cidade são campos texto em `Jogo` (mesma decisão já registrada em `landing-page`/D4).

## Goals / Non-Goals

**Goals:**
- Página `/jogos` funcional, com dados 100% vindos do banco semeado (nenhum dado fictício), fiel ao layout do protótipo já vendorizado.
- Reaproveitar o padrão já estabelecido em `landing-page`: nenhum componente Razor acessa `AppDbContext` diretamente, toda leitura passa por um serviço.
- Componentes pequenos e reutilizáveis (`JogosFiltro`, `JogosDataHeader`, `JogoCard`) que a futura página Grupos ou outras páginas possam reaproveitar quando fizer sentido (ex.: `JogoCard` para mostrar um jogo em outro contexto).

**Non-Goals:**
- Implementar a página Grupos em si — o botão "Ver Grupos" aponta para uma rota que ainda não existe (mesmo padrão aceito para `/simulador` em `landing-page`/D8).
- Qualquer edição, simulação ou alteração de placar (fora do escopo do PRD para esta página; é responsabilidade do Simulador).
- Paginação ou lazy loading — 104 jogos é um volume pequeno o suficiente para renderizar de uma vez.

## Decisions

**D1. Serviço dedicado `IJogosService`/`JogosService`, em vez de estender `ILandingPageService`**
Cria `Services/IJogosService` + `JogosService`, registrados via `AddJogosServices(this IServiceCollection)` em `Extensions/`, chamado a partir de `Program.cs` ao lado de `AddLandingPageServices`. Expõe `GetJogosAgrupadosPorDiaAsync(char? grupo = null)`, retornando os dias (com contagem de jogos) e, para cada dia, os jogos ordenados por hora; o DTO de cada jogo (ex.: `JogoItem`, análogo a `ProximoJogoItem` de `landing-page`) SHALL incluir `GolsMandante`/`GolsVisitante` (nulos quando o jogo ainda não tem placar oficial), além de mandante/visitante ou vaga, grupo, estádio, cidade e data/hora — são esses campos que `JogoCard` (D5) consome. Alternativa considerada: adicionar esse método a `ILandingPageService` — rejeitada porque `ILandingPageService` é conceitualmente escopado à Landing Page (seu nome e seus outros métodos já refletem isso); a página Jogos é uma tela própria e merece seu próprio contrato de dados, seguindo o mesmo princípio de "um serviço por página de consumo" já registrado em `landing-page`/D2.

**D2. Filtro por grupo aplicado no servidor, via parâmetro de rota (query string)**
`GetJogosAgrupadosPorDiaAsync(char? grupo)` filtra no banco (antes de materializar) por `j.Fase == FaseJogo.Grupos && j.Grupo!.Letra == grupo.Value` quando `grupo` é informado; sem filtro, retorna todos os jogos de todas as fases. `Jogos.razor` declara `[SupplyParameterFromQuery(Name = "grupo")] public string? Grupo { get; set; }` (o binding de query do Blazor não suporta `char?` diretamente — apenas `string`, `bool`, `DateTime`, `decimal`, `double`, `float`, `Guid`, `int`, `long` e seus `Nullable<T>`) e converte para `char?` internamente (primeiro caractere, maiúsculo) antes de recarregar os jogos agrupados em `OnParametersSetAsync`, chamando `GetJogosAgrupadosPorDiaAsync(grupo convertido)` sempre que o parâmetro de query mudar. A seleção do filtro é, portanto, uma navegação (`<a href="/jogos?grupo=C">`), não um evento de componente. Alternativa considerada (rejeitada, ver D6): `JogosFiltro` disparando `EventCallback<char?>` para o pai atualizar estado local — essa abordagem exige um modo de renderização interativo (Server/WebAssembly/Auto) para que o clique do botão dispare código C#, o que contradiz a decisão de manter a página em renderização estática (D6); a navegação por query string funciona em SSR estático via navegação aprimorada do Blazor, sem exigir interatividade, e como bônus deixa a URL do filtro compartilhável/com bookmark.

**D3. Grupos do filtro vêm do banco (`Grupos.Select(g => g.Letra)`), não de uma lista fixa A–L**
`IJogosService` expõe também `GetLetrasGruposAsync()`, consultando as letras de grupo persistidas, para popular `JogosFiltro`. Alternativa considerada: hardcodar `['A'..'L']` no componente — rejeitada porque o PRD/CLAUDE.md exige usar exclusivamente os dados oficiais do Seed; uma lista fixa poderia divergir dos grupos realmente cadastrados.

**D4. Agrupamento por dia em memória, mesmo padrão de `landing-page`/D3**
Assim como `GetProximosJogosAsync`, `GetJogosAgrupadosPorDiaAsync` materializa a query filtrada e ordenada por `DataHora` com `ToListAsync()` antes de agrupar por `DataHora.Date` em memória. Sem essa materialização, o `GroupBy` correria risco de não traduzir no provider SQLite do EF Core. Diferente da Landing Page, aqui **todos** os dias com jogo (não só os 2 mais próximos) são retornados, já que a página Jogos é a listagem completa.

**D5. Componentes por responsabilidade, seguindo o padrão de composição de `landing-page`/D1**
- `Jogos.razor` (`@page "/jogos"`): injeta `IJogosService`, lê o grupo filtrado via `[SupplyParameterFromQuery(Name = "grupo")]` (D2), compõe `JogosFiltro` + um `JogosDataHeader`/lista de `JogoCard` por dia, e o botão "Ver Grupos".
- `JogosFiltro.razor`: recebe a lista de letras de grupo e o grupo atualmente selecionado (via `[Parameter]`, não evento), e renderiza um link `<a>` "Todos" (`href="/jogos"`) e um `<a>` por grupo (`href="/jogos?grupo={letra}"`), marcando o link correspondente ao grupo atual com a classe `active`; reaproveita `.filters`/`.filter-btn` já definidos em `site.css`. Não dispara `EventCallback` — a troca de filtro é uma navegação comum (ver D2).
- `JogosDataHeader.razor`: recebe uma data e a quantidade de jogos daquele dia, reaproveitando `.day-label` de `site.css` (mesmo visual do cabeçalho de dia já usado por `ProximosJogos.razor`, mas extraído como componente próprio para reuso nesta página com N dias, não só 2).
- `JogoCard.razor`: recebe o DTO de jogo definido em D1 (mandante/visitante ou vaga, grupo, estádio, cidade, hora, `GolsMandante`/`GolsVisitante`) e renderiza o card, reaproveitando `.game-card`/`.team-side`/`.game-center`/`.badge-group`/`.flag-sm` de `site.css`; ao contrário do card inline de `ProximosJogos.razor`, mostra o placar oficial quando `GolsMandante`/`GolsVisitante` não forem nulos, em vez de sempre "— : —".
Alternativa considerada: replicar o card inline como em `ProximosJogos.razor` em vez de extrair `JogoCard.razor` — rejeitada porque o pedido explicitamente lista `JogoCard.razor` como componente próprio, e a página Jogos precisa mostrar placar oficial (diferente do card da Landing Page, que só mostra jogos sem placar).

**D6. Sem `@rendermode InteractiveServer` nesta página**
A página inteira (listagem, agrupamento por dia e filtro por grupo) é resolvida por renderização estática (SSR) do Blazor Web App: o filtro é uma navegação normal por link com query string (D2), que a navegação aprimorada (enhanced navigation) do Blazor re-executa sem recarregar a página inteira, sem exigir nenhum modo de renderização interativo. Isso é diferente de um evento de clique tratado em C# (`@onclick`), que exigiria `@rendermode` interativo em algum ancestral da árvore de componentes — mecanismo que esta página não usa. Diferente de `RankingFifaChart` (que precisa de `IJSRuntime` e por isso declara `@rendermode InteractiveServer`), a página Jogos não tem nenhuma necessidade de estado client-side ou JS interop. Alternativa considerada: `InteractiveServer` com filtro por `EventCallback` — descartada em favor de D2 por não ser necessária para atender aos requisitos da spec e por manter a página mais simples (sem circuito interativo, URL do filtro compartilhável).

## Risks / Trade-offs

- [`GroupBy` sobre `DataHora.Date` pode não traduzir para SQL no provider SQLite do EF Core] → Mitigado como em `landing-page`/D3: agrupamento em memória após materializar a lista filtrada e ordenada.
- [Filtrar por grupo exclui jogos de mata-mata mesmo quando o usuário só queria "ver mais um grupo"] → Coberto explicitamente pela spec (`jogos-page` - Requirement "Filtro por grupo"): é o comportamento pedido (grupo é um conceito só da fase de grupos).
- [Renderizar os 104 jogos de uma vez sem paginação] → Aceitável nesta fase (mesma ordem de grandeza que a Landing Page já lida bem); pode ser revisitado se a volumetria crescer (torneios futuros/expansão de fases).
- [Botão "Ver Grupos" aponta para rota inexistente] → Mesmo padrão já aceito em `landing-page`/D8 para o link do Simulador; explícito e documentado, não um bug.
- [Todos os 104 jogos semeados hoje estão sem placar oficial — ver Context] → O cenário "jogo já disputado exibe o placar oficial" (spec `jogos-page`) não é observável rodando a aplicação com o banco semeado real. É verificado por teste automatizado de `IJogosService`/`JogosService` com placar definido manualmente em memória (mesmo padrão de `LandingPageServiceTests.GetProximosJogosAsync_Retorna_Vazio_Quando_Todos_Os_Jogos_Tem_Placar_Oficial`), não por inspeção visual com dados reais.
- [Sem `bunit` no projeto de testes hoje] → Esta mudança não introduz teste automatizado de componente Razor (`.razor`), assim como `landing-page` não introduziu; a cobertura de comportamento fica em `IJogosService`/`JogosService` (regras de filtro, agrupamento e placar) e a composição visual é verificada manualmente rodando a aplicação. Adotar `bunit` fica como evolução futura, não escopo desta change.

## Migration Plan

Não há alteração de schema nem dado em produção. Mudança aditiva (novos componentes e serviço); nenhuma página ou serviço existente é removido ou tem comportamento alterado.
