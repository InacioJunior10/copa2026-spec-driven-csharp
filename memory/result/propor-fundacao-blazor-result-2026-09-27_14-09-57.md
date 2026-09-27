# Resultado: propor fundação Blazor Web App (OpenSpec)

Change `foundation-blazor-app` criada em `openspec/changes/foundation-blazor-app/` (spec-driven). Os 4 artefatos estão completos e passam em `openspec validate --strict`. Foi uma nova execução: a versão anterior tinha sido apagada e só restava a pasta vazia, que foi reaproveitada.

## Artefatos
- proposal.md: fundação Blazor Web App (.NET 10, interatividade Server) + EF Core/SQLite em `src/`, 7 entidades, seed idempotente e projeto de testes.
- specs/app-foundation: app executável, entidades independentes do EF e acesso a dados via DI adequado ao Blazor Server.
- specs/data-persistence: modelo de dados, 104 jogos de todas as fases com vagas indefinidas, SQLite com migrations, seed idempotente (12/48/104) e simulações isoladas por visitante com placar de 0 a 30.
- design.md: decisões D1–D10.
- tasks.md: 7 grupos, cada um com verificação e testes próprios.

## Decisões relevantes
- Fonte do seed: `fontes/*.txt` (48 seleções, ~1238 jogadores, 104 jogos, ranking de 98 seleções). O `site.js` entra só com os códigos FIFA e as participações em Copas; quando não há informação, o valor fica nulo.
- `Jogo` cobre todas as fases, com mandante e visitante opcionais e a vaga em texto.
- `IDbContextFactory` em vez de `DbContext` scoped (Blazor Server).
- `RankingFifa.Pontos` como double, porque o SQLite não ordena decimal; FK opcional para `Selecao`.
- `Simulacao.VisitanteId` (Guid anônimo) substitui o localStorage do protótipo.
- `fontes/ddd_ajustes_refinamentos1.txt` é de outro projeto e foi ignorado.

## Pendência
- Open question: fonte das participações em Copas para os jogadores que não estão no `site.js`.

## Próximo passo
Aguardando o usuário iniciar o `/opsx:apply`.
