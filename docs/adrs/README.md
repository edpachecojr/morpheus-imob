# ADRs — Architecture Decision Records

Uma decisão arquitetural por arquivo, numerada, datada e **imutável depois de
aceita**. Mudou de ideia? Escreva um ADR novo que substitui o anterior — não
reescreva a história. O valor de um ADR está em registrar o contexto em que a
decisão fez sentido, inclusive quando ela deixa de fazer.

## Índice

| # | Decisão | Status | Data |
| --- | --- | --- | --- |
| [0001](0001-registrar-decisoes-em-adrs.md) | Registrar decisões arquiteturais em ADRs | Aceito | 2026-07-20 |
| [0002](0002-escolha-de-stack.md) | Escolha de stack | **Proposto** | 2026-07-20 |
| [0003](0003-isolamento-multi-tenant.md) | Isolamento multi-tenant por coluna com RLS | Aceito | 2026-07-20 |
| [0004](0004-linguagem-ubiqua-em-portugues.md) | Linguagem ubíqua do domínio em português | Aceito | 2026-07-20 |
| [0005](0005-rbac-simples.md) | RBAC simples com permissões nomeadas | Aceito | 2026-07-20 |
| [0006](0006-triagem-por-arvore-de-decisao.md) | Triagem por árvore de decisão, não LLM, no MVP | Aceito | 2026-07-20 |
| [0007](0007-integracoes-atras-de-porta.md) | Integrações externas atrás de porta do domínio | Aceito | 2026-07-20 |
| [0008](0008-observabilidade-agnostica.md) | Observabilidade agnóstica via Serilog e OpenTelemetry | Aceito | 2026-07-21 |
| [0009](0009-eventos-de-dominio-e-outbox.md) | Eventos de domínio com gravação transacional no outbox | Aceito | 2026-07-21 |

## Quando escrever um ADR

Escreva quando a decisão for **cara de reverter** ou quando alguém em três
meses fosse perguntar "por que diabos isso é assim?":

- Escolha de tecnologia, framework, banco, provedor externo.
- Modelo de dados estrutural (isolamento, chaves, particionamento).
- Fronteira entre módulos ou contextos.
- Padrão transversal: autenticação, autorização, tratamento de erro, filas.
- Decisão de **não** fazer algo, quando a omissão pareceria descuido.

Não escreva para: escolha de nome de variável, formatação, refatoração local,
ou qualquer coisa que um PR revertível resolve.

## Status

| Status | Significa |
| --- | --- |
| `Proposto` | Em discussão. Não implemente ainda |
| `Aceito` | Vale agora. É lei até ser substituído |
| `Substituído por ADR-XXXX` | Não vale mais; leia o sucessor |
| `Descartado` | Considerado e recusado. O registro do "não" evita reabrir a discussão |

## Modelo

```markdown
# ADR-XXXX — Título no imperativo

**Status:** Proposto | Aceito | Substituído por ADR-YYYY | Descartado
**Data:** AAAA-MM-DD
**Contexto do produto:** link para o requisito ou épico que motivou

## Contexto
Qual força nos trouxe aqui. Restrições reais, não teoria.

## Decisão
O que foi decidido, em uma frase afirmativa.

## Alternativas consideradas
| Alternativa | Por que não |

## Consequências
**Positivas:** o que ganhamos.
**Negativas:** o preço — seja honesto; ADR sem custo listado é propaganda.
**Como saberemos que erramos:** o sinal concreto que exige revisitar.
```
