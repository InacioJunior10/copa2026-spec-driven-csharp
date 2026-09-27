# Design

## Context

Ver `proposal.md` - Why. Estado atual relevante:

- O template Blazor já referencia Bootstrap 5 vendorizado em `wwwroot/lib/bootstrap` (`App.razor`) e usa interatividade Server **por página** (`@rendermode InteractiveServer`), não globalmente — só `Counter.razor` está marcada assim hoje.
- O acesso a dados segue `IDbContextFactory<AppDbContext>` (D3 da fundação); nenhum serviço de leitura específico de página existe ainda, só o `AppDbContext` e o `SeedData`.
- O protótipo (`index.html` + `site.css` + `site.js`) usa um tema escuro próprio (tokens em `:root`, ex.: `--bg`, `--card`, `--gold`), **não** Bootstrap, e monta o gráfico de ranking com Chart.js (`chart.umd.min.js` via CDN, versão 4.4.4) desenhando um `bar` horizontal com cores especiais para os 3 primeiros.
- Todos os jogos semeados estão sem placar oficial (`GolsMandante`/`GolsVisitante` nulos) — a Copa de 2026 ainda não ocorreu na linha do tempo dos dados.
- Não existe entidade `Estadio`; o nome do estádio é um campo texto em `Jogo`.

## Goals / Non-Goals

**Goals:**
- Landing Page funcional com dados reais do banco, fiel ao layout/tema visual do protótipo, mas construída com Bootstrap 5 + componentes Blazor.
- Nenhum componente Razor acessa `AppDbContext` diretamente — toda leitura passa por um serviço.
- Wrapper de interoperabilidade com Chart.js reutilizável por outros gráficos futuros do portal, não amarrado ao Ranking.

**Non-Goals:**
- Qualquer outra página (Jogos, Grupos, Equipes, Ranking completo, Simulador).
- Navegação avançada (menu mobile, active-link) ou responsividade além do grid do Bootstrap.
- Countdown regressivo até o primeiro jogo (citado no PRD para a Home, mas fora do escopo desta mudança, que já delimita as 6 seções acima).

## Decisions

**D1. Composição por componentes, um por seção, sob `Components/Pages/LandingPage/`**
`Home.razor` (rota `/`) só compõe `<HeroSection />`, `<EstatisticasCopa />`, `<ProximosJogos />`, `<RankingFifaChart />` e `<SimuladorPainel />`, cada um em seu próprio arquivo `.razor` (+ `.razor.css` quando precisar de estilo próprio, seguindo o padrão já usado por `MainLayout.razor.css`). Alternativa considerada: uma única página com tudo inline — rejeitada porque o pedido exige componentes reutilizáveis e testáveis isoladamente, e porque `RankingFifaChart` precisa de um ciclo de vida próprio (`OnAfterRenderAsync` para o JS interop) que não deve acoplar ao resto da página.
`HeroSection` inclui o logo da Copa (`https://api.fifa.com/api/v3/picture/tournaments-sq-4/285023`, mesma URL do protótipo e do PRD seção 11), o nome do portal, as duas chamadas (Simulador/Jogos) e os 3 países-sede — todos estáticos, sem consulta ao banco.

**D2. Camada de serviços dedicada à Landing Page**
Cria `Services/ILandingPageService` + `LandingPageService`, registrados via `AddLandingPageServices(this IServiceCollection)` em `Extensions/`, chamado a partir de `Program.cs` ao lado de `AddPortalCopaData`. O serviço recebe `IDbContextFactory<AppDbContext>` e expõe métodos por seção: `GetEstatisticasAsync()`, `GetProximosJogosAsync()`, `GetTopRankingAsync(int quantidade = 10)`. Alternativa considerada: um serviço genérico de "repositório" por entidade — rejeitada por ser mais genérica do que o necessário agora; um serviço por página de consumo é suficiente e mais simples, e pode ser dividido depois se outra página precisar de parte dele.

**D3. "Próximos jogos" definido por ausência de placar oficial, não por relógio do sistema**
Um jogo é candidato a "próximo" quando `GolsMandante`/`GolsVisitante` são nulos (independente da data atual do servidor). O serviço busca esses jogos ordenados por `DataHora` e materializa o resultado em memória (`ToListAsync()`) **antes** de agrupar por `DataHora.Date` e selecionar os 2 primeiros grupos (dias) — o agrupamento por data SHALL ocorrer em memória (LINQ-to-Objects), não traduzido para SQL, porque o provider SQLite do EF Core não tem suporte garantido para `GroupBy` sobre `DateTime.Date` (pode lançar erro de tradução em runtime dependendo da versão). Como a quantidade de jogos sem placar é pequena (no máximo 104), materializar antes de agrupar não é um problema de performance. Alternativa considerada: filtrar por `DataHora >= DateTime.Now` (como o protótipo faz com uma data fixa `'2026-06-05'`) — rejeitada porque o relógio real do servidor pode estar fora da janela de 11/06–19/07/2026 durante o desenvolvimento e testes, deixando a seção sempre vazia; a ausência de placar é um proxy estável e correto enquanto não há resultados reais.

**D4. Estatística de estádios via `SELECT DISTINCT Jogo.Estadio`**
Sem entidade `Estadio`, a contagem de estádios da seção de estatísticas é `Jogos.Select(j => j.Estadio).Distinct().Count()`. Alternativa considerada: criar uma entidade `Estadio` agora — rejeitada como fora de escopo; o PRD não pede isso nesta mudança e a fundação já modelou estádio como texto em `Jogo` deliberadamente.

