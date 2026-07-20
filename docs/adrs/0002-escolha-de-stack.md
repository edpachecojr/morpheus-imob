# ADR-0002 — Escolha de stack

**Status:** **Proposto** — nenhuma opção foi escolhida
**Data:** 2026-07-20
**Bloqueia:** todo desenvolvimento (D4 do [PRD](../prd.md))

## Contexto

Não há código de produção. A stack condiciona modelo de dados, forma de rodar
jobs, hospedagem, custo operacional e a velocidade de todo o resto. Escolher no
primeiro commit, por hábito, é como projetos deste tipo costumam se
comprometer sem discussão — este ADR existe para impedir isso.

As necessidades reais estão em [fundacao/stack.md](../fundacao/stack.md). Em
resumo: **jobs agendados e em fila** são o coração do produto (dunning,
lembretes, relatórios), o webhook precisa responder em < 500ms enfileirando, a
interface de cliente final é web mobile-first sem app, o banco é relacional com
isolamento por tenant, e a equipe é pequena demais para operar arquitetura
distribuída.

## Decisão

**Adiada deliberadamente.** Este ADR permanece *proposto* até que uma spike
prove o caminho crítico ponta a ponta numa candidata.

O critério de decisão é o descrito em [stack.md](../fundacao/stack.md):
receber webhook → enfileirar job → job chama serviço externo falso → persiste
com tenant correto → página mobile lê o resultado, tudo com teste automatizado
e sem rede. Se levar mais de dois dias, a candidata está avisando algo.

## Alternativas consideradas

### A — TypeScript full-stack
Next.js (App Router) para painel e Magic Link, Fastify ou NestJS para API,
Prisma + PostgreSQL, BullMQ + Redis para filas.

- **A favor:** uma linguagem em toda a superfície; ecossistema maduro para
  OAuth Google e Meta; Next.js resolve bem o mobile-first do Magic Link;
  contratação e material abundantes.
- **Contra:** duas peças de infraestrutura (Postgres + Redis); tipagem em
  runtime exige disciplina (Zod nas bordas); ecossistema volátil — o custo de
  manutenção aparece em 18 meses, não agora.

### B — Dart/Flutter com backend em Dart
Flutter para web e mobile, Dart Frog ou Serverpod na API.

- **A favor:** uma linguagem com tipagem sólida; caminho natural se houver app
  nativo depois; skills de Dart/Flutter já instaladas nesta máquina sugerem
  familiaridade.
- **Contra:** Flutter Web tem bundle pesado e é o **oposto** do que o RNF3 pede
  para a página de Magic Link em 4G; ecossistema de backend Dart pequeno, com
  poucas bibliotecas maduras para WhatsApp, OAuth e filas. Risco real de
  escrever integração que na opção A é dependência pronta.

### C — Python no backend, TypeScript no front
FastAPI + SQLAlchemy + Celery, Next.js no front.

- **A favor:** o melhor ecossistema se o roadmap for fundo em IA/NLP na
  triagem; Celery é maduro para o que o dunning exige; SQLAlchemy é sólido.
- **Contra:** duas linguagens e dois times mentais numa equipe minúscula;
  tipagem opcional exige rigor; a [ADR-0006](0006-triagem-por-arvore-de-decisao.md)
  tira do MVP justamente a parte que justificaria Python.

### D — Rails ou Django monolito
- **A favor:** máxima velocidade inicial, batteries included, jobs e auth
  prontos, um só processo para operar.
- **Contra:** tipagem fraca colide de frente com a regra "tipos explícitos, sem
  `any`, sem dicionário genérico como contrato" do CLAUDE.md. Adotar exigiria
  revisar essa regra — o que é decisão explícita, não efeito colateral.

## Consequências

**Positivas**
- A ausência de decisão fica visível e datada, em vez de ser resolvida por
  acidente no primeiro commit.
- O critério de decisão está escrito antes de haver preferência investida.

**Negativas**
- Nada de produção começa até isto fechar. É o custo aceito conscientemente.
- Documentação de fundação fica com lacunas (`<a definir>`) enquanto isso.

**Como saberemos que erramos:** se o adiamento passar de uma semana sem spike
iniciada, o adiamento virou procrastinação. Nesse caso, escolha a opção A pelos
argumentos acima e siga — decisão medíocre executada vale mais que decisão
ótima pendente.

## Ao aceitar

1. Mudar o status deste ADR e registrar a data e o resultado da spike.
2. Preencher [fundacao/stack.md](../fundacao/stack.md).
3. Substituir os `<a definir>` do [CLAUDE.md](../../CLAUDE.md) — comando único
   da suíte e convenção de diretórios.
4. Criar `.env.example` com as variáveis reais da stack.
