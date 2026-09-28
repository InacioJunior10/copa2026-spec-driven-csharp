# Design

## Context

O modelo de dados já existe e já está populado (`data-persistence`): `Selecao` (Codigo, Nome, Tecnico, GrupoId, Jogadores), `Jogador` (Nome, Posicao, Idade, Gols, ParticipacoesCopas?, SelecaoId) e `RankingFifa` (Posicao, SelecaoId?). A carga inicial (`SeedData.SeedAsync`) já popula as 48 seleções com seus elencos (22 a 26 jogadores cada, conforme `fontes/copa2026_selecoes_jogadores.txt`) e técnicos (conforme `fontes/copa2026_pais_tecnicos.txt`) a partir de `Data/Seed/selecoes.json`.

O `NavBar` já tem um link para `/equipes`; a rota ainda não existe. O protótipo `equipes.html` define o layout e o comportamento client-side (busca + filtro + modal, tudo sem reload), e as classes CSS necessárias (`team-grid`, `team-card`, `modal-overlay`, `filter-btn`, `search-bar`, `pos`) já existem em `wwwroot/css/site.css` (linhas 415+, 563+). O padrão de acesso a dados do projeto usa `IDbContextFactory<AppDbContext>` dentro de um `I*Service`/`*Service` por página (ver `GruposService`), nunca `AppDbContext` direto em componentes Razor, e registro do serviço via `AddXxxServices()` em `Extensions/ServiceCollectionExtensions.cs`.

A lista de "Arquivos oficiais" do CLAUDE.md (6 arquivos) está desatualizada em relação ao conteúdo real de `./fontes` (hoje com 19 arquivos); o projeto já usa, desde `data-persistence`, outros arquivos dessa mesma pasta — incluindo `copa2026_pais_tecnicos.txt` e `copa2026_selecoes_jogadores.txt` — como fonte oficial. Esta change segue essa mesma prática já estabelecida: qualquer arquivo em `./fontes` é considerado dado oficial, independentemente de constar na lista textual do CLAUDE.md. `fontes/selecoes_jogadores_convocados.txt` também existe na pasta, mas é um arquivo de referência sem os campos estruturados (idade, posição, gols) usados pelo seed e por esta página; `copa2026_selecoes_jogadores.txt` é a fonte de fato usada, e o outro arquivo não é consumido por nenhuma capacidade do projeto.

Ver proposal.md para motivação e specs/equipes-page/spec.md para o comportamento requerido.

## Goals / Non-Goals

**Goals:**
- Implementar a página Equipes reproduzindo o protótipo (grade, busca, filtro, modal) usando componentes Blazor Server (`@rendermode InteractiveServer`), sem JS além do já existente para nav mobile.
- Manter busca e filtro totalmente no servidor (estado em `@code`, sem round-trip HTTP), replicando a experiência client-side do protótipo dentro do modelo de componentes do projeto.
- Reaproveitar entidades e dados já existentes; nenhuma migração de banco.

**Non-Goals:**
- Não alterar o modelo de dados nem a carga inicial (`data-persistence` já cobre `Selecao`/`Jogador`/`RankingFifa`).
- Não implementar edição de elenco, técnico ou ranking a partir desta página (somente leitura).
- Não resolver a divergência entre "26 jogadores por seleção" do pedido original e a variação real (22–26) da fonte oficial: a página exibe o elenco real, sem completar com jogadores fictícios (proibido por CLAUDE.md).

## Decisions

**Estrutura de página e componentes** — `Components/Pages/Equipes/Equipes.razor` como página (`@page "/equipes"`), com subcomponentes em `Components/Pages/Equipes/`, seguindo a convenção de `Components/Pages/Grupos/` e `Components/Pages/Jogos/` (componentes de uma página moram junto da página, não em uma pasta `Components/Times` separada como o pedido original sugeria). Componentes:
- `CartaoTime.razor`: um cartão da grade (bandeira, nome, grupo, código).
- `FiltroTime.razor`: barra de busca + botões de filtro por grupo.
- `ModalTime.razor`: modal com cabeçalho (bandeira, nome, grupo, técnico, ranking) e a tabela de elenco.
- `TabelaElencoTime.razor`: tabela de jogadores (nome, posição, idade, gols, copas), usada dentro de `ModalTime`.

*Alternativa considerada*: nome de pasta `Components/Times` do pedido original. Rejeitada para manter a convenção já estabelecida no projeto (pasta por página sob `Components/Pages`), evitando duas convenções concorrentes.

