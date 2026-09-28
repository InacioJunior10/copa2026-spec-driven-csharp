# Proposal

## Why

O PRD (seção 10) define o Simulador como uma das 6 páginas principais do portal e a chamada de destaque da Home já aponta para `/simulador`, mas a rota não existe — cai em `NotFound`. `data-persistence` já modela `Simulacao`/`SimulacaoJogo` exatamente para isso (placares simulados por visitante anônimo, sem tocar no placar oficial), porém nada os usa ainda. Esta mudança implementa a página Simulador da fase de grupos, com base no protótipo validado (`../prototipo/simulador.html`), persistindo os placares simulados no SQLite em vez de `localStorage`.

## What Changes

- Cria a rota `/simulador` com 4 componentes novos em `Components/Pages/Simulador/`: `SimuladorGrupo.razor` (painel do grupo ativo: abas de grupo, jogos e ações de limpar), `SimuladorJogo.razor` (uma linha de jogo com os dois campos de placar), `ClassificacaoGrupo.razor` (tabela Bootstrap responsiva da classificação simulada) e `SimuladorResumo.razor` (card com as seleções do grupo ativo e sua posição no ranking FIFA).
- Simula apenas a fase de grupos (12 grupos, 6 jogos por grupo); explicitamente **não** implementa o mata-mata nesta mudança.
- Permite informar o placar de cada jogo do grupo ativo; a cada alteração, a classificação do grupo é recalculada automaticamente (pontos, vitórias/empates/derrotas, saldo de gols) e o placar é persistido automaticamente no SQLite, sem exigir nome ou confirmação explícita de salvar.
- Ao entrar na página (mesma aba ou uma nova visita), a simulação em andamento do visitante é restaurada automaticamente a partir do banco.
- Destaca na classificação os 2 primeiros colocados como classificados e o 3º colocado como wildcard (mesmo critério visual do protótipo), sem calcular a disputa de wildcard entre os 12 grupos (fora do escopo — depende do mata-mata).
- Ações "Limpar Grupo" (remove os placares simulados só do grupo ativo) e "Limpar Tudo" (remove todos os placares simulados do visitante).
- Identifica o "visitante" (dono da simulação) sem login, via um identificador anônimo persistente no navegador, para que a simulação sobreviva a fechamentos de aba/navegador.
- Usa exclusivamente os dados oficiais do Seed (seleções, grupos, jogos, ranking FIFA); nenhum dado fictício é introduzido.

## Capabilities

### New Capabilities
- `simulador-page`: comportamento observável da página Simulador — o que é exibido por grupo, como o placar simulado é informado e persistido, como a classificação e os destaques (classificados/wildcard) são calculados, e como "Limpar Grupo"/"Limpar Tudo" se comportam.

### Modified Capabilities
(nenhuma — `data-persistence` já expõe as entidades `Simulacao`/`SimulacaoJogo` com as regras de negócio necessárias — placar entre 0 e 30, no máximo um placar por jogo por simulação, sem alterar o placar oficial —; esta mudança apenas consome essas entidades já especificadas, sem alterar seu comportamento)

## Impact

- Novo código: `Components/Pages/Simulador/*.razor` (`SimuladorGrupo`, `SimuladorJogo`, `ClassificacaoGrupo`, `SimuladorResumo`), e um serviço de dados (`Services/ISimuladorService`/`SimuladorService`) para ler/gravar `Simulacao`/`SimulacaoJogo` e calcular a classificação simulada.
- `Components/Layout/NavBar.razor` e a chamada da Home (`SimuladorPainel.razor`) já linkam `/simulador` — passam a resolver de fato.
- Consome as entidades `Simulacao`/`SimulacaoJogo` já persistidas por `data-persistence` (nenhuma migration nova).
- Reaproveita os estilos já vendorizados do protótipo em `wwwroot/css/site.css` (`.sim-card`, `.sim-game`, `.sim-input`, `.group-tabs`/`.gtab`, `.t-pos.qualify`/`.playoff`), combinados com uma tabela Bootstrap (`table`/`table-responsive`) para a classificação, conforme pedido explicitamente.
- Fora do escopo: simulação do mata-mata (oitavas em diante), disputa real de wildcard entre os 12 grupos, qualquer edição do placar oficial dos jogos, e login/identificação real de usuário (o "visitante" continua anônimo).
