# ADR-0008 — Observabilidade agnóstica de fornecedor via Serilog e OpenTelemetry

**Status:** Aceito
**Data:** 2026-07-21
**Contexto do produto:** [E1-F4](../backlog/e1-core-saas.md) (observabilidade), RNF de operação

## Contexto

O produto é assíncrono e multi-tenant: incidente sem log pesquisável e sem
correlação é incidente investigado por adivinhação. A equipe é pequena e não há
plantão ([fundacao/stack.md](../fundacao/stack.md), R6), então a análise depende
de log estruturado e de rastreamento distribuído, não de leitura de prosa.

Ao mesmo tempo, a hospedagem e o fornecedor de APM ainda estão em aberto
([ADR-0002](0002-escolha-de-stack.md)). Amarrar o código a um SDK proprietário
(Datadog, Elastic, Sentry) agora seria escolher fornecedor por acidente — o
mesmo erro que o ADR-0002 existe para evitar.

## Decisão

**Log estruturado com Serilog, rastreamento com OpenTelemetry, exportação por
OTLP.** O código não conhece nenhum APM: emite log em JSON (CLEF) no console e
traces em OTLP; qualquer coletor de mercado (Seq local, Datadog, Elastic, Grafana
Tempo) recebe sem recompilação — só endpoint e protocolo, lidos do appsettings
via `IOptions` (`OpcoesDeRastreamento`).

Três decisões de apoio:

1. **Sink em console e Seq apenas.** Console é o denominador comum de qualquer
   plataforma (o coletor lê stdout). Seq é o visualizador local. Sem sink em
   arquivo: arquivo é rotação, permissão e disco para operar, sem ganho sobre
   stdout + coletor.
2. **Correlação pronta para Datadog.** O OpenTelemetry emite trace/span id em
   hexadecimal (W3C); o Datadog lê em decimal. Um enricher
   (`EnriquecedorDeCorrelacaoDatadog`) adiciona `dd.trace_id`/`dd.span_id` em
   decimal ao lado dos ids W3C, sem acoplar ao SDK do Datadog. Um APM que leia
   W3C ignora os campos `dd.*`; o Datadog usa-os direto.
3. **Log transversal por composição, não por herança de framework.** Log entra
   nos serviços por decorador registrado no DI (`DecoracaoDeServico.Decorar`),
   não editando cada serviço (OCP). Sem mediator: o produto ainda não tem
   pipeline de casos de uso que justifique a dependência.

Sanitização como defesa em profundidade: `RedatorDeCamposSensiveis` mascara
propriedades de nome sensível (senha, token, cpf...), reforçando a regra do
CLAUDE.md de nunca logar segredo ou dado pessoal.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| SDK do Datadog (ou de outro APM) direto | Amarra o código ao fornecedor antes de a hospedagem estar decidida; reverter custa reescrever a instrumentação |
| `Microsoft.Extensions.Logging` puro (como estava) | Sem enrichers ricos, sem correlação de trace, sem ecossistema de sinks; o `AddJsonConsole` só resolvia o formato |
| Sink em arquivo + rotação | Operação extra (disco, permissão, rotação) sem ganho sobre stdout num ambiente conteinerizado |
| Mediator com pipeline behaviors para o log | Dependência pesada para um produto sem casos de uso ainda; decorador no DI resolve com o que o framework já dá |

## Consequências

**Positivas**
- Troca de APM é configuração, não código. A decisão de hospedagem (ainda aberta)
  não fica bloqueada pela observabilidade.
- Log e trace correlacionados por `TraceId`/`SpanId` (e `dd.*` para o Datadog)
  desde a primeira linha, em qualquer ambiente.
- Novos serviços ganham log sem serem editados: só aderem ao decorador.

**Negativas**
- Duas tecnologias a entender (Serilog + OpenTelemetry) em vez de uma.
- CLEF usa campos `@t`/`@l`/`@mt`, não os nomes literais `timestamp`/`nivel`/
  `mensagem` do E1-F4-H1 — escolha consciente: interoperar com o ecossistema de
  ferramentas pesa mais que traduzir nomes de campo de protocolo. A linguagem
  ubíqua ([ADR-0004](0004-linguagem-ubiqua-em-portugues.md)) rege o domínio, não
  o formato de fio da telemetria.
- `organizacao_id` (o tenant_id) só aparece no log quando há sessão autenticada;
  enquanto a autenticação (E1-F2) não existe, o campo fica ausente por construção.

**Como saberemos que erramos:** se adotar um APM exigir mudar código de
instrumentação — e não só endpoint/protocolo —, a promessa de agnosticismo caiu e
este ADR precisa de sucessor.