**Estado e busca/filtro sem reload** — Toda a lista de seleções é carregada uma vez em `OnInitializedAsync` (48 registros, custo desprezível) e filtrada em memória em `@code` a cada mudança de `_busca`/`_grupoFiltro`, igual ao padrão já usado em `Grupos.razor` para a aba de grupo ativo. O campo de busca usa `@bind:event="oninput"` para atualizar a cada tecla, como no protótipo.

*Alternativa considerada*: filtrar via query string e recarregar dados do servidor a cada filtro (como `Jogos.razor` faz para o grupo). Rejeitada porque o protótipo e a spec exigem atualização sem reload da página e o volume de dados (48 seleções) não justifica ida ao banco a cada tecla digitada.

**Modal como componente controlado pela página** — `Equipes.razor` mantém o código da seleção selecionada (`_selecaoAbertaId`); `ModalTime` recebe os dados já carregados (sem chamada adicional ao serviço) e dispara um `EventCallback` de fechar. Fechar por Esc é tratado com um `@onkeydown` no contêiner da página (o componente raiz já recebe foco do teclado em Blazor Server); clique fora usa o mesmo padrão do overlay do protótipo (checar se o clique foi no overlay, não no conteúdo do modal).

**Serviço único: `ISelecaoService`** — Um único método traz todas as seleções com grupo, técnico, ranking (quando houver) e elenco completo em uma consulta (`Include(s => s.Jogadores)`, join com `RankingFifa` por `SelecaoId`), retornando DTOs (`EquipeItem`, `EquipeJogadorItem`) — mesmo padrão de records usado em `IGruposService`/`IJogosService`, para não expor `Domain.Entities` aos componentes Razor.

```csharp
public record EquipeJogadorItem(string Nome, PosicaoJogador Posicao, int Idade, int Gols, int? ParticipacoesCopas);
public record EquipeItem(int SelecaoId, string Codigo, string Nome, char Grupo, string Tecnico, int? RankingPosicao, List<EquipeJogadorItem> Jogadores);

public interface ISelecaoService
{
    Task<List<EquipeItem>> GetEquipesAsync();
}
```

*Alternativa considerada*: carregar o elenco sob demanda (só ao abrir o modal). Rejeitada: 48 seleções com até 26 jogadores é um volume pequeno para uma única consulta na carga da página, e simplifica o componente de modal (sem estado de carregamento assíncrono próprio).

**Registro do serviço: `AddEquipesServices()`** — Seguindo a convenção já usada por `AddGruposServices()`/`AddJogosServices()` (nome do método de extensão pelo nome da página/capacidade, não pelo nome da interface de serviço), o registro de `ISelecaoService` em `Extensions/ServiceCollectionExtensions.cs` é feito por um método `AddEquipesServices()`.

**Participações em Copas desconhecidas: omitir, não indicar "não disponível"** — Quando `ParticipacoesCopas` for nulo, `TabelaElencoTime` omite o valor (célula vazia) em vez de exibir um rótulo como "N/D". Mantém a tabela mais limpa e evita introduzir um texto que não está no protótipo original.

**Colisão de nome com `.modal` do Bootstrap 5, resolvida com estilo inline** — A classe `.modal` reaproveitada de `site.css` (a caixa do modal) tem o mesmo nome da classe `.modal` do Bootstrap 5, que por padrão define `display: none` e `position: fixed` (usadas pela própria API de modal do Bootstrap). Isso escondia a caixa do modal e, quando forçado a exibir, a tirava do fluxo flex do `.modal-overlay`, ancorando-a no canto superior esquerdo em vez de centralizada. Corrigido com `style="display:block;position:relative"` inline em `ModalTime.razor`, que vence a cascata sem precisar renomear a classe reutilizada nem alterar `site.css`.

## Risks / Trade-offs

- [Carregar os 48 elencos completos na carga inicial da página aumenta o payload trazido do banco em cada visita] → Aceitável dado o volume (48 × ≤26 jogadores); mesmo padrão de "carregar tudo uma vez" já usado em `Grupos.razor`/`Jogos.razor`.
- [Elencos com contagem variável (22–26) podem ser lidos como "dados incompletos" pelo usuário, divergindo do critério de aceitação original de 26] → Mitigado exibindo o elenco real sem preenchimento fictício (requisito explícito na spec) e documentado aqui e na proposta.
- [Os cartões da grade são reproduzidos fielmente do protótipo, que os torna clicáveis via mouse mas não define um handler de teclado (`Enter`/`Espaço`) para abrir o modal a partir do foco do cartão] → Limitação herdada do protótipo, aceita nesta primeira versão; não bloqueia a change pois reproduz o comportamento já validado na fonte de design, mas fica registrada aqui para uma futura melhoria de acessibilidade.
