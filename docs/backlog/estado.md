# Estado de implementação

Snapshot rápido do que já está em pé no código. A fonte de verdade dos critérios
é cada arquivo de épico; este arquivo só marca progresso. Legenda: ✅ concluída ·
◐ parcial · ☐ não iniciada.

_Atualizado em 2026-07-21._

## E1 — Core SaaS, multi-tenancy e assinaturas

- ◐ **E1-F0 — Esqueleto do projeto**
  - ◐ H1 — Spike de stack: ADR-0002 aceito (.NET 10 + PostgreSQL) e fatia de
    isolamento provada contra Postgres real; **falta** o caminho crítico
    completo (webhook → fila → job → serviço externo falso → leitura mobile) e
    os 3 `<a definir>` de `fundacao/stack.md`.
  - ✅ H2 — Projeto sobe e suíte roda: host com `/health`, validação de variável
    de ambiente obrigatória, `dotnet test` único e formatador aplicado.
  - ✅ H3 — CI: `.github/workflows/ci.yml` roda formatação + análise estática
    (`dotnet format`), build com `-warnaserror` e testes **unitários** em todo
    push/PR; depois de passar, build da imagem Docker (`Dockerfile` multi-stage).
    Testes de integração ficam fora do pipeline de propósito (exigem Postgres).
  - ◐ H4 — Orientação a eventos e outbox (lado de escrita): `EntidadeBase` com
    identidade, auditoria (VO `DadosDeAuditoria`) e eventos de domínio;
    `InterceptorDeGravacaoDeOutbox` grava o evento em `mensagens_outbox` na mesma
    transação do dado ([ADR-0009](../adrs/0009-eventos-de-dominio-e-outbox.md)).
    **Falta** a drenagem: dispatcher, filas, agendador e as garantias de job.
- ◐ **E1-F1 — Tenant e isolamento**
  - ✅ H1 — Isolamento na camada de dados: interceptor de escrita + filtro
    explícito de leitura, provados por testes de integração contra Postgres.
  - ☐ H2 — Criação de conta e tenant: sem fluxo de cadastro.
  - ✅ H3 — Chave única por tenant: índice único `(organizacao_id, codigo)` com
    teste de integração.
- ☐ **E1-F2 — Autenticação**: não iniciada.
- ☐ **E1-F3 — Autorização**: não iniciada.
- ◐ **E1-F4 — Observabilidade**
  - ✅ H1 — Log JSON estruturado: Serilog + OpenTelemetry, sinks console (CLEF) e
    Seq local, correlação `TraceId`/`SpanId` e `dd.trace_id`/`dd.span_id` para
    APM decimal (Datadog), `organizacao_id` (tenant_id) via `LogContext` quando
    há sessão, redator anti-vazamento com teste. Config no appsettings via
    `IOptions`; exportação OTLP agnóstica de fornecedor ([ADR-0008](../adrs/0008-observabilidade-agnostica.md)).
  - ◐ H2 — Tratamento de erro ponta a ponta: vocabulário de falha em pé
    (`Resultado`/`Resultado<T>` + `Erro`), exceções de domínio portando `Erro`
    e catálogos por agregado; **falta** a tradução HTTP (ProblemDetails) quando
    houver endpoints.
- ☐ **E1-F5 / E1-F6** — pós-MVP (Fase 5), não iniciadas.

## Decisões estruturais registradas

- Isolamento multi-tenant por defesa estrutural — [ADR-0003](../adrs/0003-isolamento-multi-tenant.md).
- Nomes de tabela em snake_case plural, sem prefixo `AspNet` do Identity; models
  no singular, tabelas no plural (`Organizacao` → `organizacoes`).
- Result pattern (`Resultado`/`Resultado<T>` + record `Erro` com código e
  descrição) como vocabulário único de falha entre camadas; exceção só para erro
  de fato, sempre herdando de `ErroDeRegraDeNegocio` e portando um `Erro`.
- Entidades nascem por factory (`Imovel.Cadastrar`, `Organizacao.Fundar` →
  `Resultado`) e voltam do banco por `Rehidratar` (reconstrução sem revalidar).
- Observabilidade agnóstica de fornecedor: Serilog (log CLEF) + OpenTelemetry
  (traces OTLP), Seq local, correlação pronta para Datadog e log transversal por
  decorador no DI (OCP), sem mediator — [ADR-0008](../adrs/0008-observabilidade-agnostica.md).
- Orientação a eventos com outbox no lado de escrita: `EntidadeBase` acumula
  eventos de domínio (com dados completos), gravados na mesma transação da escrita;
  drenagem (dispatcher/filas) fora do MVP — [ADR-0009](../adrs/0009-eventos-de-dominio-e-outbox.md).

## Próximo passo

Fechar a **E1-F0**: caminho crítico completo (H1) e a drenagem do outbox —
dispatcher, filas e agendador (H4) — antes de abrir novas estórias de F1.
