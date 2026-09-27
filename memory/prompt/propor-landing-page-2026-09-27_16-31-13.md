/opsx:propose Contexto:

A fundação do projeto PortalCopa26 já foi concluída.

O banco SQLite já está configurado.
O EF Core já está configurado.
As entidades e SeedData já existem.

Objetivo:

Implementar a Landing Page principal baseada no protótipo HTML validado.
Utilizar o protótipo localizado na pasta ..\prototipo como referência visual principal.
Analisar os arquivos HTML, CSS e JavaScript existentes na pasta ..\prototipo.

Escopo:

* Hero Section
* Estatísticas da Copa
* Próximos Jogos
* Ranking FIFA (Top 10)
* Gráfico Chart.js via JSInterop
* Chamada para o Simulador

Estrutura dos Componentes:
Criar os componentes reutilizáveis na pasta:

Components/Pages/LandingPage

Componentes obrigatórios:

* HeroSection.razor
* EstatisticasCopa.razor
* ProximosJogos.razor
* RankingFifaChart.razor
* SimuladorPainel.razor

A página inicial deverá ser composta através desses componentes.
Evitar concentrar toda a implementação em uma única página Razor.

Serviços:

Criar serviços específicos para acesso aos dados da Landing Page.
Utilizar injeção de dependência.
Evitar consultas EF Core diretamente nos componentes Razor.

Chart.js:

Implementar o gráfico Ranking FIFA utilizando:

* Chart.js
* JSInterop

A implementação deve permitir reutilização futura para outros gráficos estatísticos do portal.

Requisitos:

* Utilizar componentes Blazor reutilizáveis
* Utilizar Bootstrap 5
* Utilizar os dados do banco SQLite
* Utilizar serviços para acesso aos dados
* Utilizar Chart.js via JSInterop
* Seguir as diretrizes definidas no CLAUDE.md

Fora do Escopo:

* Página Jogos
* Página Grupos
* Página Seleções
* Página Ranking Completo
* Página Simulador
* Navegação avançada
* Responsividade avançada
* Implementação do simulador
