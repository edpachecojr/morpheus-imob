# E3 — Motor SDR autônomo via WhatsApp

> **Objetivo:** responder todo lead em menos de 60s e entregar ao corretor
> apenas quem vale o tempo dele.
> **Requisitos:** RF3 · **Fase:** 2 · **Decisão:** [ADR-0006](../adrs/0006-triagem-por-arvore-de-decisao.md)
> **Bloqueado por:** D1 e D2 do [PRD](../prd.md) — conta WhatsApp e templates aprovados

Duas restrições da plataforma condicionam tudo aqui e não são negociáveis:
fora da **janela de 24h** só se envia template aprovado (R1), e cadência
agressiva derruba o rating do número (R2).

---

## E3-F1 — Canal WhatsApp

### E3-F1-H1 `[MVP]` · 5 pts — Conectar número do tenant
**Como** dono da conta, **quero** conectar meu WhatsApp, **para** que o sistema
atenda por mim.

- **Dado** as credenciais do canal, **quando** eu conectar, **então** o vínculo
  número → tenant é gravado e uma mensagem de teste é enviada e confirmada.
- **E** as credenciais ficam cifradas e **nunca** aparecem em log ou na API.
- **E** falha de conexão informa a causa em linguagem acionável.

### E3-F1-H2 `[MVP]` · 5 pts — Receber mensagens por webhook idempotente
**Como** sistema, **quero** receber mensagens sem duplicar nem perder, **para**
que nenhuma conversa se corrompa.

- **Dado** um webhook do provedor, **quando** chegar uma mensagem, **então** a
  assinatura é validada antes de qualquer processamento.
- **E** o endpoint responde em < 500ms, enfileirando o trabalho (RNF2).
- **E** a mesma mensagem reentregue pelo provedor **não** cria mensagem
  duplicada nem dispara resposta em dobro (RNF6, idempotência por id externo).
- **E** o tenant é resolvido pelo número de destino; sem resolução, a mensagem
  vai para quarentena com alerta — nunca para um tenant padrão.

### E3-F1-H3 `[MVP]` · 3 pts — Enviar mensagens atrás de porta
**Como** time, **quero** enviar mensagem por uma interface do domínio, **para**
poder trocar de provedor sem reescrever o negócio ([ADR-0007](../adrs/0007-integracoes-atras-de-porta.md)).

- **Dado** a porta `EnviadorDeMensagem`, **quando** o domínio enviar texto,
  template ou botões, **então** o adaptador traduz para o provedor.
- **E** em `local` e `test` o adaptador é um fake nomeado que registra o que
  teria enviado, sem tocar a rede.
- **E** falha de envio é retentada com backoff e, esgotada, alerta o corretor.

### E3-F1-H4 `[MVP]` · 3 pts — Respeitar a janela de 24h
**Como** sistema, **quero** conhecer a janela, **para** não ter mensagem
rejeitada nem o número punido.

- **Dado** uma conversa cuja última mensagem do contato foi há menos de 24h,
  **quando** eu enviar texto livre, **então** o envio ocorre.
- **E** fora da janela, texto livre é **bloqueado antes do envio** e substituído
  por template aprovado equivalente.
- **E** não havendo template adequado, o envio é adiado e o corretor é avisado.
- **E** a regra vive no adaptador, não no domínio.

### E3-F1-H5 `[MVP]` · 2 pts — Monitorar saúde do canal
- **Dado** o número conectado, **quando** o rating cair ou o envio for
  bloqueado pelo provedor, **então** um alerta é gerado — isto é incidente,
  não métrica (R2).

---

## E3-F2 — Triagem e qualificação

### E3-F2-H1 `[MVP]` · 3 pts — Boas-vindas em menos de 60s
**Como** lead, **quero** resposta imediata, **para** não procurar outro corretor.

- **Dado** um lead novo, **quando** ele chegar por qualquer origem, **então**
  recebe boas-vindas em menos de 60s no p95 (O1/RNF1).
