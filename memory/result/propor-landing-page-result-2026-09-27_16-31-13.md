# Resultado: propor Landing Page (OpenSpec)

Criada a change `landing-page` (schema `spec-driven`), com os 4 artefatos completos e validados (`openspec validate --strict` OK). Nova capability: `landing-page` (nenhuma capability existente — `app-foundation`, `data-persistence` — foi modificada).

## Análise do protótipo
`index.html`/`site.css`/`site.js`: hero com 3 países-sede fixos, 4 stat-cards, próximos jogos agrupados pelos 2 primeiros dias com jogo (usa data fixa `'2026-06-05'` no protótipo), gráfico de ranking Top 10 em barras horizontais via Chart.js 4.4.4 (CDN), CSS próprio com tema escuro/dourado (não Bootstrap).

## Decisões registradas no design.md
- "Próximos jogos" definido por **ausência de placar oficial** (não por relógio do sistema) — evita seção vazia se o clock real estiver fora da janela 11/06–19/07/2026.
- Estatística de estádios via `SELECT DISTINCT Jogo.Estadio` (sem entidade `Estadio`); confirmei 16 estádios distintos no seed real, batendo com o PRD.
- `Services/ILandingPageService` + `Services/IChartInterop` (wrapper genérico de Chart.js, reutilizável, não específico do Ranking).
- Chart.js vendorizado localmente em `wwwroot/lib/chartjs/` (mesmo padrão do Bootstrap já vendorizado pelo template), não via CDN.
- Bootstrap 5 para grid/layout + tokens de tema do protótipo (fundo escuro, dourado) sobrepostos em `app.css` — não um port 1:1 do `site.css`.
- `RankingFifaChart.razor` precisa de `@rendermode InteractiveServer` (JSInterop); os demais componentes ficam estáticos.
- Link do Simulador aponta para `/simulador`, que cairá em NotFound (implementação do Simulador é outra change).

## Artefatos
- `proposal.md`, `specs/landing-page/spec.md` (5 requirements: Hero, Estatísticas, Próximos Jogos, Ranking Top 10, Chamada Simulador), `design.md` (D1–D8), `tasks.md` (8 grupos, 12 tarefas).

## Próximo passo
Aguardando o usuário iniciar `/opsx:apply`.
