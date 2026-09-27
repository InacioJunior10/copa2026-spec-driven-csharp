# Regras Copa 2026

## Objetivo

Este documento define as regras de negócio da Copa do Mundo FIFA 2026 utilizadas pelo PortalCopa26.

---

## Estrutura da Competição

* 48 seleções participantes
* 12 grupos (A até L)
* 4 seleções por grupo
* 72 jogos na fase de grupos
* 102 jogos no total

---

## Fase de Grupos

Cada grupo possui 4 seleções.

Cada seleção realiza 3 partidas.

---

## Classificação dos Grupos

A classificação deve ser calculada a partir dos resultados dos jogos.

Pontuação:

* Vitória = 3 pontos
* Empate = 1 ponto
* Derrota = 0 ponto

Exibir:

* Jogos
* Vitórias
* Empates
* Derrotas
* Gols Pró
* Gols Contra
* Saldo de Gols
* Pontos

---

## Critérios de Desempate

Ordem de aplicação:

1. Pontos
2. Saldo de gols
3. Gols marcados
4. Confronto direto
5. Saldo de gols nos confrontos diretos
6. Fair Play
7. Ranking FIFA

Observação:

A primeira versão da aplicação poderá implementar apenas os três primeiros critérios.

---

## Classificação para o Mata-Mata

Classificam-se:

* Os 2 primeiros colocados de cada grupo
* Os 8 melhores terceiros colocados

Total:

* 32 seleções classificadas

---

## Resultados Oficiais

Os resultados oficiais são a única fonte de verdade para:

- Classificação dos grupos
- Estatísticas oficiais
- Exibição dos resultados na página Jogos
- Definição dos classificados para as fases eliminatórias

---

## Simulações

Os resultados simulados nunca devem alterar:

- Jogos oficiais
- Classificação oficial
- Estatísticas oficiais

As classificações geradas pelo simulador devem utilizar apenas os resultados simulados.

---

## Mata-Mata

Fases Eliminatórias (Mata-Mata)

Após a fase de grupos, a competição passa a ser disputada em partidas eliminatórias.

As fases eliminatórias da Copa do Mundo de 2026 são:

Segunda Fase
Oitavas de Final
Quartas de Final
Semifinais
Disputa do Terceiro Lugar
Final

## Formação dos Confrontos

Cada fase é formada pelos vencedores da fase anterior.

Os confrontos não devem armazenar seleções fixas, mas sim referências aos jogos de origem.

Quando o resultado de um jogo eliminatório for alterado:

o vencedor deve ser recalculado;
os confrontos das fases seguintes devem ser atualizados automaticamente.
Resultado do Jogo

Nas fases eliminatórias não pode existir empate.

Um jogo eliminatório pode terminar de três formas:

Vitória no tempo regulamentar;
Vitória na prorrogação;
Vitória nos pênaltis.

Prorrogação

Se o placar estiver empatado ao final do tempo regulamentar:

a partida será disputada em uma prorrogação de dois tempos de 15 minutos.

Pênaltis

Persistindo o empate após a prorrogação:

A classificação será decidida por disputa de pênaltis.

Determinação do Vencedor

Todo jogo eliminatório, exceto a Final e a disputa do Terceiro Lugar, deve produzir um classificado para a próxima fase.

