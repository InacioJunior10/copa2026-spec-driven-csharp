# Spec Delta

## Purpose

Define o comportamento observável da página inicial (Landing Page) do PortalCopa26: o que cada seção exibe, de onde vêm os dados persistidos e como a página se comporta quando algum dado está ausente ou incompleto.

## ADDED Requirements

### Requirement: Hero com logo da Copa e países-sede
A Landing Page SHALL exibir uma seção de destaque com o logo da Copa do Mundo FIFA 2026, o nome do portal, uma chamada para o Simulador, uma chamada para a listagem de Jogos, e os 3 países-sede da Copa 2026 (Estados Unidos, Canadá, México), cada um com sua bandeira e a quantidade de estádios sediando jogos naquele país.

#### Scenario: Logo e países-sede sempre presentes
- **WHEN** a Landing Page é carregada
- **THEN** o logo da Copa e os 3 países-sede aparecem na seção de destaque, cada país-sede com nome e quantidade de estádios, independentemente do estado do banco de dados

### Requirement: Estatísticas da Copa a partir do banco
A Landing Page SHALL exibir a quantidade total de seleções, grupos, jogos e estádios distintos, calculada a partir dos dados persistidos no banco.

#### Scenario: Estatísticas refletem os dados semeados
- **WHEN** a Landing Page é carregada com o banco populado pela carga inicial
- **THEN** exibe 48 seleções, 12 grupos, 104 jogos e a quantidade de estádios distintos usados nos jogos cadastrados

### Requirement: Próximos jogos sem placar oficial
A Landing Page SHALL exibir os jogos que ainda não têm placar oficial, agrupados por dia, cobrindo os 2 dias mais próximos (em ordem cronológica) que tenham ao menos um jogo agendado. Cada jogo exibido SHALL mostrar as seleções mandante e visitante (ou a vaga, quando ainda não definidas), data, hora, estádio, cidade e grupo (quando aplicável).

#### Scenario: Exibe os 2 próximos dias com jogos
- **WHEN** a Landing Page é carregada e existem jogos sem placar oficial
- **THEN** exibe os jogos agrupados pelos 2 dias cronologicamente mais próximos que possuem jogo sem placar oficial, e nenhum jogo com placar oficial já definido aparece nessa seção

#### Scenario: Nenhum jogo pendente
- **WHEN** todos os jogos cadastrados já têm placar oficial
- **THEN** a seção de próximos jogos indica que não há jogos futuros, sem erro

### Requirement: Ranking FIFA (Top 10)
A Landing Page SHALL exibir um gráfico com as 10 seleções mais bem posicionadas do ranking FIFA persistido, ordenadas pela posição, cada uma com nome e pontuação.

#### Scenario: Top 10 do ranking
- **WHEN** a Landing Page é carregada com o ranking FIFA populado
- **THEN** o gráfico mostra exatamente as 10 seleções com as melhores posições (menor número de posição), na ordem do ranking

#### Scenario: Ranking com menos de 10 registros
- **WHEN** o ranking FIFA persistido tem menos de 10 registros
- **THEN** o gráfico mostra todos os registros existentes, sem erro nem espaços vazios

### Requirement: Chamada para o Simulador
A Landing Page SHALL exibir um bloco de destaque convidando o usuário a simular a Copa, com um link para a página do Simulador.

#### Scenario: Bloco de chamada sempre visível
- **WHEN** a Landing Page é carregada
- **THEN** o bloco de chamada para o Simulador está visível, independentemente do estado do banco de dados
