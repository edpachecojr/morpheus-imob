# morpheus-imob — Diretrizes de Engenharia

Este arquivo é a **fonte canônica** de regras de engenharia do projeto. As regras
abaixo são inegociáveis e valem para todo código produzido aqui, por humanos ou
agentes.

> Documento vivo: regras que se provarem erradas na prática devem ser removidas
> ou reescritas — mas só por decisão explícita, nunca por conveniência pontual.

## Contexto do produto

Suíte operacional automatizada para o mercado imobiliário (pequenas imobiliárias
e corretores autônomos). Três frentes:

1. **Triagem de leads via WhatsApp** — qualificação instantânea e agendamento
   autônomo de visitas, com integração bidirecional ao Google Calendar.
2. **Dossiê Digital** — esteira documental mobile-first via Magic Links, com
   motor de cobrança automática de certidões e comprovantes.
3. **Radar do Proprietário** — centralização de feedbacks de visita e relatórios
   quinzenais de transparência.

O objetivo é substituir CRMs estáticos por um fluxo conversacional, assíncrono e
inteligente que zera o tempo de primeira resposta.

## Estilo de código

- **Funções: 4 a 20 linhas.** Passou disso, divida.
- **Arquivos: abaixo de 200 linhas é o ideal; 200–300 é aceitável.** Acima de
  300, divida por responsabilidade.
- **Uma coisa por função, uma responsabilidade por módulo (SRP).** Cada módulo
  tem uma única razão para mudar.
- **Nomes específicos e únicos.** Revelam intenção, sem desinformação,
  pronunciáveis e pesquisáveis. Evite `data`, `handler`, `Manager`, `util`.
  Prefira nomes que retornem menos de 5 ocorrências num grep da base.
- **Tipos explícitos.** Sem `any`, sem dicionário genérico como contrato, sem
  função destipada. A assinatura declara o que entra, o que sai e o que é válido.
- **Sem duplicação (DRY).** Lógica repetida vira função ou módulo reutilizável.
- **Retorno cedo em vez de `if` aninhado. Máximo 2 níveis de indentação.**
  Um único nível de abstração por função.
- **Mensagens de erro carregam contexto:** o valor que causou a falha e o
  formato esperado.

## Comentários

- **Preserve os comentários existentes.** Não os remova em refatorações — eles
  carregam intenção e proveniência.
- **Escreva o PORQUÊ, não o QUÊ.** Nada de `// incrementa contador` acima de `i++`.
- **Sem comentários descrevendo o óbvio.** Comentário em excesso é code smell:
  cada comentário é uma dívida que envelhece mal.
- **Docstrings em funções públicas:** intenção + um exemplo de uso.
- **Cite número de issue ou SHA de commit** quando uma linha existe por causa de
  um bug específico ou de uma restrição externa.

## Testes

- **Comando único para rodar a suíte:** `<a definir com a stack>`.
- **Toda função nova ganha teste. Toda correção de bug ganha teste de regressão.**
- **Mocke I/O externo** (API, banco, sistema de arquivos) com classes fake
  nomeadas — não com stubs inline.
- **Testes devem ser F.I.R.S.T.:**
  - **Fast** (rápidos) — rodam em milissegundos, ninguém evita executá-los.
  - **Independent** (independentes) — nenhum teste depende da ordem ou do estado
    deixado por outro.
  - **Repeatable** (repetíveis) — mesmo resultado em qualquer ambiente, sem rede
    nem relógio real.
  - **Self-Validating** (autoverificáveis) — passam ou falham, sem inspeção
    manual de log.
  - **Timely** (oportunos) — escritos junto com o código de produção, não depois.

## Dependências

- **Injete dependências** por construtor ou parâmetro — nunca por global ou
  import direto no ponto de uso. Código com dependência injetada é testável em
  isolamento.
- **Envolva bibliotecas de terceiros atrás de uma interface fina** de
  propriedade deste projeto. O domínio não conhece o SDK.

## Estrutura de diretórios

- Siga a convenção do framework escolhido.
- Módulos pequenos e focados em vez de arquivos-deus.
- Caminhos previsíveis e consistentes.

> **A definir em conjunto.** A convenção de pastas deste projeto ainda não foi
> fechada — será construída junto com a escolha de stack e documentada aqui.
> Até lá, não improvise estrutura nova sem alinhar.

## Commits

