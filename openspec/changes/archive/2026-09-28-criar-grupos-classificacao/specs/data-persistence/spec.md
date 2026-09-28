# Spec Delta

## ADDED Requirements

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
