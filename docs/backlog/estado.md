# Estado de implementação

Snapshot rápido do que já está em pé no código. A fonte de verdade dos critérios
é cada arquivo de épico; este arquivo só marca progresso. Legenda: ✅ concluída ·
◐ parcial · ☐ não iniciada.

_Atualizado em 2026-07-21 (onboarding, confirmação de e-mail, e-mail
transacional real e tratamento de erro ponta a ponta)._

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
    override explícito de `SaveChanges` grava o evento em `mensagens_outbox` na
    mesma transação do dado ([ADR-0009](../adrs/0009-eventos-de-dominio-e-outbox.md)).
    **Falta** a drenagem: dispatcher, filas, agendador e as garantias de job.
- ✅ **E1-F1 — Tenant e isolamento**
  - ✅ H1 — Isolamento na camada de dados: interceptor de escrita + filtro
    explícito de leitura, provados por testes de integração contra Postgres.
  - ✅ H2 — Criação de conta e tenant: `POST /contas` funda organização + usuário
    dono na mesma transação, com papel no Identity, configuração padrão (fuso
    `America/Sao_Paulo`, janela 9h–18h) e eventos no outbox. Resposta idêntica
    para conta criada, e-mail repetido e armadilha de robô.
  - ✅ H3 — Chave única por tenant: índice único `(organizacao_id, codigo)` com
    teste de integração.
  - ✅ H4 — Onboarding: `PATCH /organizacao` renomeia (`tenant.configurar`, só o
    dono), `PATCH /organizacao/usuarios/atual` completa o próprio nome
    (qualquer papel). Autor e horário auditados por `OrganizacaoRenomeadaEvento`
    no outbox — `DadosDeAuditoria` não ganhou campo de autor, o "quem" é fato de
    negócio, não coluna. Nenhuma das duas rotas é obrigatória.
- ◐ **E1-F2 — Autenticação**
  - ✅ H1 — Login por e-mail e senha: cookie opaco + sessão no servidor
    ([ADR-0011](../adrs/0011-sessao-opaca-no-servidor.md)), senha em Argon2id,
    recusas indistinguíveis, limite por origem e bloqueio por conta.
  - ☐ H2 — Login com Google: adiado por decisão, entra sobre o que já existe.
  - ✅ H3 — Recuperação de senha: resposta genérica, token de uso único com 30 min
    e queda de todas as sessões na troca. Provedor de e-mail real em pé desde
    esta revisão (ver abaixo) — `IEnvioDeEmailDeRecuperacao` não é mais só log.
  - ✅ H4 — Encerrar sessão: revogação no servidor, só do aparelho que saiu.
  - ☐ H5 — MFA por TOTP: pós-MVP.
  - ✅ H6 — Confirmar e-mail no cadastro: token emitido e enviado ao criar a
    conta, sem bloquear login; reenvio gira o carimbo de segurança do Identity,
    invalidando o token anterior — mesmo mecanismo que a recuperação de senha.
- ◐ **E1-F3 — Autorização**
  - ✅ H1 — Verificação central: ponto único `Pode(usuario, permissao)`, política
    por permissão registrada na subida, rota sem declaração **derruba o processo**,
    401 sem sessão, negação logada e nenhum contrato aceitando tenant.
  - ✅ H2 — Papéis dono e corretor: enum removido; papéis em `user_roles` e
    permissões em `role_claims`, semeadas por migração
    ([ADR-0010](../adrs/0010-papeis-e-permissoes-no-identity.md)), com teste
    negativo por permissão restrita.
  - ☐ H3 / H4 — Equipe ampliada e vínculo por responsável: pós-MVP.
- ◐ **E1-F4 — Observabilidade**
  - ✅ H1 — Log JSON estruturado: Serilog + OpenTelemetry, sinks console (CLEF) e
    Seq local, correlação `TraceId`/`SpanId` e `dd.trace_id`/`dd.span_id` para
    APM decimal (Datadog), `organizacao_id` (tenant_id) via `LogContext` quando
    há sessão, redator anti-vazamento com teste. Config no appsettings via
    `IOptions`; exportação OTLP agnóstica de fornecedor ([ADR-0008](../adrs/0008-observabilidade-agnostica.md)).
  - ✅ H2 — Tratamento de erro ponta a ponta: `ManipuladorGlobalDeExcecoes`
    (`IExceptionHandler`, primeiro middleware do pipeline) captura exceção não
    tratada de qualquer rota — ProblemDetails genérico para o cliente, exceção
    completa para o log — reaproveitando `RespostaDeFalha`, o mesmo ponto único
    que já traduz `Resultado` de falha. `traceId` do `AddProblemDetails()`
    removido de propósito para não quebrar a resposta byte-idêntica contra
    enumeração de contas.
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
- Papel do usuário e permissões vivem no IdentityCore (`user_roles`/`role_claims`,
  semeados por migração), não em enum — [ADR-0010](../adrs/0010-papeis-e-permissoes-no-identity.md).
- Sessão opaca no servidor (tabela `sessoes` atrás de `ITicketStore`): cookie
  carrega só um id, revogação é apagar linha — [ADR-0011](../adrs/0011-sessao-opaca-no-servidor.md).
- Endpoints em Minimal API, cada grupo implementando `IEndpoint` e listado
  explicitamente em `MapeamentoDeEndpoints` — sem varredura de assembly, sem
  controllers, e com verificação na subida de que toda rota declarou seu acesso.

## Próximo passo

1. **Fechar a E1-F0** — caminho crítico completo (H1) e a drenagem do outbox:
   dispatcher, filas e agendador (H4). O agendador também resolve a varredura de
   sessões expiradas.
2. **E1-F2-H2 — login com Google**, agora que o onboarding (E1-F1-H4) e a
   confirmação de e-mail (E1-F2-H6) — os dois itens que a bloqueavam por
   decisão — estão em pé.
3. **E2-F1 — completar cadastro de imóveis** (listar/buscar, editar, registrar
   proprietário) — pré-requisito direto do Dossiê Digital (E5).
