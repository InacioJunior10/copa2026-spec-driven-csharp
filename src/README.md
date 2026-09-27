# PortalCopa26 — Fundação (Blazor Web App + EF Core + SQLite)

Este diretório contém a solução `PortalCopa26.sln`: o projeto de aplicação `PortalCopa26` (Blazor Web App, .NET 10) e, em `../tests`, o projeto de testes `PortalCopa26.Tests` (xUnit).

Nesta primeira fase (fundação) não há nenhuma página de domínio — apenas a estrutura, o modelo de dados e a carga inicial (SeedData) a partir dos dados do protótipo.

## Rodar a aplicação

```bash
cd src/PortalCopa26
dotnet run
```

Na primeira execução, o EF Core aplica as migrations e popula o banco (grupos, seleções, jogadores, jogos e ranking FIFA). Reinícios seguintes não duplicam os dados.

## Rodar os testes

```bash
cd src
dotnet test PortalCopa26.sln
```

## Onde fica o banco de dados

Arquivo SQLite local `portalcopa26.db`, criado em `src/PortalCopa26/` (mesmo diretório do projeto, ao lado do `appsettings.json`). Não é versionado (veja `.gitignore` na raiz do repositório).

A connection string está em `appsettings.json` (`ConnectionStrings:PortalCopa26`); para usar outro caminho em desenvolvimento, sobrescreva em `appsettings.Development.json`.

## Como resetar o banco

Com a aplicação parada, apague o arquivo `portalcopa26.db` (e `portalcopa26.db-shm`/`portalcopa26.db-wal`, se existirem) dentro de `src/PortalCopa26/`. Na próxima execução, o banco é recriado e populado do zero.

```bash
rm src/PortalCopa26/portalcopa26.db*
```

## Como gerar uma nova migration

A partir de `src/PortalCopa26`:

```bash
dotnet ef migrations add NomeDaMigration --output-dir Data/Migrations
```

A migration é aplicada automaticamente no próximo `dotnet run` (via `Database.MigrateAsync()` no `Program.cs`).

## Dados de seed

Os arquivos `Data/Seed/selecoes.json`, `Data/Seed/jogos.json` e `Data/Seed/ranking.json` são gerados a partir do protótipo (`prototipo/fontes/*.txt` e `prototipo/js/site.js`) e versionados no repositório. `SeedData.SeedAsync` os lê e popula o banco apenas se ele estiver vazio.

## Convenção de horário

Todos os horários de `Jogo.DataHora` estão no horário de Brasília (não há conversão de fuso armazenada), consistente com o protótipo e com o público-alvo do portal.
