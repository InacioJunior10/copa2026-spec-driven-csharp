# PortalCopa26

Aplicação web para fãs da Copa do Mundo FIFA 2026: jogos, grupos, seleções, elencos, ranking FIFA e simulação de resultados.

O PRD completo (visão geral, requisitos por página, regras de negócio e escopo) está em [`CLAUDE.md`](./CLAUDE.md).

## Stack

- **Aplicação:** Blazor Web App (.NET 10), interatividade Server
- **Persistência:** EF Core + SQLite
- **Testes:** xUnit
- **Planejamento:** [OpenSpec](https://github.com/Fission-AI/OpenSpec) (`openspec/`)

## Estrutura do repositório

```
prd/
├── CLAUDE.md                 # PRD completo do projeto
├── src/                       # Solução PortalCopa26 (Blazor Web App)
│   ├── PortalCopa26.sln
│   ├── PortalCopa26/           # Projeto da aplicação
│   └── README.md               # Como rodar, testar e resetar o banco
├── tests/
│   └── PortalCopa26.Tests/     # Testes automatizados (xUnit)
├── openspec/                  # Planejamento e histórico de changes (OpenSpec)
│   ├── specs/                   # Main specs (capabilities já implementadas)
│   └── changes/                  # Changes em andamento e arquivadas
└── memory/                    # Registro de prompts e resultados por interação
    ├── prompt/
    ├── result/
    └── agents/
```

## Como rodar

Instruções detalhadas (executar, testar, resetar o banco, gerar migrations) estão em [`src/README.md`](./src/README.md). Resumo:

```bash
cd src/PortalCopa26
dotnet run
```

Na primeira execução, o EF Core aplica as migrations e popula o banco SQLite com os dados iniciais da Copa (grupos, seleções, jogadores, jogos e ranking FIFA).

## Status

- ✅ Fundação da aplicação (solução, EF Core/SQLite, entidades, seed) — ver `openspec/changes/archive/2026-09-27-foundation-blazor-app/`
- ⏳ Páginas do portal (Home, Jogos, Grupos, Equipes, Ranking, Simulador) — ainda não implementadas

## Fora de escopo (primeira versão)

Área administrativa, cadastro de usuários, autenticação e integração automática com APIs esportivas. Detalhes na seção 12 do [`CLAUDE.md`](./CLAUDE.md).
