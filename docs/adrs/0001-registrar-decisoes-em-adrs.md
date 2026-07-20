# ADR-0001 — Registrar decisões arquiteturais em ADRs

**Status:** Aceito
**Data:** 2026-07-20

## Contexto

O projeto começa do zero, com muitas decisões estruturais tomadas em sequência
rápida e, em boa parte, por agentes de IA em sessões que não compartilham
memória. Duas patologias previsíveis:

1. Decisão enterrada em thread de PR, mensagem de chat ou na cabeça de quem
   estava lá. Seis meses depois ninguém sabe por que é assim, e a pergunta
   "isso foi de propósito?" não tem resposta.
2. Decisão silenciosamente revertida por conveniência pontual — o cenário que
   o [CLAUDE.md](../../CLAUDE.md) proíbe explicitamente.

O `CLAUDE.md` já é a fonte canônica das **regras de engenharia**. Falta o
registro das **decisões arquiteturais** e do contexto que as justificou.

## Decisão

Toda decisão arquitetural de custo alto de reversão é registrada como um ADR
numerado em `docs/adrs/`, seguindo o modelo do [README](README.md). ADR aceito
é imutável: mudança de rumo é ADR novo que substitui o anterior.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| Documento único de arquitetura | Vira colcha de retalhos sem data e sem contexto; ninguém sabe o que ainda vale |
| Só o histórico de PRs | Contexto se perde na migração de ferramenta e não é pesquisável offline |
| Wiki externa (Notion, Confluence) | Sai de sincronia com o código e não chega ao contexto de um agente que lê o repositório |
| Nada — decidir e seguir | É o modo padrão de falha deste tipo de projeto |

## Consequências

**Positivas**
- Um agente novo reconstrói o raciocínio lendo o repositório, sem depender de
  histórico de conversa.
- Discussão encerrada não reabre por esquecimento.
- Decidir "não fazer" vira registro explícito, não omissão ambígua.

**Negativas**
- Custo por decisão. Mitigado mantendo ADRs curtos e reservados ao que é caro
  reverter.
- Risco de ADR desatualizado, que é pior que ADR ausente. Mitigado pelo status
  explícito e pela regra de substituição.

**Como saberemos que erramos:** se os ADRs virarem formalidade escrita depois
do fato para justificar o que já foi feito, o mecanismo perdeu a função e deve
ser simplificado ou abandonado — não mantido por cerimônia.
