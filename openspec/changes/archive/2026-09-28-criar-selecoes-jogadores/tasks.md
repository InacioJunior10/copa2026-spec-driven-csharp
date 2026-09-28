# Tasks

## 1. Serviço de acesso a dados

- [x] 1.1 Criar `Services/ISelecaoService.cs` com os records `EquipeJogadorItem`, `EquipeItem` e o método `Task<List<EquipeItem>> GetEquipesAsync()`, conforme design.md
- [x] 1.2 Implementar `Services/SelecaoService.cs` usando `IDbContextFactory<AppDbContext>`, incluindo `Jogadores` e cruzando com `RankingsFifa` por `SelecaoId`, e verificar com um teste unitário (`SqliteInMemoryFixture`) que `GetEquipesAsync` retorna 48 itens, cada um com grupo, técnico e a lista de jogadores do elenco seedado
- [x] 1.3 Adicionar teste cobrindo seleção sem posição no Ranking FIFA (`RankingPosicao` nulo) e seleção com Ranking FIFA (`RankingPosicao` preenchido)
- [x] 1.4 Registrar `AddEquipesServices()` em `Extensions/ServiceCollectionExtensions.cs` (padrão de `AddGruposServices`) e chamá-lo em `Program.cs`

## 2. Componentes da página Equipes

- [x] 2.1 Criar `Components/Pages/Equipes/CartaoTime.razor`, recebendo um `EquipeItem` e um `EventCallback<int>` de seleção, exibindo bandeira (`https://api.fifa.com/api/v3/picture/flags-sq-4/{codigo}`), nome, "Grupo X" e código FIFA com as classes `team-card`/`team-card-name`/`team-card-sub` já existentes em `site.css`
- [x] 2.2 Criar `Components/Pages/Equipes/FiltroTime.razor`, com campo de busca (`@bind:event="oninput"`) e botões "Todos"/"Grp A".."Grp L" (classe `filter-btn`, com `active` no selecionado), expondo os valores via `EventCallback` para a página
- [x] 2.3 Criar `Components/Pages/Equipes/TabelaElencoTime.razor`, recebendo a lista de `EquipeJogadorItem` e renderizando a tabela (Jogador, Pos., Idade, Gols, Copas), omitindo o valor de Copas quando `ParticipacoesCopas` for nulo
- [x] 2.4 Criar `Components/Pages/Equipes/ModalTime.razor`, recebendo o `EquipeItem` selecionado (ou nulo) e um `EventCallback` de fechar; exibir cabeçalho (bandeira, nome, "Grupo X", técnico, "Ranking FIFA #N" somente quando `RankingPosicao` não for nulo) e `TabelaElencoTime`; fechar ao clicar no botão de fechar ou fora do modal (`modal-overlay`/`modal`)

## 3. Página Equipes e integração

- [x] 3.1 Criar `Components/Pages/Equipes/Equipes.razor` (`@page "/equipes"`, `@rendermode InteractiveServer`), carregando as equipes em `OnInitializedAsync` via `ISelecaoService`, mantendo `_busca`, `_grupoFiltro` e `_selecaoAbertaId` em `@code`, filtrando em memória e exibindo `CartaoTime`/`FiltroTime`/`ModalTime`/mensagem de "nenhuma seleção encontrada"
- [x] 3.2 Tratar Esc para fechar o modal (handler de teclado na página) e verificar manualmente que Esc, clique fora e o botão de fechar fecham o modal sem alterar busca/filtro em andamento
- [x] 3.3 Rodar a aplicação (`dotnet run`) e verificar manualmente, comparando com `prototipo/equipes.html`: 48 cartões ao carregar, busca por nome e por código FIFA, filtro por cada um dos 12 grupos, combinação busca+filtro, mensagem de "nenhuma seleção encontrada", e modal com elenco completo (contagem real de cada seleção, incluindo alguma com menos de 26 jogadores) para pelo menos 3 seleções diferentes

## 4. Testes automatizados da página

- [x] 4.1 Escrever `tests/PortalCopa26.Tests/Services/SelecaoServiceTests.cs` cobrindo: 48 seleções retornadas, elenco de uma seleção conhecida (ex.: "BRA") com os jogadores esperados, e a seleção sem Ranking FIFA retornando `RankingPosicao` nulo
- [x] 4.2 Rodar `dotnet test` na solução e verificar que todos os testes (novos e existentes) passam
