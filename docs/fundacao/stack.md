# Stack

> **Estado: DECIDIDA (backend).** .NET 10 + PostgreSQL, aceita em 2026-07-20.
> A decisão e os trade-offs avaliados estão em
> [ADR-0002](../adrs/0002-escolha-de-stack.md), com status *aceito*.
>
> Esta página é o espelho operacional daquele ADR. O front-end ainda não foi
> escolhido — será decidido quando a Fase 1 exigir a página de Magic Link.

## O que a escolha precisa atender

Requisitos derivados do [PRD](../prd.md), na ordem de peso. Qualquer candidata
tem que responder a todos:

| # | Necessidade | De onde vem |
| --- | --- | --- |
| 1 | **Jobs agendados e em fila**, idempotentes, com retentativa e backoff | Dunning (RF6), lembretes (RF4.4), relatórios (RF7) — o produto é assíncrono por natureza |
| 2 | **Webhook de entrada rápido** (< 500ms) que enfileira em vez de processar | RNF2 |
| 3 | **Web mobile-first** para o Magic Link, leve em 4G, sem app | RNF3, RNF15 |
| 4 | **Banco relacional** com isolamento por tenant garantido na camada de dados | RNF8, [multi-tenancy](multi-tenancy.md) |
| 5 | **Storage de objetos privado** com URL assinada de curta duração | RNF10 |
| 6 | **Tipagem forte** e teste sem rede nem relógio real | CLAUDE.md, RNF21 |
| 7 | **Operação simples** — equipe muito pequena, sem plantão | R6 |

## Como decidir

Não decida por preferência declarada. Faça uma **spike time-boxed** que prove o
caminho crítico ponta a ponta na candidata favorita:

> Receber um webhook, enfileirar um job, o job chamar um serviço externo falso,
> persistir com o tenant correto e uma página mobile ler o resultado — com
> teste automatizado, sem rede.

Se isso levar mais de dois dias na stack candidata, é sinal. Registre o
resultado no ADR-0002 e mude o status para *aceito*.

## Decisão (backend)

```
Linguagem / runtime:        .NET 10 (C#)
Framework web / API:        ASP.NET Core (Minimal API)
Camada de dados / migrações: EF Core 10 + Npgsql (escrita e migrações);
                             Dapper (leitura performática)
Fila e agendador:           a definir (Fase 0 — E1-F0-H4, ainda não implementado)
Storage de objetos:         a definir (entra na Fase 1, com o Dossiê)
Autenticação (biblioteca):  IdentityCore (sessão do painel); Magic Link próprio
Testes (runner + fakes):    xUnit; fakes nomeados escritos à mão (sem lib de mock)
Formatador / linter:        dotnet format
Comando único da suíte:     dotnet test (em morpheus-core/)
Hospedagem / deploy:        a definir
Banco:                      PostgreSQL 17
```

O que ainda está aberto (fila/agendador, storage, deploy, front-end) não bloqueia
a fundação de multi-tenancy e será fechado na fase que primeiro exigir cada peça.

## Registro de mudanças

| Data | Mudança |
| --- | --- |
| 2026-07-20 | Página criada; decisão deliberadamente adiada. Documentar a ausência da decisão evita que ela seja tomada por acidente, no primeiro commit, sem discussão |
| 2026-07-20 | Backend decidido: .NET 10 + PostgreSQL, EF Core + Dapper, IdentityCore. Fila, storage, deploy e front-end seguem em aberto |
