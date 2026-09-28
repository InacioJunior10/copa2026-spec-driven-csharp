# jogos-page Specification

## Purpose

Define o comportamento observável da página Jogos: quais jogos são listados, como são ordenados e agrupados, como o filtro por grupo se comporta e quais informações cada jogo exibe.

## Requirements

### Requirement: Listagem completa dos jogos ordenada por data e hora

A página Jogos SHALL listar todos os jogos persistidos (de todas as fases), ordenados cronologicamente por data e hora.

#### Scenario: Todos os jogos aparecem na listagem
- **WHEN** a página Jogos é carregada sem filtro aplicado
- **THEN** os 104 jogos cadastrados são exibidos, na ordem cronológica do jogo de abertura (11/06) à final (19/07)

#### Scenario: Ordenação por data e hora dentro do mesmo dia
- **WHEN** existe mais de um jogo no mesmo dia
- **THEN** esses jogos aparecem em ordem crescente de horário dentro do agrupamento daquele dia

### Requirement: Agrupamento por dia

A página Jogos SHALL agrupar os jogos exibidos por dia da partida, exibindo um cabeçalho de data para cada grupo.

#### Scenario: Cabeçalho de data por dia
- **WHEN** a listagem contém jogos de datas diferentes
- **THEN** cada data distinta aparece com seu próprio cabeçalho, contendo a data e a quantidade de jogos daquele dia, antes dos jogos correspondentes

### Requirement: Filtro por grupo

A página Jogos SHALL permitir filtrar a listagem por um dos grupos da fase de grupos (A–L). Quando um grupo é selecionado, somente os jogos da fase de grupos associados a esse grupo SHALL ser exibidos; jogos de outras fases (sem grupo) MUST NOT aparecer enquanto um filtro de grupo estiver ativo. Quando nenhum filtro está selecionado, a página SHALL exibir os jogos de todas as fases, incluindo os que ainda não têm grupo.

#### Scenario: Filtrar por um grupo específico
- **WHEN** o usuário seleciona o Grupo C no filtro
- **THEN** somente os jogos da fase de grupos do Grupo C são exibidos, agrupados e ordenados por data e hora

#### Scenario: Sem filtro exibe todas as fases
- **WHEN** nenhum grupo está selecionado no filtro
- **THEN** a listagem inclui tanto os jogos da fase de grupos quanto os jogos de mata-mata (sem grupo)

#### Scenario: Grupo sem jogos correspondentes no filtro
- **WHEN** um grupo é selecionado e nenhum jogo da fase de grupos pertence a ele (situação hipotética de dados incompletos)
- **THEN** a página indica que não há jogos para o filtro selecionado, sem erro

### Requirement: Informações exibidas por jogo

Cada jogo exibido na página Jogos SHALL mostrar: seleção mandante e visitante (ou a descrição da vaga, quando a seleção ainda não está definida), o grupo (quando o jogo pertencer à fase de grupos), o estádio, a cidade, a data, a hora e o placar oficial quando o jogo já tiver sido disputado.

#### Scenario: Jogo da fase de grupos com informações completas
- **WHEN** o jogo de abertura é exibido
- **THEN** mostra México x África do Sul, Grupo A, o estádio e a cidade, 11/06/2026 às 16:00 (horário de Brasília), sem placar oficial

#### Scenario: Jogo de mata-mata sem seleções definidas
- **WHEN** um jogo de mata-mata cujas seleções ainda não foram definidas é exibido
- **THEN** mostra a descrição das vagas (ex.: "Venc. Oitavas 1") no lugar do nome da seleção, sem grupo associado

#### Scenario: Jogo já disputado exibe o placar oficial
- **WHEN** um jogo com placar oficial registrado é exibido
- **THEN** o placar oficial das duas seleções é mostrado junto às informações do jogo

### Requirement: Acesso à página Grupos

A página Jogos SHALL exibir um botão "Ver Grupos" que direciona o usuário à página Grupos.

#### Scenario: Botão sempre visível
- **WHEN** a página Jogos é carregada, com ou sem filtro aplicado
- **THEN** o botão "Ver Grupos" está visível e aponta para a rota da página Grupos
