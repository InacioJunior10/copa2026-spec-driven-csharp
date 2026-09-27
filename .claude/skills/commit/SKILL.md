---
name: commit
description: Generate a Conventional Commits-formatted commit message that summarizes the changes made on the current git branch (staged changes, working tree changes, or the full branch diff against its base). Use when the user asks to write/generate a commit message, describe what changed on the branch, or says "/commit", "gerar commit", "mensagem de commit". Can also perform the commit itself if the user confirms.
---

Gerar uma descrição de commit no padrão **Conventional Commits**, resumindo o que foi realizado na branch atual, e opcionalmente executar o commit.

## Passo 1 — Coletar o contexto das mudanças

Execute em paralelo:

```bash
git status
git diff --cached
git diff
git status --porcelain=v1 --untracked-files=all
```

Depois, identifique a branch base para comparação (`main` ou `master`, o que existir) e, se houver commits na branch atual além da base, obtenha também o resumo completo da branch:

```bash
git merge-base <base> HEAD
git diff <base>...HEAD
git log <base>..HEAD --oneline
```

Regras de prioridade sobre qual diff resume o commit:

1. Se existirem mudanças **staged** (`git diff --cached` não vazio), a descrição deve resumir apenas o que está staged — é isso que será commitado.
2. Se não houver nada staged, mas houver mudanças no working tree (modificadas ou não rastreadas), resuma essas mudanças.
3. Se o working tree estiver limpo mas a branch tiver commits ainda não mesclados na base, resuma o diff acumulado da branch (`<base>...HEAD`) — é o cenário de "descrever o que foi feito na branch".

Nunca invente mudanças que não aparecem nos diffs. Se não houver nenhuma mudança em nenhum dos três cenários, informe o usuário e pare.

## Passo 2 — Classificar o tipo (type)

Baseado nos arquivos e no conteúdo alterado, escolha um `type` do Conventional Commits:

- `feat`: nova funcionalidade visível para o usuário/consumidor do código
- `fix`: correção de bug
- `docs`: apenas documentação (`*.md`, `CLAUDE.md`, comentários)
- `style`: formatação, espaçamento, lint — sem mudança de comportamento
- `refactor`: reestruturação de código sem mudar comportamento externo
- `perf`: melhoria de performance
- `test`: adição/ajuste de testes
- `build`: build system, dependências, empacotamento
- `ci`: pipelines/configuração de integração contínua
- `chore`: manutenção geral que não se encaixa nos anteriores (config, scripts, memória, `.gitignore`)
- `revert`: desfaz um commit anterior

Se as mudanças cobrirem mais de um tipo, escolha o tipo predominante para o `type` principal e mencione os demais no corpo da mensagem. Se a mudança quebra compatibilidade, adicione `!` após o type/scope (ex: `feat!:`) e um rodapé `BREAKING CHANGE: <descrição>`.

## Passo 3 — Determinar o scope (opcional)

Derive o `scope` do diretório/módulo predominante alterado (ex: `simulador`, `jogos`, `grupos`, `equipes`, `ranking`, `seed`, `memory`, `skills`). Omita o scope se as mudanças forem transversais a muitas áreas sem um módulo claro.

## Passo 4 — Montar a mensagem

Formato:

```
<type>[(scope)][!]: <resumo curto, em português, no imperativo, sem ponto final>

- <bullet resumindo mudança relevante 1>
- <bullet resumindo mudança relevante 2>
- ...

[BREAKING CHANGE: <descrição>]
```

Regras:
- Linha de assunto com no máximo ~72 caracteres.
- Resumo no imperativo (ex: "adiciona", "corrige", "remove"), minúsculo após os dois-pontos, sem ponto final.
- Corpo em bullets, cada um resumindo uma mudança concreta e observável (não parafraseie o diff linha a linha, agrupe por intenção).
- Não inclua nomes de arquivos irrelevantes; cite arquivos/módulos só quando ajudar a entender o escopo.
- Idioma: português, salvo se o usuário pedir em inglês.

## Passo 5 — Apresentar e confirmar

Mostre a mensagem gerada ao usuário. Não execute `git add`/`git commit` automaticamente — pergunte se deseja que o commit seja realizado com essa mensagem.

Se o usuário confirmar:
1. Se havia mudanças staged, comite apenas o que já está staged (não adicione mais nada).
2. Se não havia nada staged, pergunte quais arquivos deseja incluir (ou confirme se é para adicionar todos os arquivos modificados/novos relevantes) antes de rodar `git add` com caminhos específicos — nunca `git add -A`/`git add .` sem confirmação.
3. Rode `git commit -m "$(cat <<'EOF'\n<mensagem completa>\nEOF\n)"` para preservar a formatação em múltiplas linhas.
4. Rode `git status` após o commit para confirmar o resultado.
5. Ao final da mensagem de commit, mantenha as linhas de atribuição definidas nas instruções do sistema para esta sessão (Co-Authored-By), caso existam.

Se o usuário não confirmar, apenas deixe a mensagem gerada disponível para uso posterior (ex: copiar manualmente).
