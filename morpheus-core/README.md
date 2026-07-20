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
# 1. Postgres local
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

## Rodar a suíte

```bash
dotnet test
```

Os testes unitários rodam em milissegundos, sem banco e sem rede (F.I.R.S.T.).
Os testes de integração sobem um PostgreSQL efêmero via Testcontainers e provam
o isolamento por organização contra o banco real — por isso **exigem Docker**
(o mesmo pré-requisito de subir o ambiente local).
