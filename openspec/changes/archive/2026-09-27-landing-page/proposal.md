# Proposal

## Why

A fundação do PortalCopa26 (solução Blazor, EF Core/SQLite, entidades e SeedData) está pronta, mas a aplicação ainda não tem nenhuma página — só o template padrão do Blazor. A Home é o ponto de entrada do portal (PRD, seção 4/5) e é o primeiro lugar onde o usuário vê dados reais da Copa 2026 (estatísticas, próximos jogos, ranking FIFA) e é direcionado ao Simulador. Sem ela, a fundação não tem nenhuma prova visual de que persiste e expõe os dados corretamente.

## What Changes

- Substitui a página padrão do template (`Home.razor`) pela Landing Page do PortalCopa26, composta a partir de 5 componentes reutilizáveis em `Components/Pages/LandingPage/`: `HeroSection`, `EstatisticasCopa`, `ProximosJogos`, `RankingFifaChart`, `SimuladorPainel`.
- Hero Section: título, chamada para o Simulador, chamada para Jogos e os 3 países-sede (Estados Unidos, Canadá, México) com bandeira e quantidade de estádios — conteúdo estático, sem consulta ao banco (dado fixo do PRD/protótipo).
- Estatísticas da Copa: 4 números vindos do banco (seleções, grupos, jogos, estádios distintos).
- Próximos Jogos: jogos ainda sem placar oficial, agrupados por dia, cobrindo os 2 dias mais próximos com jogo agendado.
- Ranking FIFA (Top 10): gráfico de barras horizontais renderizado com Chart.js via JSInterop, com os 10 melhores colocados do banco.
- Chamada para o Simulador: bloco de destaque com link para a futura página do Simulador (fora do escopo desta mudança — a rota ainda não existe).
- Cria uma camada de serviços (`Services/`) para os dados da Landing Page, consumida pelos componentes via injeção de dependência — nenhum componente Razor consulta o `AppDbContext` diretamente.
- Cria um wrapper de interoperabilidade JS para Chart.js pensado para reaproveitamento por outros gráficos futuros do portal (ex.: Ranking completo, Grupos), não só o desta mudança.
- Estiliza a página com Bootstrap 5 (já referenciado no template) combinado com os tokens visuais do protótipo (paleta escura, dourado de destaque), para manter a identidade visual validada sem reintroduzir o CSS do protótipo como está.

## Capabilities

### New Capabilities
- `landing-page`: comportamento observável da página inicial do portal — o que cada seção exibe, de onde vêm os dados e quando algo aparece vazio/indisponível.

### Modified Capabilities
(nenhuma — `app-foundation` e `data-persistence` já expõem tudo que esta mudança consome; nenhum requisito existente muda de comportamento)

## Impact

- Novo código: `Components/Pages/LandingPage/*.razor`, `Services/` (interface + implementação dos dados da Landing Page), um serviço/módulo JS de interoperabilidade para Chart.js.
- `Components/Pages/Home.razor` é substituído pela composição dos 5 componentes.
- `wwwroot/`: adiciona a biblioteca Chart.js (vendorizada, no mesmo padrão do Bootstrap já incluído pelo template) e o módulo JS de interop.
- Consome exclusivamente dados já persistidos por `data-persistence` (nenhuma migration ou mudança de schema).
- Sem integração com APIs externas nesta mudança, exceto as URLs públicas de bandeira/logo da FIFA já usadas como imagens estáticas (mesmo padrão do protótipo e do PRD, seção 11) — não é uma chamada de API feita pelo servidor, só `src` de `<img>`.
- Fora do escopo: páginas de Jogos, Grupos, Equipes, Ranking completo e Simulador; navegação avançada; responsividade além do que o Bootstrap grid já oferece.
