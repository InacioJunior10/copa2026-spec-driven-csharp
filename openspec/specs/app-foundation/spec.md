# app-foundation Specification

## Purpose
Provê o esqueleto executável do PortalCopa26 como Blazor Web App em projeto único, com organização interna preparada para futura separação em camadas e com os serviços de dados disponíveis para as páginas que serão construídas nas próximas mudanças.

## Requirements

### Requirement: Aplicação executável
O sistema SHALL compilar e iniciar como uma aplicação web única, respondendo a requisições HTTP mesmo sem nenhuma página de domínio implementada.

#### Scenario: Aplicação sobe em ambiente limpo
- **WHEN** a aplicação é iniciada em uma máquina sem banco de dados pré-existente
- **THEN** ela inicia sem erros e a página inicial responde com sucesso (HTTP 200)

#### Scenario: Suporte a componentes interativos
- **WHEN** uma página futura declarar interatividade no servidor (necessária para o Simulador e para interoperabilidade com JavaScript, como o gráfico do Ranking)
- **THEN** a aplicação já possui a interatividade no servidor habilitada, sem mudança de configuração

### Requirement: Organização preparada para camadas
O sistema SHALL manter, dentro do projeto único, uma separação explícita entre entidades de domínio, acesso a dados e serviços da aplicação, de modo que as entidades de domínio não dependam do mecanismo de persistência.

#### Scenario: Entidades independentes da persistência
- **WHEN** as entidades de domínio são inspecionadas
- **THEN** elas não referenciam tipos ou atributos do mecanismo de persistência; todo o mapeamento fica na área de acesso a dados

#### Scenario: Áreas identificáveis
- **WHEN** um desenvolvedor inspeciona a estrutura do projeto
- **THEN** entidades de domínio, acesso a dados (contexto, mapeamentos, migrations, seed) e registro de serviços estão em pastas/namespaces distintos

### Requirement: Serviços de dados via injeção de dependência
O sistema SHALL registrar, na inicialização, o acesso ao banco de dados de forma adequada a componentes interativos de longa duração, permitindo que qualquer componente ou serviço futuro obtenha acesso aos dados sem configuração adicional.

#### Scenario: Componente obtém acesso aos dados
- **WHEN** um componente ou serviço solicita acesso ao banco de dados via injeção de dependência
- **THEN** recebe um meio de criar um contexto de dados de vida curta, funcional e isolado de outros usuários conectados
