# Spec Delta

## ADDED Requirements

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
