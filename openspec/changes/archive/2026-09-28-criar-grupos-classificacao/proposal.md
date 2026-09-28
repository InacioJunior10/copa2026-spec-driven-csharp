# Proposal

## Why

O protótipo estático (`grupos.html`) mostra grupos, classificação e jogos, mas usa dados fixos em JavaScript e não permite registrar resultados. O portal já persiste jogos, seleções e ranking FIFA em SQLite (capacidade `data-persistence`) e já calcula classificações no Simulador, mas não existe hoje nenhuma página nem serviço que exiba a classificação oficial dos 12 grupos com base nos placares oficiais registrados, nem uma forma de registrar esses placares oficiais pela interface.

## What Changes

- Criar a página Grupos (`/grupos`), com navegação por abas entre os 12 grupos (A–L), replicando a fidelidade visual do protótipo (legenda de classificação, abas, cartão de classificação e cartão de jogos do grupo).
- Calcular a classificação oficial de cada grupo dinamicamente a partir dos placares oficiais armazenados em `Jogo` (campos `GolsMandante`/`GolsVisitante`), seguindo os critérios de desempate de RN-01 (`fontes/copa2026_regras_negocio.txt`): pontos → saldo de gols → gols marcados → confronto direto → saldo de gols no confronto direto → Ranking FIFA. O critério de Fair Play (cartões) da RN-01 não é implementado nesta change por não existir fonte de dados de cartões; ao chegar nesse ponto do desempate, o cálculo segue direto para o Ranking FIFA.
- Exibir, por seleção, na classificação: posição, seleção, jogos, vitórias, empates, derrotas, gols pró, gols contra, saldo de gols e pontos; destacar visualmente as 2 primeiras posições (classificado) e a 3ª (candidata a wildcard), reaproveitando o padrão visual já usado na classificação do Simulador.
- Permitir registrar e atualizar, diretamente na página Grupos, o placar oficial de qualquer jogo da fase de grupos; ao salvar, o placar é persistido em `Jogo`, e a classificação e as estatísticas das seleções envolvidas são recalculadas e refletidas imediatamente na interface, sem exigir recarregar a página.
- Exibir um botão "Simular" no grupo ativo, que direciona para a página Simulador filtrada nesse grupo.
- Extrair a lógica de acumulação de estatísticas por seleção (jogos, V/E/D, gols pró/contra) a partir de uma lista de jogos com placar, hoje duplicada implicitamente no cálculo do Simulador, para um ponto único reutilizável pelas duas páginas, mantendo os critérios de ordenação/desempate de cada página independentes (detalhado em `design.md`).

## Capabilities

### New Capabilities
- `grupos-page`: comportamento observável da página Grupos — abas dos 12 grupos, classificação oficial calculada a partir dos placares oficiais, listagem dos jogos do grupo, registro/atualização de placares oficiais e navegação para o Simulador.

### Modified Capabilities
- `data-persistence`: adiciona o requisito de que o placar oficial de um jogo da fase de grupos pode ser criado e atualizado após a carga inicial (hoje a spec só cobre a carga inicial e a leitura do placar oficial, não sua atualização), sem afetar as simulações de visitantes.
- `simulador-page`: adiciona o requisito de que a página Simulador aceite um grupo inicial via parâmetro de URL, para que o botão "Simular" da página Grupos abra o Simulador já no grupo correto (hoje o Simulador sempre inicia no primeiro grupo disponível).

## Impact

- Novo: `Components/Pages/Grupos/*` (página e componentes: abas, legenda, classificação, lista de jogos do grupo, formulário de placar oficial), `Services/IGruposService.cs` + `GruposService.cs`.
- Possível novo tipo compartilhado para o cálculo de estatísticas por seleção, referenciado tanto por `GruposService` quanto por `SimuladorService` (decisão de local exato em `design.md`).
- `AppDbContext`/`Jogo`: nenhuma mudança de esquema — `GolsMandante`/`GolsVisitante` já existem; passam a ser atualizáveis via um novo método de serviço.
- `NavBar.razor`: link "Grupos" já existe e hoje leva a rota inexistente; passa a resolver para a nova página.
- `Jogos.razor`/`JogoCard.razor`: sem mudança de comportamento; o botão "Ver Grupos" já existente passa a apontar para uma página real.
- `Simulador.razor`: ganha um novo parâmetro de URL opcional para grupo inicial (comportamento aditivo, coberto pela spec delta de `simulador-page`); sem parâmetro, o comportamento observável é idêntico ao atual.
- Novo `GrupoJogo.razor`: componente de jogo com placar editável específico da página Grupos, no mesmo padrão visual e de interação de `SimuladorJogo.razor` (ver design.md D7).
