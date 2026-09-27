# Resultado: apply da change landing-page

13/13 tarefas implementadas (`state: "all_done"`). Build limpo, 20/20 testes passando (16 anteriores + 4 novos do `LandingPageService`).

## O que foi criado
- `wwwroot/lib/chartjs/chart.umd.min.js` (Chart.js 4.4.4, vendorizado a partir do jsDelivr, versionado no git) e `wwwroot/js/charts.js` (wrapper genérico `renderBarChart`/`destroyChart`, reutilizável).
- `Services/ILandingPageService` + `LandingPageService`: `GetEstatisticasAsync`, `GetProximosJogosAsync` (materializa antes de agrupar por dia — mitigação do risco de `GroupBy(DateTime.Date)` no SQLite, confirmada pelos testes), `GetTopRankingAsync`.
- `Services/Charts/IChartInterop`/`ChartInterop` (JSInterop fino sobre `charts.js`).
- 5 componentes em `Components/Pages/LandingPage/`: `HeroSection` (com logo da Copa, ajustado na revisão), `EstatisticasCopa`, `ProximosJogos`, `RankingFifaChart` (`@rendermode InteractiveServer`), `SimuladorPainel`.
- `Home.razor` composta pelos 5 componentes; `wwwroot/app.css` com tokens de tema escuro/dourado explícitos (não depende mais de dark-mode do SO).
- Testes: `LandingPageServiceTests` (4 novos) + `SqliteInMemoryFixture` estendida para também implementar `IDbContextFactory<AppDbContext>`.

## Verificação
- Testado no Chrome de verdade via `claude-in-chrome` (não só curl): Hero com logo/bandeiras/países-sede, estatísticas 48/12/104/16 exatas, próximos jogos com badges de grupo e horários corretos, gráfico de ranking renderizado com Top 10 e cores de medalha (ouro/prata/bronze), painel do Simulador visível, `/simulador` cai em 404 (esperado). Sem erros no console.
- Bug pego e corrigido durante a implementação (não estava no design): colisão de nome entre o componente `EstatisticasCopa.razor` e o record `EstatisticasCopa` do serviço — resolvido com alias `@using EstatisticasCopaDto = ...`.

## Próximo passo
Sugestão: arquivar a change (`/opsx:archive`) quando o usuário confirmar a implementação.
