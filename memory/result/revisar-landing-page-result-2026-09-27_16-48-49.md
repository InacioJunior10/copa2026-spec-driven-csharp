# Resultado: revisão sdd-review + ajustes na change landing-page

Primeira execução da skill `sdd-review` (recém-criada pelo usuário), aplicada à change `landing-page`. Achados e correção aplicada:

## Achados (por severidade)
- **[Médio]** Logo da Copa (exigido pelo CLAUDE.md seção 5, Hero Section) estava ausente do requirement/design/tasks do Hero — só citado de forma genérica no proposal.
- **[Médio]** Risco técnico não endereçado: `GroupBy(j => j.DataHora.Date)` pode não traduzir para SQL no provider SQLite do EF Core.
- **[Baixo]** "Estatísticas da Copa" não constava na seção 5 do CLAUDE.md (a change a introduz, consistente internamente, mas divergia do PRD-base).
- **[Baixo]** Fallback de código de seleção do ranking (`DeriveCodigoFallback`) pode gerar URL de bandeira inválida se uma seleção "fallback" entrar no Top 10 no futuro.
- **[Baixo]** `IChartInterop` (interop de UI) na mesma pasta `Services/` que `ILandingPageService` (acesso a dados).
- **[Baixo]** Task 1.1 não especificava a URL de origem do Chart.js nem se o arquivo deveria ser versionado no git.
- **[Baixo]** Nenhuma tarefa cobre teste automatizado dos componentes Razor (aceito como está, sem ajuste).

Conclusão da revisão: "Requer ajustes antes do Apply".

## Ajustes aplicados
- `specs/landing-page/spec.md`: requirement do Hero renomeado e ampliado para incluir o logo da Copa.
- `design.md`: D1 (logo no Hero), D3 (agrupamento por dia explicitamente em memória, não traduzido para SQL), D5 (`IChartInterop` movido para `Services/Charts/`), D6 (URL exata do Chart.js + versionamento no git), e dois novos itens em Risks/Trade-offs (fallback de código no ranking; mitigação do GroupBy).
- `tasks.md`: 1.1 (URL exata + commit), 2.1 (materializar antes de agrupar), 2.2 (novo caminho `Services/Charts/`), 3.1 (logo da Copa).
- `CLAUDE.md` (seção 5): adicionada subseção "Estatísticas da Copa" com nota de que foi incorporada via a change `landing-page`.

`openspec validate --strict` passou após os ajustes. Nenhum código foi implementado — só os artefatos de planejamento e o PRD foram atualizados.

## Próximo passo
Aguardando o usuário iniciar `/opsx:apply` para a change `landing-page` já ajustada.
