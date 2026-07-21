# morpheus-core

Backend da suíte morpheus-imob. Fundação do MVP: multi-tenancy por organização,
identidade e acesso a dados. Contexto de produto e regras em
[../CLAUDE.md](../CLAUDE.md) e [../docs](../docs/README.md).

## Stack

| Camada | Escolha |
| --- | --- |
| Runtime | .NET 10 |
| API | ASP.NET Core (Minimal API) |
| Dados / migrações | EF Core 10 + Npgsql |
| Consultas performáticas | Dapper |
| Banco | PostgreSQL 17 |
| Identidade | IdentityCore |
| Observabilidade | Serilog (log CLEF) + OpenTelemetry (traces OTLP); Seq local |
| Testes | xUnit |

Detalhes e trade-offs em [../docs/fundacao/stack.md](../docs/fundacao/stack.md)
e [ADR-0002](../docs/adrs/0002-escolha-de-stack.md).

## Estrutura

```
src/
  Morpheus.Dominio         Entidades, contratos e regras puras (sem SDK)
  Morpheus.Aplicacao       Casos de uso, contexto e cache de organização
  Morpheus.Infraestrutura  EF Core, Dapper, Identity, cache, isolamento
  Morpheus.Api             Host HTTP, validação de ambiente, /health
tests/
  Morpheus.Testes.Unitarios  Testes unitários (sem banco, sem rede)
  Morpheus.Testes.Integracao Testes de integração (Postgres real via Testcontainers)
```

## Isolamento por organização

`Organizacao` é o tenant. O isolamento é **filtro explícito, imposto por
construção** (ver [ADR-0003](../docs/adrs/0003-isolamento-multi-tenant.md)):

- Leitura EF passa por `FiltroDaOrganizacao.DaOrganizacao`; leitura Dapper
  carrega `WHERE organizacao_id = @OrganizacaoId` sempre explícito.
- Escrita é carimbada/validada pelo `InterceptorDeEscritaPorOrganizacao`.
- A organização do usuário autenticado é resolvida uma vez e mantida em cache
  (`ResolvedorDaOrganizacaoDoUsuario`).
- `organizacao_id` é indexado em toda tabela de negócio.

Não há query filter global no model creating — o filtro é explícito e estrutural.

## Subir o ambiente local

Pré-requisitos: .NET 10 SDK e Docker.

```bash
# 1. Postgres e Seq (visualizador de logs/traces) locais
docker compose up -d

# 2. Configuração obrigatória
cp .env.example .env            # ajuste a senha se quiser
export $(grep -v '^#' .env | xargs)

# 3. Migrações
dotnet dotnet-ef database update \
  --project src/Morpheus.Infraestrutura \
  --startup-project src/Morpheus.Infraestrutura

# 4. API
dotnet run --project src/Morpheus.Api
# GET /health responde { "status": "ok" }
```

A API **falha ao subir** sem `MORPHEUS_BANCO_CONEXAO`, dizendo qual variável
falta e o formato esperado.

## Observabilidade

Log estruturado (Serilog, formato CLEF) e rastreamento distribuído
(OpenTelemetry) agnósticos de fornecedor — trocar de APM é só endpoint e
protocolo, sem tocar em código ([ADR-0008](../docs/adrs/0008-observabilidade-agnostica.md)).

- **Console** recebe log em JSON em qualquer ambiente (o coletor lê stdout).
- **Seq** local, em <http://localhost:5341>, recebe log e traces em dev via
  `appsettings.Development.json`. Nada de segredo: é ambiente de máquina.
- Cada linha carrega correlação `TraceId`/`SpanId` e, para APM que lê decimal
  (Datadog), `dd.trace_id`/`dd.span_id`. Com sessão autenticada, o log ganha
  `organizacao_id` (o tenant).
- Log transversal entra por decorador no DI (`Decorar<,>`), sem editar o
  serviço observado (OCP). `RedatorDeCamposSensiveis` mascara campo sensível.
- Em produção, aponte `Rastreamento__EndpointOtlp` (em `.env`) para o seu APM.

## Rodar a suíte

```bash
dotnet test
```

Os testes unitários rodam em milissegundos, sem banco e sem rede (F.I.R.S.T.).
Os testes de integração sobem um PostgreSQL efêmero via Testcontainers e provam
o isolamento por organização contra o banco real — por isso **exigem Docker**
(o mesmo pré-requisito de subir o ambiente local).
