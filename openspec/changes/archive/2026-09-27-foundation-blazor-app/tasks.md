# Tasks

## 1. Solução e projetos

- [x] 1.1 Criar `src/PortalCopa26.sln` e o projeto `src/PortalCopa26` com `dotnet new blazor --interactivity Server -f net10.0`; verificar que `dotnet build src/PortalCopa26.sln` compila e que `dotnet run` serve a página inicial com HTTP 200
- [x] 1.2 Criar as pastas `Domain/Entities`, `Domain/Enums`, `Data/Configurations`, `Data/Seed`, `Services` e `Extensions` no projeto (D1); verificar que a estrutura existe e o build continua verde
- [x] 1.3 Criar `tests/PortalCopa26.Tests` (xUnit, net10.0) referenciando o projeto da aplicação e adicioná-lo à solução; verificar que `dotnet test src/PortalCopa26.sln` executa com sucesso
- [x] 1.4 Adicionar `*.db`, `*.db-shm` e `*.db-wal` ao `.gitignore`; verificar com `git check-ignore portalcopa26.db`

## 2. Entidades de domínio

- [x] 2.1 Criar os enums `FaseJogo` e `PosicaoJogador` em `Domain/Enums`; verificar o build
- [x] 2.2 Criar as entidades `Grupo`, `Selecao` e `Jogador` conforme D5 (com `ParticipacoesCopas` anulável), sem atributos de EF; verificar o build
- [x] 2.3 Criar a entidade `Jogo` conforme D5 (fase, rótulo, grupo opcional, `DataHora` em horário de Brasília, estádio/cidade, mandante/visitante opcionais, vagas, placar oficial anulável); verificar o build
- [x] 2.4 Criar as entidades `RankingFifa` (`Pontos` double, `SelecaoId` opcional), `Simulacao` (`VisitanteId`, datas) e `SimulacaoJogo`; verificar o build
- [x] 2.5 Adicionar um teste que falha se algum tipo do namespace `PortalCopa26.Domain` referenciar o assembly `Microsoft.EntityFrameworkCore`; verificar que `dotnet test` passa

## 3. EF Core, SQLite e injeção de dependência

- [x] 3.1 Adicionar `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.EntityFrameworkCore.Design` (versão 10.x); verificar com `dotnet list package`
- [x] 3.2 Criar `AppDbContext` com os 7 `DbSet`s e aplicar as configurações via `ApplyConfigurationsFromAssembly`; verificar o build
- [x] 3.3 Criar um `IEntityTypeConfiguration<T>` por entidade: `Grupo.Letra` e `Selecao.Codigo` únicos, `Jogo.Numero` único, FKs mandante/visitante e `SimulacaoJogo.JogoId` com `Restrict`, `SimulacaoJogo.SimulacaoId` com `Cascade`, índice único (`SimulacaoId`, `JogoId`), índice em `Simulacao.VisitanteId`, `CHECK` 0–30 nos gols simulados e enums armazenados como texto; verificar com `dotnet ef dbcontext info`
- [x] 3.4 Adicionar a connection string `PortalCopa26` em `appsettings.json` e criar `AddPortalCopaData(IConfiguration)` em `Extensions/` registrando `AddDbContextFactory<AppDbContext>` (D3); chamar em `Program.cs` e verificar que a aplicação sobe sem erros de DI
- [x] 3.5 Adicionar testes com SQLite in-memory cobrindo: placar simulado não altera o placar oficial do `Jogo`, duas simulações isoladas para o mesmo jogo, rejeição de placar duplicado na mesma simulação e rejeição de gols fora de 0–30; verificar que `dotnet test` passa

## 4. Migration inicial

- [x] 4.1 Gerar `InitialCreate` com `dotnet ef migrations add InitialCreate --output-dir Data/Migrations`; revisar se as 7 tabelas, índices únicos, CHECKs e comportamentos de exclusão estão presentes
- [x] 4.2 Aplicar `MigrateAsync()` na inicialização em `Program.cs` (D4); verificar que iniciar sem `portalcopa26.db` cria o arquivo com as 7 tabelas e que um segundo start preserva os dados

## 5. Dados de seed (conversão das fontes)

- [x] 5.1 Montar o dicionário nome→código FIFA cobrindo as variações de nome das fontes (Context do design) e gerar `Data/Seed/selecoes.json` com as 48 seleções (código, nome, grupo, técnico, cabeça de chave) e seus jogadores a partir de `fontes/copa2026_selecoes_jogadores.txt`, `copa2026_grupos.txt`, `copa2026_pais_tecnicos.txt` e `copa2026_cabecas-chave.txt`, preenchendo `participacoesCopas` a partir do `site.js` quando houver correspondência (senão `null`); verificar 48 seleções, 4 por grupo, todas com técnico e elenco não vazio, e a conversão falhando para nome não mapeado
- [x] 5.2 Gerar `Data/Seed/jogos.json` com os 104 jogos a partir dos arquivos de jogos em `fontes/` (72 de grupos, 16 da segunda fase, 8 oitavas, 4 quartas, 2 semifinais, 3º lugar e final), com estádios pelo nome oficial de `cidades_sede_estadios.txt`, rótulos (ex.: "Oitavas 1") e vagas textuais para seleções indefinidas; verificar contagens por fase e que o jogo de abertura e a final batem com as fontes
- [x] 5.3 Gerar `Data/Seed/ranking.json` a partir de `fontes/copa2026_ranking_fifa.txt` (98 seleções, pontos decimais); verificar que não há posições duplicadas e que os pontos decrescem com a posição
- [x] 5.4 Marcar os JSON como conteúdo copiado para a saída (ou recurso embarcado); verificar que estão presentes em `bin/` após o build

## 6. SeedData

- [x] 6.1 Implementar `SeedData.SeedAsync(AppDbContext)` lendo os JSON e inserindo grupos → seleções → jogadores → jogos → ranking (vinculando `RankingFifa.SelecaoId` quando a seleção estiver na Copa) em uma única transação, somente se `Grupos` estiver vazio (D8); verificar o build
- [x] 6.2 Chamar `SeedAsync` na inicialização logo após `MigrateAsync`, usando um contexto criado pela factory; verificar no log/consulta que um banco novo fica populado
- [x] 6.3 Adicionar testes de seed com SQLite in-memory: 12 grupos, 48 seleções (4 por grupo), 104 jogos (72 de grupos), toda seleção com elenco, jogos de grupo entre seleções do mesmo grupo, BRA no grupo C com técnico, final sem seleções e com vagas "Venc. Semifinal 1"/"Venc. Semifinal 2", jogos em ordem cronológica de 11/06 a 19/07, e uma segunda execução de `SeedAsync` sem alterar as contagens; verificar que `dotnet test` passa

## 7. Verificação integrada e documentação

- [x] 7.1 Apagar `portalcopa26.db`, rodar a aplicação, confirmar a página inicial com HTTP 200 e as contagens no banco (ex.: `sqlite3` ou log de inicialização); reiniciar e confirmar que as contagens não mudam
- [x] 7.2 Criar `src/README.md` com como rodar a aplicação e os testes, onde fica o banco, como resetá-lo (apagar o `.db`), como gerar novas migrations e a convenção de horário de Brasília; verificar que os comandos documentados funcionam como escritos
