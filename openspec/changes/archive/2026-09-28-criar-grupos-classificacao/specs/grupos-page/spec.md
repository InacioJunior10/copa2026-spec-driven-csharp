# Spec Delta

## Purpose

Define o comportamento observável da página Grupos: como os 12 grupos são navegados por abas, como a classificação oficial de cada grupo é calculada e desempatada a partir dos placares oficiais registrados, como os jogos do grupo são exibidos, e como um placar oficial pode ser registrado ou atualizado diretamente na página.

## ADDED Requirements

### Requirement: Navegação por abas entre os 12 grupos

A página Grupos SHALL exibir os 12 grupos da fase de grupos (A–L) como abas de navegação, com um grupo ativo por vez. Ao selecionar uma aba, a página SHALL exibir a classificação e os jogos do grupo correspondente, sem recarregar a página.

#### Scenario: Grupos disponíveis nas abas
- **WHEN** a página Grupos é carregada
- **THEN** as 12 abas (Grupo A a Grupo L) são exibidas, com um grupo ativo selecionado por padrão

#### Scenario: Alternar grupo ativo
- **WHEN** o usuário seleciona a aba do Grupo D
- **THEN** a classificação e os jogos exibidos passam a ser os do Grupo D, sem recarregar a página

### Requirement: Legenda de classificação

A página Grupos SHALL exibir uma legenda indicando o significado dos destaques visuais usados na tabela de classificação: classificado (1º e 2º colocados) e candidato a wildcard (3º colocado).

#### Scenario: Legenda sempre visível
- **WHEN** a página Grupos é carregada, com qualquer grupo ativo
- **THEN** a legenda de "Classificado (1º e 2º)" e "Possível wildcard (3º)" está visível

### Requirement: Classificação oficial calculada a partir dos placares oficiais

A página Grupos SHALL calcular a classificação do grupo ativo exclusivamente a partir dos placares oficiais registrados em `Jogo` para os jogos da fase de grupos daquele grupo. Placares simulados pelos visitantes na página Simulador MUST NOT influenciar essa classificação. Um jogo sem placar oficial registrado SHALL ser tratado como não disputado, não contando nas estatísticas de nenhuma das duas seleções.

Para cada seleção do grupo, a classificação SHALL exibir: posição, seleção, jogos disputados, vitórias, empates, derrotas, gols pró, gols contra, saldo de gols e pontos (3 por vitória, 1 por empate, 0 por derrota).

#### Scenario: Classificação reflete somente resultados oficiais
- **WHEN** um grupo tem jogos com placar oficial registrado e, para as mesmas seleções, existem placares simulados diferentes na página Simulador
- **THEN** a classificação exibida na página Grupos usa apenas os placares oficiais, ignorando os placares simulados

#### Scenario: Jogo sem placar oficial não conta na classificação
- **WHEN** um jogo do grupo ativo ainda não tem placar oficial registrado
- **THEN** esse jogo não é contado nas estatísticas (jogos, vitórias, empates, derrotas, gols) de nenhuma das duas seleções

#### Scenario: Estatísticas completas por seleção
- **WHEN** a classificação do grupo ativo é exibida
- **THEN** cada seleção mostra posição, nome, jogos, vitórias, empates, derrotas, gols pró, gols contra, saldo de gols e pontos

### Requirement: Critérios de desempate da classificação

Quando duas ou mais seleções do grupo empatam em pontos, a classificação SHALL desempatá-las, nesta ordem, usando exclusivamente os jogos oficiais já disputados entre as seleções do grupo: (1) saldo de gols geral no grupo; (2) gols marcados geral no grupo; (3) resultado do confronto direto entre as seleções empatadas (considerando apenas os jogos entre elas); (4) saldo de gols nos confrontos diretos entre as seleções empatadas; (5) posição no Ranking FIFA (melhor posição primeiro). O critério de Fair Play (cartões) previsto nas regras oficiais não é aplicado por não haver dados de cartões disponíveis; o desempate segue direto do saldo de gols no confronto direto para o Ranking FIFA.

#### Scenario: Desempate por saldo de gols geral
- **WHEN** duas seleções do grupo terminam com a mesma pontuação
- **THEN** a seleção com melhor saldo de gols no grupo aparece em posição superior

#### Scenario: Desempate por gols marcados geral
- **WHEN** duas seleções do grupo empatam em pontos e em saldo de gols geral
- **THEN** a seleção com mais gols marcados no grupo aparece em posição superior

#### Scenario: Desempate por confronto direto
- **WHEN** duas seleções do grupo empatam em pontos, saldo de gols geral e gols marcados geral, e já se enfrentaram na fase de grupos
- **THEN** a seleção vencedora do confronto direto entre elas aparece em posição superior

#### Scenario: Desempate por saldo de gols no confronto direto
- **WHEN** duas seleções empatam em pontos, saldo de gols geral, gols marcados geral e o confronto direto entre elas também terminou empatado
- **THEN** a seleção com melhor saldo de gols no(s) jogo(s) entre elas aparece em posição superior

