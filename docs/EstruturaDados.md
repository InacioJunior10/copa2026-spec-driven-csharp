# Estrutura de Dados

## Objetivo

Este documento descreve as principais entidades do PortalCopa26 e as decisões de modelagem adotadas pelo projeto.

---

## Entidades Principais

### Grupo

Representa um dos grupos da Competição

---

### Selecao

Representa uma seleção participante da competição

---

### EstadioSede

Representa um estádio utilizado na competição.

Contém:

* Nome
* Cidade
* País

---

### Jogo

Representa um jogo oficial da Copa.

Responsável por armazenar:

* Seleção mandante
* Seleção visitante
* Data e horário
* Grupo
* Fase
* Estádio
* Cidade
* GolsMandante
* GolsVisitante

Importante:

Os campos GolsMandante e GolsVisitante representam os resultados oficiais do torneio.

---

### RankingFifa

Representa a posição oficial de cada seleção no ranking FIFA.

---

### Simulacao

Representa uma simulação persistida pelo sistema.

Atualmente existe uma única simulação ativa.

---

### SimulacaoJogo

Representa um resultado simulado.

Contém:

* SimulacaoId
* JogoId
* GolsMandante
* GolsVisitante

---

## Regras de Modelagem

### Resultados Oficiais

Os resultados oficiais pertencem à entidade Jogo.

Esses resultados representam a competição oficial.

---

### Resultados Simulados

Os resultados simulados pertencem à entidade SimulacaoJogo.

Esses resultados representam apenas cenários hipotéticos.

---

### Classificações

Não devem existir tabelas persistidas para classificação.

A classificação deve ser calculada dinamicamente a partir dos resultados.

Exemplos:

* Classificação oficial → Jogos
* Classificação simulada → SimulacaoJogos

---

## Serviços Esperados

Os cálculos devem ser centralizados em serviços.

Exemplos:

* JogosService
* GruposService
* RankingService
* SimuladorService
* ClassificacaoService

Evitar duplicação da lógica de classificação em páginas ou componentes Razor.