**D5. Wrapper de interoperabilidade Chart.js reutilizável**
Um módulo JS (`wwwroot/js/charts.js`) expõe uma função genérica, por exemplo `renderBarChart(canvasId, options)`, chamada via `IJSRuntime` por um serviço C# fino (`Services/Charts/IChartInterop`/`ChartInterop`) que qualquer componente de gráfico futuro pode injetar — `RankingFifaChart` é o primeiro consumidor, não o único. Fica em uma subpasta própria (`Services/Charts/`) e não junto de `ILandingPageService` na raiz de `Services/`, porque é interoperabilidade de apresentação (JS interop), não acesso a dados — misturar as duas coisas na mesma pasta tende a virar um "bagunceiro" conforme mais serviços forem adicionados. `RankingFifaChart.razor` SHALL declarar `@rendermode InteractiveServer` (necessário para `IJSRuntime` funcionar fora da renderização estática) e criar/atualizar o gráfico em `OnAfterRenderAsync`. Alternativa considerada: JS específico do Ranking (`renderRankingChart`) — rejeitada por não ser reutilizável, indo contra o requisito explícito de reuso futuro.

**D6. Chart.js vendorizado localmente, no mesmo padrão do Bootstrap**
Baixar o build UMD do Chart.js (mesma versão usada no protótipo, 4.4.4) a partir de `https://cdn.jsdelivr.net/npm/chart.js@4.4.4/dist/chart.umd.min.js` (a mesma URL que o protótipo usa como CDN) e salvar em `wwwroot/lib/chartjs/chart.umd.min.js`, versionado no repositório (não gerado em build), no mesmo padrão do Bootstrap já vendorizado pelo template. Referenciá-lo em `App.razor`, em vez de carregar via CDN em runtime. Alternativa considerada: `<script src="https://cdn.jsdelivr.net/...">` como no protótipo — rejeitada porque o restante do projeto (Bootstrap) já vendoriza dependências de front-end localmente; depender de CDN introduziria uma falha de rede não presente em nenhuma outra parte do app.

**D7. Estilo: Bootstrap 5 para grid/layout + tokens de tema do protótipo em CSS próprio**
Usa classes utilitárias e grid do Bootstrap para estrutura (`container`, `row`, `col-*`, `card`, `btn`), e recria só os tokens de cor essenciais do protótipo (fundo escuro, dourado de destaque) em `wwwroot/app.css` (já referenciado em `App.razor`), como uma pequena sobreposição de tema — não um port 1:1 de `site.css`. Alternativa considerada: portar `site.css` inteiro — rejeitada porque contradiz o requisito explícito de "Utilizar Bootstrap 5"; o requisito é usar Bootstrap como base, com o protótipo como referência visual, não substituí-lo pelo CSS do protótipo.

**D8. Link do Simulador aponta para uma rota que ainda não existe**
`SimuladorPainel` linka para `/simulador`; como a página não existe nesta mudança, a navegação cai na página `NotFound` já configurada no `Router`. Isso é aceitável e explícito: implementar o Simulador está fora do escopo desta mudança.

## Risks / Trade-offs

- [`RankingFifa.CodigoSelecao` pode ser um código de fallback (3 letras derivadas do nome, não um código FIFA real) para seleções fora da Copa — ver `SeedData.DeriveCodigoFallback`] → Hoje as 10 primeiras posições do ranking são todas seleções da Copa com código real, então a bandeira do Top 10 sempre resolve corretamente; se uma atualização futura do ranking colocar uma seleção "fallback" no Top 10, a URL de bandeira montada a partir desse código pode não corresponder a uma bandeira real na API da FIFA. Risco baixo e não bloqueador nesta mudança; documentado aqui para não ser esquecido caso o ranking seja atualizado depois.
- [`GroupBy` sobre `DataHora.Date` pode não traduzir para SQL no provider SQLite do EF Core] → Mitigado pela D3: o agrupamento por dia é feito em memória, após materializar a lista de jogos sem placar, não como parte da consulta traduzida para SQL.
- [O critério "2 dias mais próximos" pode retornar 0 jogos se todo o torneio já tiver placar oficial em algum momento futuro do projeto] → Coberto pela spec (Scenario "Nenhum jogo pendente"): a seção mostra uma mensagem, não erro.
- [Recriar só os tokens de tema (não o `site.css` inteiro) pode deixar a página visualmente mais próxima do Bootstrap padrão do que do protótipo] → Aceitável dado o requisito explícito de "Utilizar Bootstrap 5"; o objetivo é a mesma identidade (fundo escuro, dourado), não pixel-perfect.
- [Vendorizar o Chart.js manualmente (sem gerenciador de pacotes front-end) pode ficar desatualizado] → Mesmo padrão de risco que o Bootstrap já vendorizado pelo template; aceitável nesta fase do projeto.
- [`RankingFifaChart` exige `InteractiveServer` numa página majoritariamente estática] → Delimitado ao componente do gráfico, não à página inteira, minimizando o custo de manter um circuito Server ativo.

## Migration Plan

Não há dado em produção nem alteração de schema. Nada a migrar; a mudança é aditiva (novos arquivos de componente, serviço e assets estáticos) e substitui apenas o conteúdo de `Home.razor`.
