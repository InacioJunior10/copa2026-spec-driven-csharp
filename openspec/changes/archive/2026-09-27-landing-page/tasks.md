# Tasks

## 1. Infraestrutura de gráficos reutilizável (Chart.js + JSInterop)

- [x] 1.1 Baixar o build UMD do Chart.js 4.4.4 de `https://cdn.jsdelivr.net/npm/chart.js@4.4.4/dist/chart.umd.min.js` (mesma versão e fonte do protótipo) para `wwwroot/lib/chartjs/chart.umd.min.js`, versionar o arquivo no repositório (não gerar em build, mesmo padrão do Bootstrap já vendorizado) e referenciá-lo em `App.razor`; verificar que o arquivo é servido pela aplicação (ex.: `curl` em `/lib/chartjs/chart.umd.min.js` retorna 200) e que `git status` mostra o arquivo rastreado após o commit
- [x] 1.2 Criar `wwwroot/js/charts.js` com uma função genérica de gráfico de barras horizontais (ex.: `renderBarChart(canvasId, labels, data, options)`), reutilizável por outros gráficos futuros; referenciar o módulo em `App.razor`; verificar que o arquivo é servido (200) e não gera erro de console ao carregar uma página vazia

## 2. Serviço de acesso a dados da Landing Page

- [x] 2.1 Criar `Services/ILandingPageService` e `LandingPageService` (usando `IDbContextFactory<AppDbContext>`) com `GetEstatisticasAsync()` (total de seleções, grupos, jogos e estádios distintos), `GetProximosJogosAsync()` (jogos sem placar oficial: buscar ordenados por `DataHora`, materializar com `ToListAsync()` e só então agrupar por `DataHora.Date` em memória, selecionando os jogos dos 2 primeiros dias — não traduzir o agrupamento por data para SQL, ver D3) e `GetTopRankingAsync(int quantidade = 10)`; registrar via `AddLandingPageServices` em `Extensions/` e chamar em `Program.cs`; verificar build
- [x] 2.2 Criar `Services/Charts/IChartInterop`/`ChartInterop` encapsulando a chamada a `charts.js` via `IJSRuntime`, com uma assinatura reutilizável (não específica de ranking); registrar em DI; verificar build
- [x] 2.3 Adicionar testes (xUnit, SQLite in-memory com `SeedData.SeedAsync`) para `LandingPageService`: estatísticas retornam 48/12/104/16; próximos jogos retornam apenas jogos sem placar oficial, cobrindo exatamente os 2 dias mais próximos com jogo agendado; quando todos os jogos têm placar oficial, a lista de próximos jogos vem vazia sem lançar exceção; `GetTopRankingAsync` retorna as 10 seleções de melhor posição em ordem crescente de posição; verificar que `dotnet test` passa

## 3. Componente HeroSection

- [x] 3.1 Criar `Components/Pages/LandingPage/HeroSection.razor` com o logo da Copa (`https://api.fifa.com/api/v3/picture/tournaments-sq-4/285023`), título/nome do portal, chamada para o Simulador (`/simulador`) e para Jogos (`/jogos`), e os 3 países-sede fixos (Estados Unidos, Canadá, México) com bandeira via API pública da FIFA e quantidade de estádios; verificar renderização rodando a aplicação (`dotnet run`) e inspecionando a seção no navegador (logo, nome do portal e os 3 países-sede visíveis)

## 4. Componente EstatisticasCopa

- [x] 4.1 Criar `Components/Pages/LandingPage/EstatisticasCopa.razor` injetando `ILandingPageService` e exibindo os 4 cards (seleções, grupos, jogos, estádios) com `@rendermode` estático (sem interatividade necessária); verificar que os valores exibidos são 48/12/104/16 rodando a aplicação com o banco semeado

## 5. Componente ProximosJogos

- [x] 5.1 Criar `Components/Pages/LandingPage/ProximosJogos.razor` injetando `ILandingPageService`, exibindo os jogos agrupados por dia (seleções ou vaga, data/hora, estádio, cidade, grupo) e uma mensagem quando não há jogos pendentes; verificar visualmente com o banco semeado (deve mostrar jogos a partir de 11/06/2026) e revisar o caso vazio manualmente (ex.: banco de teste com todos os jogos com placar)

## 6. Componente RankingFifaChart

- [x] 6.1 Criar `Components/Pages/LandingPage/RankingFifaChart.razor` com `@rendermode InteractiveServer`, injetando `ILandingPageService` e `IChartInterop`, renderizando o gráfico de barras do Top 10 em `OnAfterRenderAsync`; verificar visualmente que o gráfico aparece com as 10 seleções do topo do ranking semeado, na ordem correta

## 7. Componente SimuladorPainel

- [x] 7.1 Criar `Components/Pages/LandingPage/SimuladorPainel.razor` com o bloco de chamada estático e link para `/simulador`; verificar renderização (o link pode cair em "página não encontrada", já que o Simulador está fora do escopo desta mudança)

## 8. Composição da Home, estilo e verificação final

- [x] 8.1 Substituir `Components/Pages/Home.razor` pela composição dos 5 componentes dentro de um layout Bootstrap (`container`/`row`/`col-*`); verificar `dotnet build`
- [x] 8.2 Adicionar em `wwwroot/app.css` os tokens de tema do protótipo (fundo escuro, dourado de destaque) como sobreposição ao Bootstrap padrão; verificar visualmente que a página usa o tema escuro, não o tema claro padrão do Bootstrap
- [x] 8.3 Rodar a aplicação de ponta a ponta com o banco semeado do zero e confirmar visualmente as 6 seções (Hero, Estatísticas, Próximos Jogos, Ranking com gráfico renderizado, Simulador CTA); rodar `dotnet build` e `dotnet test` da solução completa e confirmar que tudo passa
