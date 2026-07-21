# ADR-0009 — Eventos de domínio com gravação transacional no outbox

**Status:** Aceito
**Data:** 2026-07-21
**Contexto do produto:** [E1-F0-H4](../backlog/e1-core-saas.md) (fundação assíncrona), RF de arquitetura orientada a eventos

## Contexto

O produto é assíncrono por natureza: dunning, lembretes, relatórios quinzenais e
triagem reagem a fatos que acontecem em outro momento — "conta criada", "imóvel
cadastrado", "contrato assinado". Amarrar cada reação diretamente a quem causou o
fato acopla módulos que deveriam evoluir separados e, pior, arrisca perder o fato:
se a aplicação grava o dado e cai antes de publicar a mensagem, o consumidor nunca
souber que algo mudou.

Ao mesmo tempo, ainda é cedo para trazer o peso de um message broker, um mediator
ou um framework de eventos. A F0 é fundação: o que precisamos é a **propriedade**
de que todo fato de escrita é registrado de forma durável e atômica, sem escolher
ainda fila, serializador de fio ou biblioteca de mercado.

## Decisão

**Toda entidade acumula eventos de domínio; a escrita os grava no outbox na mesma
transação do dado.** Três peças, sem dependência externa nova:

1. **Entidade base com eventos e auditoria.** `EntidadeBase` concentra o que é
   comum a toda entidade — identidade, `DadosDeAuditoria` (value object com
   `CriadoEm`/`AtualizadoEm`) e o acúmulo de `IEventoDeDominio`. A ação de escrita
   (`Imovel.Cadastrar`, `Organizacao.Fundar`) registra o evento; a base só guarda,
   entrega e limpa. Entidades presas a SDK de terceiros (usuário do Identity)
   podem aderir só ao contrato `IPossuiEventosDeDominio`.

2. **Evento carrega os dados de negócio completos, não só um id.** Num sistema
   distribuído, o consumidor não deve reconsultar a origem para agir. `ContaCriada`
   leva nome, e-mail e plano para uma mensagem de boas-vindas ou upsell;
   `ImovelCadastrado` leva código e endereço. O tenant é metadado de **envelope**,
   resolvido na gravação — não campo do evento.

3. **Outbox só do lado de escrita.** Um interceptor de `SaveChanges`
   (`InterceptorDeGravacaoDeOutbox`) drena os eventos das entidades alteradas e os
   grava na tabela `mensagens_outbox` **dentro da mesma transação** — dado e evento
   sobem ou caem juntos. Registrado depois do interceptor de vínculo por
   organização, para ler a entidade já carimbada com o tenant. A lógica de drenagem
   vive num montador puro (`MontadorDeMensagensDeOutbox`), testável sem banco.

**Explicitamente fora de escopo neste MVP:** a drenagem da tabela — dispatcher,
publicadores, filas e consumidores. A coluna `processado_em` nasce nula só para o
futuro consumidor marcar o que publicou, sem exigir migração depois.

## Emenda — outbox por override de SaveChanges, não por interceptor (2026-07-21)

A decisão original drenava os eventos num `InterceptorDeGravacaoDeOutbox`. O interceptor
é o ponto de extensão idiomático do EF, mas o efeito ficava invisível no ponto de chamada —
registrado longe, no contêiner de dependências. Trocamos por um **override explícito de
`SaveChanges`/`SaveChangesAsync` no próprio `MorpheusDbContext`**: a drenagem é visível
onde o commit acontece, e a atomicidade (dado e evento na mesma transação) fica óbvia para
quem lê o contexto. O `MontadorDeMensagensDeOutbox` continua puro e testável fora do EF; o
override apenas o invoca antes do `base.SaveChanges`. Nada muda no contrato de eventos nem
na tabela `mensagens_outbox`.

A ordenação em relação ao antigo interceptor de vínculo deixou de importar: o tenant agora
já vem resolvido na entidade desde a construção (ver emenda do
[ADR-0003](0003-isolamento-multi-tenant.md)), então o outbox sempre lê a organização já
definida. Um teste de integração contra Postgres real prova que uma escrita rejeitada no
commit (FK de organização inexistente) leva junto a linha de outbox enfileirada — nenhum
evento órfão.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| Publicar direto num broker (RabbitMQ, SNS/SQS) na escrita | Dois commits sem transação distribuída: grava o dado, cai, perde a mensagem — exatamente o que o outbox evita. E amarra o broker antes de a hospedagem estar decidida ([ADR-0002](0002-escolha-de-stack.md)) |
| Mediator (MediatR) com notificações de domínio | Dependência e pipeline pesados para uma fundação sem casos de uso ainda; a mesma recusa do log transversal ([ADR-0008](0008-observabilidade-agnostica.md)) |
| Evento carregando só o id do agregado | Obriga o consumidor a reconsultar a origem — impossível ou caro num consumidor distribuído; e o dado pode já ter mudado quando ele reagir |
| Implementar o outbox inteiro (com dispatcher) agora | Fila, retente com backoff, idempotência e fila morta são um épico próprio; entregá-los junto atrasaria a fundação sem necessidade de MVP |

## Consequências

**Positivas**
- Todo fato de escrita fica registrado de forma durável e atômica desde já; nenhum
  evento se perde entre gravar o dado e anunciá-lo.
- O domínio ganha vocabulário de eventos sem broker, mediator ou biblioteca nova —
  reverter custa apagar código nosso, não desmontar integração de terceiro.
- Auditoria (`CriadoEm`/`AtualizadoEm`) deixa de ser carimbada à mão em cada
  entidade: a base e o value object concentram a regra.

**Negativas**
- A tabela `mensagens_outbox` cresce sem limite até existir o dispatcher que a
  drena e poda — dívida consciente, restrita ao período do MVP.
- Adicionar entidades dentro do interceptor de `SaveChanges` é um acoplamento sutil
  ao ciclo do EF; um teste de integração prova que dado e evento compartilham a
  transação, inclusive que uma escrita rejeitada não deixa evento órfão.
- Renomear `cadastrado_em`/`criada_em` para as colunas de auditoria custou uma
  migração — barata agora (fundação sem dados de produção), cara se adiada.

**Como saberemos que erramos:** se, ao construir o dispatcher, o formato do evento
gravado se mostrar insuficiente para o consumidor agir sem reconsultar a origem, o
princípio de "dados completos no evento" falhou e este ADR precisa de sucessor.
