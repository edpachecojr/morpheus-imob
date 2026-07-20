# E6 — Cobrança documental automática (dunning)

> **Objetivo:** o corretor nunca mais cobra documento à mão.
> **Requisitos:** RF6 · **Fase:** 1 (junto com [E5](e5-dossie-digital.md))

Este épico tem um risco que nenhum outro tem: **ele envia mensagem não
solicitada, repetidamente.** Feito com a mão pesada, gera bloqueio, derruba o
rating do número (R2 do [PRD](../prd.md)) e mata o canal de que o produto
inteiro depende. A cadência conservadora não é timidez — é preservação de
ativo.

Toda estória aqui é, na prática, um job. Idempotência não é detalhe de
implementação: é critério de aceite.

---

## E6-F1 — Motor de cadência

### E6-F1-H1 `[MVP]` · 3 pts — Rotina diária identifica pendências
**Como** sistema, **quero** varrer os dossiês todo dia, **para** saber quem
cobrar.

- **Dado** os dossiês do tenant, **quando** a rotina rodar, **então** ela
  seleciona os que estão `aberto` com item `pendente` ou `rejeitado`.
- **E** dossiê `completo`, `cancelado` ou sem participante ativo é ignorado.
- **E** a rotina roda por tenant, com o tenant explícito no payload do job.
- **E** o teste usa relógio falso e não toca a rede.
- **E** rodar a rotina duas vezes no mesmo dia não gera disparo duplicado.

### E6-F1-H2 `[MVP]` · 5 pts — Cobrança a cada 48h com teto
**Como** corretor, **quero** que o sistema insista sozinho — sem exagerar —
**para** o processo andar sem eu virar cobrador.

- **Dado** um participante com pendência, **quando** passarem 48h desde o último
  disparo (ou desde a emissão do link), **então** ele recebe uma cobrança.
- **E** a mensagem diz **o que exatamente falta** e traz o link de upload.
- **E** o intervalo mínimo entre disparos ao mesmo participante é respeitado
  mesmo que a rotina rode várias vezes.
- **E** há **teto de tentativas**; atingido, o sistema para e alerta o corretor
  em vez de continuar insistindo.
- **E** todo disparo é registrado com canal, horário e resultado.

### E6-F1-H3 `[MVP]` · 3 pts — Parar na hora certa
**Como** cliente, **quero** parar de receber cobrança assim que resolver,
**para** não achar que é spam.

- **Dado** um participante que completou sua parte, **quando** o item chegar,
  **então** nenhuma cobrança adicional é enviada a ele.
- **E** `DossieCompletado` encerra a cadência de todo o dossiê.
- **E** cancelar o dossiê encerra a cadência imediatamente.
- **E** contato com opt-out (`descartado` por solicitação do titular) **nunca**
  recebe disparo — teste explícito, conforme E3-F2-H5.
- **E** o corretor pode pausar a cobrança de um dossiê específico.

### E6-F1-H4 `[MVP]` · 2 pts — Reiniciar cadência na rejeição
- **Dado** um item rejeitado, **quando** `ItemRejeitado` for publicado,
  **então** a cadência daquele participante reinicia, cobrando o reenvio com o
  motivo da rejeição na mensagem.

### E6-F1-H5 `[MVP]` · 2 pts — Respeitar horário civil
**Como** cliente, **quero** não receber cobrança de madrugada, **para** não
bloquear o número do corretor.

- **Dado** um disparo devido, **quando** o horário estiver fora da faixa
  permitida (ex.: 08h–20h no fuso do tenant), **então** ele é adiado para a
  próxima faixa válida, não cancelado.
- **E** o teste cobre virada de dia e fuso.

---

## E6-F2 — Canais de disparo

### E6-F2-H1 `[MVP]` · 3 pts — Cobrança por WhatsApp
- **Dado** um disparo devido, **quando** enviado, **então** usa a porta
  `EnviadorDeMensagem` (E3-F1-H3).
- **E** fora da janela de 24h, usa template aprovado — sem exceção (R1).
- **E** falha de envio é retentada com backoff e não conta como disparo
  entregue.

### E6-F2-H2 `[MVP]` · 3 pts — Cobrança por e-mail
**Como** time, **quero** cobrar por e-mail também, **para** o módulo funcionar
antes de o WhatsApp ser aprovado.

- **Dado** um participante com e-mail, **quando** o disparo ocorrer, **então**
  o e-mail é enviado com a mesma informação da mensagem.
- **E** este é o canal padrão enquanto D1 do PRD não estiver resolvido — é o que
  desbloqueia a Fase 1 sem depender da Meta.
- **E** o e-mail traz descadastro funcional.

### E6-F2-H3 · 2 pts — Escolher canal e cadência por tenant
Pós-MVP (Fase 7). RF6.5.

---

## E6-F3 — Alertas ao corretor

### E6-F3-H1 `[MVP]` · 2 pts — Avisar quando a automação desistir
- **Dado** um participante que atingiu o teto de tentativas, **quando** isso
  ocorrer, **então** o corretor é notificado para agir pessoalmente.

### E6-F3-H2 · 3 pts — Alerta de estagnação após 5 dias
Pós-MVP (Fase 7). RF6.4 — dossiê sem movimentação nenhuma, mesmo dentro do teto.

### E6-F3-H3 · 2 pts — Resumo diário de pendências
Pós-MVP. Uma mensagem por dia com o estado de todos os dossiês.

---

## E6-F4 — Visibilidade

### E6-F4-H1 `[MVP]` · 2 pts — Histórico de cobrança no dossiê
**Como** corretor, **quero** ver o que já foi cobrado, **para** saber se ainda
cabe insistir antes de ligar.

- **Dado** um dossiê, **quando** eu abrir o histórico, **então** vejo cada
  disparo com data, canal, destinatário e resultado.
- **E** o histórico **não** exibe conteúdo de documento nem token de link.

### E6-F4-H2 `[MVP]` · 1 pt — Métrica de esforço até a conclusão
- **Dado** um dossiê completo, **quando** eu consultar suas métricas, **então**
  vejo quantos disparos foram necessários — insumo do KPI
  `lembretes_ate_conclusao` (PRD §2.3).
