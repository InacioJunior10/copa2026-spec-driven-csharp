# Resultado: apply da change foundation-blazor-app

25/25 tarefas implementadas (`openspec instructions apply` reporta `state: "all_done"`). Build limpo, 16/16 testes passando.

## O que foi criado
- `src/PortalCopa26.sln` + projeto `PortalCopa26` (Blazor Web App .NET 10, interatividade Server), pastas `Domain/{Entities,Enums}`, `Data/{Configurations,Migrations,Seed}`, `Services/`, `Extensions/`.
- `tests/PortalCopa26.Tests` (xUnit): pureza do domínio (sem referência a EF Core), testes de simulação com SQLite in-memory (isolamento entre visitantes, rejeição de placar duplicado e de gols fora de 0-30) e testes de seed (contagens, integridade, ordem cronológica, idempotência).
- 7 entidades de domínio, `AppDbContext` com Fluent API (índices únicos, FKs, CHECK constraints, enums como texto), migration `InitialCreate`, `AddPortalCopaData` via `IDbContextFactory`, `SeedData.SeedAsync` idempotente.
- `Data/Seed/{selecoes,jogos,ranking}.json`: convertidos das fontes reais do protótipo (`prototipo/fontes/*.txt` + `js/site.js`) por um script C# descartável (não faz parte da solução).
- `src/README.md` com instruções de uso.
- `.gitignore` atualizado com `*.db`, `*.db-shm`, `*.db-wal`.

## Verificação end-to-end
Banco recriado do zero populou corretamente: 12 grupos, 48 seleções, 1238 jogadores, 104 jogos (72 fase de grupos + 32 mata-mata), 98 rankings. Reinício não duplicou nada. App responde HTTP 200.

## Desvios/decisões durante a implementação (não previstos no design)
- `dotnet new sln` no .NET 10 gera `.slnx` por padrão; recriei explicitamente com `--format sln` para bater com o que as tasks.md pediam.
- Bug na regex de extração de "participações em Copas" do `site.js` (exigia duas chaves de fechamento em vez de uma) — corrigido antes de gravar os JSON finais; taxa de preenchimento ficou em 290/1238 jogadores (o restante fica `null`, conforme já previsto na Open Question do design).
- Todos os 48 códigos FIFA dos times foram resolvidos posicionalmente comparando `copa2026_grupos.txt` com `GRUPOS` do `site.js` (mesma ordem em cada grupo) — não precisei de matching fuzzy por nome para os times, só para variantes usadas nos arquivos de jogos/técnicos/elenco (ex.: "EUA", "Bósnia", "Curaçau").
- `RankingFifa.CodigoSelecao` é obrigatório na entidade; para seleções do ranking que não são nem uma das 48 nem estão no dicionário do `site.js`, gerei um código de exibição de 3 letras a partir do nome (não é um código FIFA oficial) em `SeedData.DeriveCodigoFallback`.
- Testes de simulação/seed usam SQLite in-memory com uma conexão nova por teste (não `IClassFixture` compartilhado), porque bancos compartilhados entre testes da mesma classe causavam violação de unique constraint entre um teste e outro.

## Próximo passo
Nenhuma página de domínio foi implementada (fora do escopo desta change). Sugestão: arquivar a change (`/opsx:archive`) quando o usuário confirmar a implementação.
