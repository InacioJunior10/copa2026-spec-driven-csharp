# simulador-page Specification

## Purpose

Define o comportamento observável da página Simulador: como o visitante informa placares da fase de grupos, como a classificação simulada é calculada e destacada, e como a simulação é persistida e restaurada automaticamente entre visitas.

## Requirements

### Requirement: Simulação restrita à fase de grupos

O Simulador SHALL permitir informar placares apenas para os jogos da fase de grupos. Jogos de outras fases (mata-mata) MUST NOT ser exibidos nem simuláveis nesta página.

#### Scenario: Apenas jogos de grupo aparecem no simulador
- **WHEN** o visitante seleciona um grupo no Simulador
- **THEN** somente os jogos da fase de grupos daquele grupo são exibidos para simulação

### Requirement: Entrada de placar por jogo

O Simulador SHALL permitir informar o placar (gols do mandante e do visitante, cada um entre 0 e 30) de cada jogo do grupo selecionado. Um jogo sem placar informado SHALL ser tratado como não simulado, não como 0 a 0.

#### Scenario: Placar informado
- **WHEN** o visitante informa 2 a 1 para um jogo do grupo ativo
- **THEN** o jogo passa a contar com esse placar na classificação simulada do grupo

#### Scenario: Jogo sem placar não conta na classificação
- **WHEN** um jogo do grupo ativo ainda não tem placar informado pelo visitante
- **THEN** esse jogo não é contado nas estatísticas (jogos, vitórias, empates, derrotas, gols) de nenhuma das duas seleções na classificação simulada

### Requirement: Classificação recalculada automaticamente

Ao informar ou alterar um placar, o Simulador SHALL recalcular automaticamente a classificação do grupo correspondente, exibindo para cada seleção: jogos, vitórias, empates, derrotas, saldo de gols e pontos (3 por vitória, 1 por empate, 0 por derrota). A ordenação SHALL seguir, em ordem de prioridade: pontos, depois saldo de gols, depois gols marcados.

#### Scenario: Recalcula ao alterar um placar
- **WHEN** o visitante altera o placar de um jogo já simulado
- **THEN** a classificação do grupo é recalculada e exibida imediatamente, sem exigir uma ação separada de "salvar" ou "calcular"

#### Scenario: Desempate por saldo de gols
- **WHEN** duas seleções do grupo terminam a simulação com a mesma pontuação
- **THEN** a seleção com melhor saldo de gols aparece em posição superior na classificação

#### Scenario: Desempate por gols marcados
- **WHEN** duas seleções do grupo empatam em pontos e em saldo de gols
- **THEN** a seleção com mais gols marcados aparece em posição superior na classificação

### Requirement: Destaque de classificados e wildcard

A classificação simulada de cada grupo SHALL destacar visualmente as 2 primeiras posições como classificadas e a 3ª posição como candidata a wildcard, distintos entre si e da 4ª posição.

#### Scenario: Primeiro e segundo colocados destacados como classificados
- **WHEN** a classificação simulada de um grupo é exibida
- **THEN** as seleções nas posições 1 e 2 aparecem com o destaque visual de classificado

#### Scenario: Terceiro colocado destacado como wildcard
- **WHEN** a classificação simulada de um grupo é exibida
- **THEN** a seleção na posição 3 aparece com o destaque visual de candidata a wildcard, diferente do destaque de classificado

### Requirement: Persistência automática da simulação

O Simulador SHALL persistir automaticamente, no banco de dados, cada placar informado pelo visitante, sem exigir uma ação explícita de salvar nem um nome para a simulação. A simulação SHALL ser identificada por um visitante anônimo, sem exigir login.

#### Scenario: Placar persiste sem ação de salvar
- **WHEN** o visitante informa um placar e não realiza nenhuma outra ação
- **THEN** o placar já está persistido no banco de dados para aquele visitante

#### Scenario: Simulação sem nome
- **WHEN** o visitante usa o Simulador pela primeira vez
- **THEN** nenhuma tela ou campo pede um nome para a simulação antes de permitir informar placares

### Requirement: Restauração automática da simulação

Ao carregar a página Simulador, o Simulador SHALL restaurar automaticamente os placares previamente persistidos para o visitante, exibindo a classificação já recalculada com base neles.

#### Scenario: Simulação restaurada ao retornar
- **WHEN** o visitante que já simulou placares anteriormente recarrega ou retorna à página Simulador
- **THEN** os placares informados anteriormente aparecem preenchidos e a classificação reflete esses placares, sem nenhuma ação do visitante

#### Scenario: Primeira visita sem simulação prévia
- **WHEN** um visitante sem nenhuma simulação anterior abre a página Simulador
- **THEN** todos os jogos aparecem sem placar, e a classificação de cada grupo mostra todas as seleções zeradas

### Requirement: Limpar placares do grupo ou de toda a simulação

O Simulador SHALL permitir remover todos os placares simulados do grupo ativo ("Limpar Grupo") ou todos os placares simulados do visitante em todos os grupos ("Limpar Tudo"), recalculando a classificação afetada imediatamente após a limpeza.

#### Scenario: Limpar grupo afeta somente o grupo ativo
- **WHEN** o visitante aciona "Limpar Grupo" com o Grupo C ativo e placares simulados em outros grupos
- **THEN** os placares do Grupo C são removidos e a classificação do Grupo C volta ao estado sem jogos simulados, enquanto os placares dos outros grupos permanecem inalterados

#### Scenario: Limpar tudo remove toda a simulação do visitante
- **WHEN** o visitante aciona "Limpar Tudo"
- **THEN** todos os placares simulados de todos os grupos daquele visitante são removidos

### Requirement: Informações do grupo ativo

O Simulador SHALL exibir, para o grupo selecionado, as 4 seleções participantes com sua posição no ranking FIFA (quando disponível).

#### Scenario: Seleções do grupo exibidas com ranking
- **WHEN** um grupo é selecionado no Simulador
- **THEN** as 4 seleções desse grupo aparecem listadas, cada uma com sua posição no ranking FIFA quando essa seleção constar do ranking persistido

### Requirement: Seleção inicial de grupo via parâmetro de URL

O Simulador SHALL aceitar um grupo inicial informado por parâmetro de URL. Quando presente e válido, o grupo indicado SHALL ser selecionado como grupo ativo ao carregar a página, em vez do primeiro grupo disponível. Quando ausente ou inválido, o comportamento padrão (primeiro grupo disponível) SHALL ser mantido.

#### Scenario: Grupo informado na URL é selecionado
- **WHEN** a página Simulador é aberta com o Grupo E indicado no parâmetro de URL
- **THEN** o Grupo E aparece selecionado como grupo ativo, com seus jogos e classificação exibidos

#### Scenario: Parâmetro ausente mantém o comportamento padrão
- **WHEN** a página Simulador é aberta sem parâmetro de grupo na URL
- **THEN** o primeiro grupo disponível é selecionado como grupo ativo, como antes desta mudança

#### Scenario: Parâmetro inválido é ignorado
- **WHEN** a página Simulador é aberta com um valor de grupo que não corresponde a nenhum grupo existente
- **THEN** o primeiro grupo disponível é selecionado como grupo ativo, sem erro
