# Spec Delta

## Purpose

Define o comportamento observável da página Equipes: como as 48 seleções são listadas, buscadas e filtradas por grupo, e como o elenco convocado de cada seleção é exibido em um modal de detalhes.

## ADDED Requirements

### Requirement: Listagem das 48 seleções

A página Equipes SHALL exibir, em uma grade de cartões, as 48 seleções participantes da Copa do Mundo FIFA 2026. Cada cartão SHALL mostrar a bandeira, o nome da seleção, o grupo ("Grupo X") e o código FIFA de 3 letras.

#### Scenario: Todas as seleções exibidas ao carregar a página
- **WHEN** a página Equipes é carregada, sem busca nem filtro aplicados
- **THEN** os 48 cartões de seleção são exibidos, cada um com bandeira, nome, grupo e código FIFA

### Requirement: Busca por seleção

A página Equipes SHALL permitir buscar seleções por texto livre, comparando o texto digitado com o nome da seleção e com o código FIFA, sem diferenciar maiúsculas de minúsculas. A grade SHALL atualizar a lista exibida a cada alteração do texto de busca, sem recarregar a página.

#### Scenario: Busca por nome
- **WHEN** o usuário digita "bras" no campo de busca
- **THEN** a grade passa a exibir apenas as seleções cujo nome contém "bras" (ex.: "Brasil"), sem recarregar a página

#### Scenario: Busca por código FIFA
- **WHEN** o usuário digita "bra" no campo de busca
- **THEN** a seleção de código "BRA" é exibida na grade

### Requirement: Filtro por grupo

A página Equipes SHALL permitir filtrar as seleções exibidas por grupo, através de botões "Todos" e "Grp A" a "Grp L", com um filtro ativo por vez. Selecionar um filtro de grupo SHALL atualizar a grade imediatamente, exibindo somente as seleções daquele grupo, sem recarregar a página. O filtro "Todos" SHALL ser o padrão ao carregar a página.

#### Scenario: Filtrar por um grupo específico
- **WHEN** o usuário seleciona o filtro "Grp C"
- **THEN** a grade passa a exibir somente as 4 seleções do Grupo C, sem recarregar a página

#### Scenario: Voltar para todos os grupos
- **WHEN** o usuário seleciona o filtro "Todos" após ter filtrado por um grupo
- **THEN** a grade volta a exibir as 48 seleções (respeitando a busca em andamento, se houver)

### Requirement: Combinação de busca e filtro de grupo

A busca por texto e o filtro por grupo SHALL ser aplicados em conjunto: a grade SHALL exibir apenas as seleções que satisfazem ambos os critérios simultaneamente ativos.

#### Scenario: Busca dentro de um grupo filtrado
- **WHEN** o filtro "Grp C" está ativo e o usuário digita o nome de uma seleção que não pertence ao Grupo C
- **THEN** a grade não exibe nenhuma seleção

### Requirement: Mensagem de nenhum resultado

Quando a busca e o filtro de grupo combinados não corresponderem a nenhuma seleção, a página Equipes SHALL exibir uma mensagem indicando que nenhuma seleção foi encontrada, no lugar da grade vazia.

#### Scenario: Nenhuma seleção corresponde aos critérios
- **WHEN** a busca e o filtro de grupo ativos não correspondem a nenhuma seleção
- **THEN** a grade de cartões não é exibida e uma mensagem de "nenhuma seleção encontrada" aparece em seu lugar

### Requirement: Abertura do modal de detalhes da seleção

Ao selecionar um cartão de seleção, a página Equipes SHALL abrir um modal exibindo a bandeira, o nome da seleção, o grupo, o nome do técnico e a posição no Ranking FIFA. Quando a seleção não constar do Ranking FIFA, o modal SHALL omitir a posição no ranking, sem exibir um valor inventado.

#### Scenario: Seleção ranqueada
- **WHEN** o usuário seleciona o cartão de uma seleção que consta do Ranking FIFA
- **THEN** o modal exibe bandeira, nome, grupo, técnico e a posição no Ranking FIFA

#### Scenario: Seleção fora do Ranking FIFA
- **WHEN** o usuário seleciona o cartão de uma seleção que não consta do Ranking FIFA
- **THEN** o modal exibe bandeira, nome, grupo e técnico, sem exibir posição no ranking

### Requirement: Elenco de jogadores no modal

O modal de detalhes SHALL exibir a lista completa de jogadores convocados da seleção selecionada, com nome, posição, idade e gols pela seleção para cada jogador. Quando a informação de participações em Copas do jogador for conhecida na fonte de dados, o modal SHALL exibi-la; quando desconhecida, o modal SHALL omitir o valor, sem exibir um valor inventado. A quantidade de jogadores exibida SHALL refletir exatamente o elenco convocado daquela seleção na fonte oficial de dados, mesmo quando esse número for menor que 26.

#### Scenario: Elenco completo exibido
- **WHEN** o modal de uma seleção é aberto
- **THEN** todos os jogadores convocados dessa seleção são listados, cada um com nome, posição, idade e gols pela seleção

#### Scenario: Participações em Copas desconhecidas
- **WHEN** o elenco exibido no modal inclui um jogador sem participações em Copas conhecidas na fonte de dados
- **THEN** esse jogador é exibido sem um número de participações em Copas inventado

### Requirement: Fechamento do modal

A página Equipes SHALL permitir fechar o modal de detalhes através do botão de fechar, de um clique fora da área do modal, ou da tecla Esc, retornando à grade de seleções sem alterar a busca nem o filtro de grupo em andamento.

#### Scenario: Fechar pelo botão
- **WHEN** o usuário aciona o botão de fechar do modal
- **THEN** o modal é fechado e a grade de seleções, com a busca e o filtro anteriores, volta a ser exibida

#### Scenario: Fechar pela tecla Esc
- **WHEN** o modal está aberto e o usuário pressiona a tecla Esc
- **THEN** o modal é fechado
