/opsx:propose Contexto:
Já existe um protótipo HTML validado na pasta D:\Desenvolvimento\IA\copa2026\prototipo
e um arquivo CLAUDE.md na raiz do projeto contendo as decisões
arquiteturais e os requisitos globais do projeto.

Objetivo:
Criar a fundação da aplicação utilizando Blazor Web App (.NET 10), EF Core e SQLite.

Escopo desta especificação:
- Criar a solução PortalCopa26
- Configurar Blazor Web App
- Configurar EF Core
- Configurar SQLite
- Definir a estrutura inicial do projeto
- Criar o DbContext da aplicação
- Criar as entidades iniciais:
  - Grupo
  - Selecao
  - Jogador
  - Jogo
  - RankingFifa
  - Simulacao
  - SimulacaoJogo
- Configurar o SeedData popular
- Configurar a injeção de dependência necessária
- Preparar a aplicação para futuras funcionalidades

Requisitos importantes:

- A aplicação utilizará arquitetura simplificada em projeto único
- O código deve permitir futura migração para arquitetura em camadas
- O Ranking FIFA será exibido futuramente através de Chart.js usando JSInterop
- As simulações deverão ser persistidas usando SQLite
- Os dados iniciais da Copa serão carregados através de SeedData

Fora do escopo desta especificação:

- Implementação da Landing Page
- Implementação da página de Jogos
- Implementação da página dos Grupos
- Implementação da página das Seleções
- Implementação do Ranking FIFA
- Implementação do Simulador
- Integração com APIs externas
- Área administrativa
- Autenticação
