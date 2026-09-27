# Resultado: criar skill de commit

Criada a skill `.claude/skills/commit/SKILL.md`.

## O que a skill faz
- Coleta o contexto de mudanças com prioridade: (1) diff staged, (2) diff do working tree/untracked, (3) diff acumulado da branch atual contra a base (`main`/`master`) quando o working tree está limpo.
- Classifica o `type` no padrão Conventional Commits (feat, fix, docs, style, refactor, perf, test, build, ci, chore, revert), com suporte a `!` e rodapé `BREAKING CHANGE`.
- Deriva um `scope` opcional a partir do módulo/diretório predominante alterado.
- Monta a mensagem: assunto curto no imperativo em português + corpo em bullets agrupando as mudanças por intenção.
- Apresenta a mensagem ao usuário e só executa `git add`/`git commit` mediante confirmação explícita; nunca usa `git add -A`/`git add .` sem confirmação.

## Observações
- No momento da criação, o repositório não possuía nenhum commit ainda (branch `master` sem histórico), então não havia convenção de idioma/estilo de commits anteriores para seguir; a skill assume português como padrão, com bullets no corpo.
