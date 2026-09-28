# data-persistence Specification

## Purpose
Persiste os dados da Copa do Mundo 2026 (grupos, seleções, jogadores, jogos de todas as fases, ranking FIFA) e as simulações dos visitantes em SQLite, com carga inicial idempotente a partir dos dados validados no protótipo.

## Requirements

### Requirement: Modelo de dados da Copa 2026
O sistema SHALL persistir as entidades `Grupo`, `Selecao`, `Jogador`, `Jogo`, `RankingFifa`, `Simulacao` e `SimulacaoJogo`, contendo no mínimo:
- Grupo: letra (A–L) e suas seleções.
- Seleção: código FIFA de 3 letras (único), nome em português, técnico, grupo e indicação de cabeça de chave.
- Jogador: nome, posição (goleiro, defensor, meio-campista, atacante), idade, gols pela seleção e participações em Copas (quando conhecido) e sua seleção.
- Jogo: número do jogo, fase, grupo (apenas na fase de grupos), data e hora (horário de Brasília), estádio, cidade, seleção mandante e visitante e placar oficial.
- Ranking FIFA: posição, seleção (código e nome) e pontuação.
- Simulação: identificador do visitante dono da simulação, datas de criação/atualização e os placares simulados por jogo.

#### Scenario: Consulta de seleção com elenco e grupo
- **WHEN** a seleção de código "BRA" é consultada com seu grupo e jogadores
- **THEN** retorna "Brasil", grupo "C", o técnico e a lista de jogadores com posição, idade e gols

#### Scenario: Participações em Copas desconhecidas
- **WHEN** um jogador não possui a informação de participações em Copas na fonte de dados
- **THEN** o campo é armazenado como desconhecido (vazio), e não como zero

### Requirement: Jogos de todas as fases com confrontos a definir
O sistema SHALL armazenar os jogos de todas as fases (fase de grupos, segunda fase, oitavas, quartas, semifinais, disputa de 3º lugar e final), permitindo que a seleção mandante e/ou visitante ainda não esteja definida; nesse caso, o jogo SHALL guardar a descrição da vaga (ex.: "Venc. Oitavas 1").

#### Scenario: Jogo de mata-mata sem seleções definidas
- **WHEN** a final é consultada
- **THEN** retorna data 19/07, estádio em Nova York/Nova Jersey, sem seleções definidas e com as vagas "Venc. Semifinal 1" e "Venc. Semifinal 2"

#### Scenario: Jogo da fase de grupos
- **WHEN** o jogo de abertura é consultado
- **THEN** retorna México x África do Sul, Grupo A, 11/06/2026 às 16:00 (horário de Brasília), sem placar oficial

#### Scenario: Ordenação por data
- **WHEN** os jogos são consultados ordenados por data e hora
- **THEN** a ordem é cronológica, do jogo de abertura (11/06) à final (19/07)

### Requirement: Persistência em SQLite com esquema versionado
O sistema SHALL persistir os dados em um arquivo SQLite local, criando o banco e aplicando as alterações de esquema pendentes automaticamente na inicialização.

#### Scenario: Primeira execução
- **WHEN** a aplicação inicia e o arquivo de banco não existe
- **THEN** o arquivo é criado com o esquema completo das 7 entidades

#### Scenario: Banco existente
- **WHEN** a aplicação inicia com um banco já criado e sem alterações de esquema pendentes
- **THEN** os dados existentes são preservados

### Requirement: Carga inicial idempotente
O sistema SHALL popular, na inicialização, um banco vazio com os dados da Copa 2026 derivados do protótipo validado: 12 grupos, 48 seleções, os elencos das 48 seleções, 104 jogos e o ranking FIFA completo da fonte. Execuções seguintes MUST NOT duplicar nem sobrescrever dados.

#### Scenario: Banco vazio é populado
- **WHEN** a aplicação inicia com o banco vazio
- **THEN** existem 12 grupos, 48 seleções (4 por grupo), 104 jogos (72 na fase de grupos), todas as seleções com elenco não vazio e o ranking FIFA completo

#### Scenario: Reinício não duplica
- **WHEN** a aplicação é reiniciada com o banco já populado
- **THEN** a quantidade de registros de cada entidade permanece a mesma

#### Scenario: Integridade das referências
- **WHEN** a carga inicial é concluída
- **THEN** todo jogo da fase de grupos referencia um grupo e duas seleções desse mesmo grupo

### Requirement: Simulações independentes dos resultados oficiais
O sistema SHALL persistir placares simulados em registros de simulação próprios, vinculados a um identificador anônimo de visitante, sem nunca alterar o placar oficial do jogo. Cada simulação MUST conter no máximo um placar por jogo, e placares simulados MUST ser inteiros entre 0 e 30.

#### Scenario: Simular não altera o jogo oficial
- **WHEN** um placar simulado é salvo para um jogo
- **THEN** o placar oficial do jogo permanece inalterado

#### Scenario: Simulações de visitantes diferentes são isoladas
- **WHEN** dois visitantes simulam placares diferentes para o mesmo jogo
- **THEN** cada simulação mantém o seu próprio placar

#### Scenario: Placar repetido na mesma simulação
- **WHEN** uma simulação tenta registrar um segundo placar para um jogo que já possui placar nela
- **THEN** a persistência é rejeitada

#### Scenario: Placar fora da faixa
- **WHEN** um placar simulado negativo ou maior que 30 é salvo
- **THEN** a persistência é rejeitada

### Requirement: Atualização do placar oficial após a carga inicial

O sistema SHALL permitir criar, atualizar ou remover, após a carga inicial, o placar oficial (`GolsMandante`/`GolsVisitante`) de um jogo da fase de grupos, mantendo os placares simulados dos visitantes (`SimulacaoJogo`) inalterados. O placar oficial informado MUST estar entre 0 e 30 para cada seleção. Remover o placar oficial de um jogo SHALL deixar `GolsMandante` e `GolsVisitante` novamente nulos, equivalente ao estado antes de qualquer registro.

#### Scenario: Placar oficial passa de não registrado para registrado
- **WHEN** o placar oficial de um jogo da fase de grupos que ainda não tinha resultado é registrado
- **THEN** o jogo passa a ter `GolsMandante` e `GolsVisitante` persistidos, sem alterar nenhuma simulação existente

#### Scenario: Placar oficial já registrado é atualizado
- **WHEN** o placar oficial de um jogo que já tinha resultado registrado é atualizado para um novo valor
- **THEN** o jogo passa a refletir o novo placar, sem alterar nenhuma simulação existente

#### Scenario: Placar oficial já registrado é removido
- **WHEN** o placar oficial de um jogo que já tinha resultado registrado é removido
- **THEN** `GolsMandante` e `GolsVisitante` do jogo voltam a nulo, sem alterar nenhuma simulação existente

#### Scenario: Placar oficial fora da faixa é rejeitado
- **WHEN** um placar oficial negativo ou maior que 30 é enviado para persistência
- **THEN** a persistência é rejeitada e o placar oficial anterior do jogo é mantido
