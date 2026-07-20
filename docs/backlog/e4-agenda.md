# E4 — Agenda inteligente e agendamento autônomo

> **Objetivo:** eliminar o ping-pong de mensagens — o bot fecha o horário sozinho.
> **Requisitos:** RF4 · **Fase:** 3
> **Bloqueado por:** D3 do [PRD](../prd.md) — OAuth Google verificado

Regra que atravessa o épico: **nunca agendar em cima de compromisso existente**.
Uma visita marcada sobre o almoço do corretor destrói a confiança na automação
mais rápido do que qualquer outro bug.

---

## E4-F1 — Conexão com o calendário

### E4-F1-H1 `[MVP]` · 5 pts — Conectar Google Calendar por OAuth
**Como** corretor, **quero** conectar minha agenda, **para** que o bot só ofereça
horário que eu tenho de fato.

- **Dado** o fluxo OAuth, **quando** eu autorizar, **então** o refresh token é
  armazenado cifrado e vinculado ao meu usuário.
- **E** o escopo pedido é o mínimo necessário.
- **E** posso desconectar, o que revoga o token e para os agendamentos.
- **E** token expirado ou revogado pelo Google gera alerta acionável ao
  corretor, não falha silenciosa.

### E4-F1-H2 `[MVP]` · 3 pts — Ler ocupações atrás de porta
- **Dado** a porta `AgendaDoCorretor`, **quando** o domínio pedir as ocupações
  de um intervalo, **então** o adaptador consulta o Google e devolve no
  vocabulário do domínio ([ADR-0007](../adrs/0007-integracoes-atras-de-porta.md)).
- **E** o teste usa fake nomeado, sem rede.
- **E** indisponibilidade do Google não derruba o fluxo: o bot informa que
  confirmará em seguida e o corretor é alertado (RNF7).

### E4-F1-H3 `[MVP]` · 3 pts — Janela de atendimento
**Como** corretor, **quero** definir meus dias e horários, **para** não receber
visita às 22h de domingo.

- **Dado** minha configuração, **quando** eu informar dias, faixas de horário,
  duração padrão da visita e antecedência mínima, **então** ela restringe todo
  cálculo de disponibilidade.
- **E** o fuso do tenant é respeitado em todo cálculo e exibição.
- **E** existe padrão utilizável na criação da conta (dias úteis, comercial).

---

## E4-F2 — Agendamento autônomo

### E4-F2-H1 `[MVP]` · 5 pts — Calcular horários livres
**Como** sistema, **quero** derivar slots realmente livres, **para** nunca
oferecer o que não existe.

- **Dado** a janela de atendimento e as ocupações do calendário, **quando** eu
  calcular, **então** um slot só é livre se estiver dentro da janela, sem
  conflito na agenda e com a antecedência mínima respeitada.
- **E** o cálculo é puro e testável com relógio falso — sem rede.
- **E** casos de borda têm teste: virada de dia, horário de verão, evento de dia
  inteiro, evento que cruza a borda da janela.
- **E** não havendo slot no horizonte configurado, o resultado é vazio
  explicitamente, não um erro.

### E4-F2-H2 `[MVP]` · 3 pts — Oferecer 3 horários ao lead quente
**Como** lead, **quero** escolher um horário na conversa, **para** não trocar
seis mensagens.

- **Dado** um lead `quente` recém-qualificado, **quando** houver slots,
  **então** o bot oferece os 3 próximos como botões de resposta rápida.
- **E** não havendo slot, o bot avisa e transborda ao corretor.
- **E** a oferta expira: escolha um slot já ocupado enquanto isso e o bot
  reoferece, sem erro visível ao lead.

### E4-F2-H3 `[MVP]` · 3 pts — Criar a visita e o evento
- **Dado** um horário escolhido, **quando** o lead confirmar, **então** a visita
  é criada como `agendada` e o evento aparece no Google Calendar com imóvel,
  endereço e contato do lead.
- **E** a criação é idempotente: dupla confirmação não gera dois eventos.
- **E** falha na criação do evento não deixa visita "fantasma" — ou os dois
  existem, ou nenhum, com nova tentativa e aviso.
- **E** o lead recebe confirmação com data, hora e endereço.

### E4-F2-H4 `[MVP]` · 2 pts — Agendar manualmente pelo painel
**Como** corretor, **quero** marcar uma visita à mão, **para** casos que vieram
por fora do bot.

- **Dado** um lead e um imóvel, **quando** eu escolher data e hora, **então** a
  visita segue exatamente o mesmo modelo e as mesmas regras da automática.

### E4-F2-H5 `[MVP]` · 2 pts — Registrar o desfecho da visita
- **Dado** uma visita passada, **quando** eu marcá-la, **então** posso definir
  `realizada` ou `nao_compareceu`.
- **E** `realizada` publica `VisitaRealizada`, que aciona o pedido de feedback
  no Radar (E7).
- **E** transições inválidas são rejeitadas informando estado atual e destinos.

---

## E4-F3 — Lembretes

### E4-F3-H1 `[MVP]` · 3 pts — Lembrete 2h antes
**Como** corretor, **quero** que o lead seja lembrado, **para** reduzir o
não comparecimento.

- **Dado** uma visita agendada, **quando** faltarem 2h, **então** o lead recebe
  lembrete com imóvel, horário e endereço.
- **E** o disparo é idempotente: reprocessar o job não envia duas vezes.
- **E** fora da janela de 24h, usa template aprovado (E3-F1-H4).
- **E** visita cancelada não dispara lembrete.
- **E** o teste usa relógio falso e não toca a rede.

### E4-F3-H2 `[MVP]` · 2 pts — Confirmação no lembrete
- **Dado** o lembrete, **quando** o lead confirmar, **então** a visita passa a
  `confirmada` e o corretor é notificado.
- **E** se avisar que não vai, a visita é `cancelada` e o corretor é notificado.

### E4-F3-H3 · 3 pts — Lembrete adicional 24h antes
Pós-MVP. Um lembrete só, no MVP, para conter custo por mensagem e risco de spam.

---

## E4-F4 — Reagendamento e integrações extras *(pós-MVP — Fase 7)*

| ID | Estória | Pts | Nota |
| --- | --- | --- | --- |
| E4-F4-H1 | Reagendar pelo WhatsApp, sem falar com o corretor | 5 | RF4.5 |
| E4-F4-H2 | Sincronizar bidirecionalmente: alterar no Google reflete no sistema | 5 | Exige webhook do Google e reconciliação |
| E4-F4-H3 | Buffer de deslocamento entre visitas | 5 | RF4.6 |
| E4-F4-H4 | Suportar Outlook Calendar | 5 | Segundo adaptador da mesma porta |