- **Sempre Conventional Commits**, com o assunto em **português BR**:
  `<tipo>(<escopo opcional>): <assunto no imperativo, minúsculo, sem ponto final>`
- Tipos: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `perf`, `build`, `ci`.
- Assunto em até ~72 caracteres. O corpo explica o **porquê**, não o quê — o
  diff já mostra o quê.
- Mudança incompatível: `!` após o tipo e um rodapé `BREAKING CHANGE:`.
- Referencie a issue no rodapé quando o commit existe por causa dela.

```
feat(whatsapp): qualifica lead antes de oferecer horários de visita

Agendar sem qualificar enchia a agenda do corretor de visita improdutiva.
A triagem roda antes de consultar o Google Calendar.

Closes #42
```

## Formatação

Use o formatador padrão da linguagem (`prettier`, `black`, `gofmt`, `cargo fmt`,
`rubocop -A`). Estilo além disso não se discute.

## Logging

- **JSON estruturado com campos nomeados** para depuração e observabilidade.
  Log em prosa é inútil para agentes e para busca.
- **Texto plano apenas** para saída de CLI voltada ao usuário.
- **Nunca logue segredo, token, credencial ou dado pessoal do cliente.**

## Segredos e segurança

Este repositório é **público**. Portanto:

- **Zero segredo, chave de API, token ou credencial no código.** Sem exceção,
  nem em teste, nem em comentário, nem em fixture.
- Toda configuração sensível vem de variável de ambiente, documentada em
  `.env.example` com valor de exemplo — nunca com o valor real.
- `.env` e derivados ficam no `.gitignore`.
- Dados de clientes, imóveis e conversas reais nunca entram no repositório.

<!-- ai-memory:start -->
## Long-term memory (ai-memory)

This project uses [ai-memory](https://github.com/akitaonrails/ai-memory)
for cross-session continuity.

**Default to the current project - always.** Every ai-memory tool
auto-scopes to the project resolved from your session's working
directory. **Do NOT pass `project`, `workspace`, or `cwd` arguments unless
the user explicitly references a *different* project by name** (e.g. "what
did we decide in the `other-app` project?"). Phrases like "this project",
"here", "we", "our work", and "where did we leave off" all mean the
*current* project, so call tools with no scoping args.

This default assumes the MCP client can identify the current agent
session. Static MCP clients in parallel sessions for the same user cannot
forward the real agent session id automatically; pass explicit
`workspace` + `project` / `scopes`, or use a session-aware bridge that
forwards the lifecycle-hook session id on MCP calls.

**Lifecycle hooks already capture every prompt and tool call
automatically.** Do not manually write routine notes. Only write durable
memory when the user explicitly asks to remember or annotate something
permanently.

### Use the installed ai-memory Agent Skills

Detailed tool-routing guidance lives in the installed ai-memory Agent
Skills. When a task matches an installed ai-memory Agent Skill, load and
follow that skill before calling ai-memory tools. The skills cover memory
retrieval, handoffs, durable pages, learning maintenance, and routing
install or refresh work.

### When you write a project rule, write it here

If you're about to write a durable project rule ("always X", "never
Y", "all PRs must ..."), write it in the project's canonical agent instruction file.
Many projects use CLAUDE.md for Claude Code and
AGENTS.md for Codex / OpenCode / Cursor / Gemini CLI, but if the project
says one file is canonical, use that file.

### Refreshing this snippet

This block is maintained by ai-memory. Two ways to refresh it with the
latest binary's recommended copy:

- **From the agent** (no terminal needed): ask "refresh the ai-memory
  routing in this project". The agent calls `memory_install_self_routing`,
  picks the right filename for itself (Claude Code -> `CLAUDE.md`; Codex /
  OpenCode / Cursor / Gemini -> `AGENTS.md`), uses its Write / Edit tool
  to replace or append the returned `markered_block` while preserving
  non-ai-memory user content, then writes or updates each returned
  `managed_skills` item under the selected skill root from `target_hints`
  using its `relative_path`.
- **From the CLI**: `ai-memory install-instructions` (defaults to
  `CLAUDE.md`; pass `--target AGENTS.md` for non-Claude agents or projects
  that use `AGENTS.md` as the canonical instruction file).

Both are idempotent: re-runs replace the block bracketed by
`<!-- ai-memory:start -->` / `<!-- ai-memory:end -->` markers without
disturbing the rest of the file.
<!-- ai-memory:end -->
