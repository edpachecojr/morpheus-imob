# E2 — Ingestão de leads e cadastro de imóveis

> **Objetivo:** dado entra no sistema sem digitação e sem perda, venha de onde vier.
> **Requisitos:** RF2 · **Fases:** 1 (imóvel), 2 (leads), 6 (conectores)

A regra que atravessa o épico: **nenhum lead se perde**. Falha de processamento
manda para quarentena com alerta, nunca para o limbo.

---

## E2-F1 — Cadastro de imóveis

### E2-F1-H1 `[MVP]` · 3 pts — Cadastrar imóvel manualmente
**Como** corretor, **quero** cadastrar um imóvel em menos de um minuto, **para**
começar a usar o sistema sem projeto de migração.

- **Dado** o formulário, **quando** eu informar código de referência, título,
  finalidade (`locacao`/`venda`), endereço e link do portal, **então** o imóvel
  é criado no meu tenant com situação `disponivel`.
- **E** apenas código, título e finalidade são obrigatórios — o resto é opcional.
- **E** código repetido no mesmo tenant é rejeitado citando o código conflitante.
- **E** o mesmo código em outro tenant é aceito (E1-F1-H3).

### E2-F1-H2 `[MVP]` · 2 pts — Listar e buscar imóveis
- **Dado** imóveis cadastrados, **quando** eu buscar por código, título ou
  endereço, **então** vejo os resultados do **meu** tenant, paginados.
- **E** posso filtrar por finalidade e situação.

### E2-F1-H3 `[MVP]` · 2 pts — Editar imóvel e mudar situação
- **Dado** um imóvel, **quando** eu alterá-lo, **então** a mudança é registrada
  com autor e horário.
- **E** transições de situação inválidas são rejeitadas informando o estado
  atual e os destinos permitidos.

### E2-F1-H4 `[MVP]` · 2 pts — Registrar proprietário do imóvel
**Como** corretor, **quero** vincular o dono ao imóvel, **para** poder enviar o
relatório do Radar depois.

- **Dado** um imóvel, **quando** eu informar nome e WhatsApp do proprietário,
  **então** o vínculo é criado.
- **E** um proprietário pode ter vários imóveis no mesmo tenant.
- **E** o telefone é normalizado para o formato internacional na gravação.

### E2-F1-H5 · 5 pts — Importar imóvel por link ou XML do portal
Pós-MVP (Fase 6). RF2.6.

---

## E2-F2 — Entrada de leads

### E2-F2-H1 `[MVP]` · 3 pts — Link parametrizado por imóvel
**Como** corretor, **quero** um link que abre o WhatsApp já identificando o
imóvel, **para** colocar na bio do Instagram e no meu site.

- **Dado** um imóvel, **quando** eu pedir o link de captação, **então** recebo
  uma URL que abre a conversa no WhatsApp com mensagem inicial contendo um
  identificador rastreável.
- **E** ao chegar a mensagem, o lead é criado já associado ao imóvel e à
  origem `link_direto`.
- **E** o identificador **não** expõe dado interno adivinhável de outro imóvel.

### E2-F2-H2 `[MVP]` · 3 pts — Webhook genérico de entrada
**Como** corretor, **quero** que meu site ou portal envie leads ao sistema,
**para** não copiar e colar de e-mail.

- **Dado** um webhook autenticado por chave do tenant, **quando** chegar um
  lead com nome, telefone e referência do imóvel, **então** o lead é criado com
  origem `webhook_portal`.
- **E** o endpoint responde em menos de 500ms, enfileirando o processamento.
- **E** a mesma entrega repetida não cria lead duplicado (idempotência por
  identificador externo).
- **E** payload inválido responde 4xx com mensagem que cita o campo problemático
  e o formato esperado, e é registrado para diagnóstico.
- **E** requisição sem assinatura ou chave válida é rejeitada.

### E2-F2-H3 `[MVP]` · 2 pts — Cadastro manual de lead
- **Dado** um contato que chegou por fora, **quando** eu cadastrá-lo com nome,
  telefone e imóvel, **então** o lead entra com origem `manual` e segue o mesmo
  fluxo dos demais.

### E2-F2-H4 `[MVP]` · 3 pts — Deduplicação por telefone
**Como** corretor, **quero** que o mesmo contato não vire três leads, **para**
não atender a mesma pessoa três vezes.

- **Dado** um lead ativo do telefone X, **quando** chegar outro contato do mesmo
  telefone no mesmo tenant, **então** o interesse é anexado ao lead existente,
  não duplicado.
- **E** se o lead anterior estiver em estado terminal (`convertido`, `perdido`,
  `descartado`), um lead novo é criado — exceto se `descartado` por solicitação
  do próprio titular, caso em que nada é criado e nenhum disparo ocorre.
- **E** o telefone é comparado de forma normalizada (com e sem o nono dígito).

### E2-F2-H5 `[MVP]` · 2 pts — Quarentena de lead não processável
**Como** operador, **quero** que nada suma, **para** poder recuperar
manualmente.

- **Dado** um lead que não pode ser processado (tenant não resolvido, imóvel
  inexistente), **quando** o processamento falhar, **então** ele vai para
  quarentena com o motivo e gera alerta.
- **E** é possível reprocessar da quarentena após corrigir a causa.
- **E** nunca é atribuído a um tenant "padrão".

### E2-F2-H6 · 5 pts — Conectores nomeados por portal e Meta Ads
Pós-MVP (Fase 6). RF2.4. Um adaptador por portal, atrás da mesma porta.

---

## E2-F3 — Distribuição de leads *(pós-MVP — Fase 6)*

| ID | Estória | Pts |
| --- | --- | --- |
| E2-F3-H1 | Distribuir leads por rodízio entre corretores ativos | 3 |
| E2-F3-H2 | Rotear pelo corretor responsável do imóvel | 2 |
| E2-F3-H3 | Rotear por perfil de imóvel ou região | 5 |
| E2-F3-H4 | Reatribuir lead manualmente, preservando o histórico | 2 |

Fora do MVP porque o público inicial é o corretor autônomo — não há entre quem
distribuir.

---

## E2-F4 — Conformidade de dados do lead *(pós-MVP)*

> **Estória faltante identificada na revisão de 2026-07-21.** RNF13 (LGPD)
> pede "retenção definida por tipo de dado e rotina de exclusão a pedido" para
> todo dado pessoal, não só documento. Diferente do E5-F6 (documento de
> identidade, tratado como MVP por decisão explícita do PRD em R4), nome e
> telefone de lead são dado pessoal de sensibilidade menor — fica pós-MVP até
> haver pedido real ou exigência de cliente maior.

| ID | Estória | Pts |
| --- | --- | --- |
| E2-F4-H1 | Atender pedido de exclusão de dados do lead (dados cadastrais e histórico de conversa) | 3 |

**Gatilho:** primeiro pedido real de um titular, ou exigência de auditoria de
uma imobiliária com equipe maior.