#### Scenario: Desempate final por Ranking FIFA
- **WHEN** duas seleções permanecem empatadas após pontos, saldo de gols geral, gols marcados geral, confronto direto e saldo de gols no confronto direto
- **THEN** a seleção com melhor posição no Ranking FIFA aparece em posição superior

#### Scenario: Desempate final quando uma seleção não consta do Ranking FIFA
- **WHEN** duas seleções permanecem empatadas em todos os critérios anteriores e uma delas não tem posição registrada no Ranking FIFA
- **THEN** a seleção com posição registrada no Ranking FIFA aparece em posição superior

### Requirement: Destaque de classificados e wildcard

A classificação exibida SHALL destacar visualmente as 2 primeiras posições como classificadas e a 3ª posição como candidata a wildcard, distintos entre si e da 4ª posição.

#### Scenario: Primeiro e segundo colocados destacados
- **WHEN** a classificação do grupo ativo é exibida
- **THEN** as seleções nas posições 1 e 2 aparecem com o destaque visual de classificado

#### Scenario: Terceiro colocado destacado como wildcard
- **WHEN** a classificação do grupo ativo é exibida
- **THEN** a seleção na posição 3 aparece com o destaque visual de candidata a wildcard, diferente do destaque de classificado

### Requirement: Jogos do grupo ativo

A página Grupos SHALL exibir todos os jogos da fase de grupos do grupo ativo, ordenados por data e hora, mostrando seleção mandante, seleção visitante, estádio, cidade, data, hora e o placar oficial quando já registrado. Como todos os jogos exibidos já pertencem ao grupo ativo, a página MUST NOT repetir a letra do grupo em cada jogo.

#### Scenario: Jogos do grupo listados
- **WHEN** um grupo é selecionado
- **THEN** todos os jogos da fase de grupos daquele grupo são exibidos, ordenados por data e hora, cada um com sua data e hora completas

#### Scenario: Jogo ainda não disputado
- **WHEN** um jogo do grupo ativo ainda não tem placar oficial registrado
- **THEN** o jogo é exibido sem placar, indicando que ainda não foi disputado

### Requirement: Registro e atualização do placar oficial

A página Grupos SHALL permitir registrar ou atualizar o placar oficial (gols do mandante e do visitante, cada um entre 0 e 30) de qualquer jogo da fase de grupos do grupo ativo, diretamente na página, através de dois campos de placar por jogo. Assim que os dois campos de um jogo estiverem preenchidos, o sistema SHALL persistir os gols no jogo correspondente, recalcular a classificação do grupo e as estatísticas das seleções envolvidas, e refletir imediatamente o resultado atualizado na página, sem exigir recarregá-la nem uma ação separada de "salvar". Se o usuário apagar o conteúdo de um dos campos de um jogo que já tinha placar oficial registrado, o sistema SHALL remover o placar oficial desse jogo (o jogo volta a "sem placar"), recalculando a classificação e as estatísticas imediatamente.

#### Scenario: Registrar placar de um jogo ainda não disputado
- **WHEN** o usuário preenche 2 no campo do mandante e 1 no campo do visitante de um jogo do grupo ativo que ainda não tinha placar oficial
- **THEN** o placar é persistido no jogo assim que os dois campos ficam preenchidos, a classificação do grupo é recalculada e a nova classificação é exibida imediatamente, sem nenhuma ação separada de salvar

#### Scenario: Atualizar placar de um jogo já registrado
- **WHEN** o usuário altera o valor de um dos campos de placar oficial de um jogo que já tinha resultado registrado, mantendo os dois campos preenchidos
- **THEN** o novo placar substitui o anterior no jogo, e a classificação e as estatísticas das seleções envolvidas são recalculadas e exibidas imediatamente

#### Scenario: Apagar o placar de um jogo já registrado
- **WHEN** o usuário apaga o conteúdo de um dos campos de placar de um jogo que já tinha placar oficial registrado
- **THEN** o placar oficial desse jogo é removido (o jogo volta a "sem placar"), e a classificação e as estatísticas das seleções envolvidas são recalculadas imediatamente

#### Scenario: Placar fora da faixa permitida
- **WHEN** o usuário tenta registrar um placar oficial negativo ou maior que 30
- **THEN** o sistema rejeita o registro e o placar oficial do jogo permanece inalterado

#### Scenario: Registro de placar não afeta simulações
- **WHEN** um placar oficial é registrado, atualizado ou removido na página Grupos
- **THEN** as simulações já salvas pelos visitantes na página Simulador permanecem inalteradas

### Requirement: Acesso ao Simulador a partir do grupo ativo

A página Grupos SHALL exibir, para o grupo ativo, um botão "Simular" que direciona o usuário à página Simulador com esse grupo selecionado.

#### Scenario: Botão Simular direciona ao grupo correspondente
- **WHEN** o usuário aciona o botão "Simular" com o Grupo E ativo
- **THEN** a página Simulador é aberta com o Grupo E já selecionado