- **E** a mensagem cita o imóvel de interesse quando ele é conhecido.
- **E** identifica-se como atendimento automático e oferece falar com humano
  (RNF17).
- **E** o lead passa para `em_triagem`.
- **E** fora do horário, a mensagem reconhece o horário sem deixar de responder.

### E3-F2-H2 `[MVP]` · 5 pts — Árvore de qualificação de 3 perguntas
**Como** corretor, **quero** que o bot pergunte o essencial, **para** eu não
gastar sábado com curioso.

- **Dado** um lead em triagem, **quando** o fluxo rodar, **então** ele pergunta,
  uma de cada vez, com botões de resposta rápida: urgência de mudança, forma de
  pagamento (à vista / financiamento / troca) e faixa de renda.
- **E** as respostas ficam gravadas como `Qualificacao` ligada ao lead.
- **E** resposta em texto livre fora das opções é tentada por palavra-chave e,
  não reconhecida, o bot repergunta **uma** vez e depois transborda.
- **E** o fluxo é determinístico: mesma sequência de entradas, mesmo resultado.
- **E** o teste roda sem rede e com relógio falso.

### E3-F2-H3 `[MVP]` · 3 pts — Classificar temperatura por regra auditável
**Como** corretor, **quero** saber quem é quente, **para** priorizar meu tempo.

- **Dado** uma qualificação completa, **quando** a regra rodar, **então** o lead
  recebe `quente`, `morno` ou `frio`, e a **explicação** de qual condição levou
  ao resultado fica registrada.
- **E** a temperatura é sempre derivada — nunca digitada (invariante 3 do
  [modelo de domínio](../dominio/modelo-de-dominio.md)).
- **E** mudar a regra e recalcular o histórico produz resultado consistente.
- **E** cada combinação da tabela de regras tem teste.

### E3-F2-H4 `[MVP]` · 3 pts — Transbordo para o corretor
**Como** corretor, **quero** assumir a conversa no momento certo, **para** não
perder um lead quente nem ser interrompido por um frio.

- **Dado** um lead classificado `quente`, **quando** a triagem terminar,
  **então** o corretor é notificado com o resumo da qualificação e o link da
  conversa.
- **E** a partir do transbordo o bot **para de responder** naquela conversa.
- **E** pedido explícito de falar com humano transborda imediatamente, em
  qualquer ponto do fluxo.
- **E** lead `frio` não gera notificação; fica na lista para consulta.

### E3-F2-H5 `[MVP]` · 2 pts — Silêncio e opt-out
**Como** titular dos dados, **quero** poder pedir para não ser mais contatado,
**para** exercer meu direito.

- **Dado** um lead sem resposta por 48h, **quando** o prazo vencer, **então**
  ele passa a `sem_resposta` e o bot para de insistir.
- **E** pedido de descadastro leva a `descartado` **terminal**: nenhum disparo
  automático posterior, em nenhum módulo do sistema.
- **E** existe teste que prova que o dunning (E6) também respeita esse estado.

### E3-F2-H6 · 5 pts — Fluxo configurável pelo tenant
Pós-MVP (Fase 6). RF3.6.

### E3-F2-H7 · 8 pts — Interpretação de texto livre por LLM
Pós-MVP (Fase 7). RF3.7. Só com a evidência descrita no
[ADR-0006](../adrs/0006-triagem-por-arvore-de-decisao.md) — quebrar em estórias
menores antes de executar.

---

## E3-F3 — Conversa no painel

### E3-F3-H1 `[MVP]` · 3 pts — Ver histórico da conversa
- **Dado** um lead, **quando** eu abrir a conversa, **então** vejo as mensagens
  em ordem, com autor (bot, corretor, lead) e horário.

### E3-F3-H2 `[MVP]` · 2 pts — Ver a qualificação de forma legível
- **Dado** um lead qualificado, **quando** eu abrir seu registro, **então** vejo
  temperatura, respostas e a explicação da classificação.
