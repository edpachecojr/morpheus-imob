# ADR-0003 — Isolamento multi-tenant por coluna, imposto na camada de dados

**Status:** Aceito
**Data:** 2026-07-20
**Requisito:** RNF8 do [PRD](../prd.md) · detalhe em [multi-tenancy](../fundacao/multi-tenancy.md)

## Contexto

O produto é SaaS: várias imobiliárias e corretores independentes no mesmo
sistema, guardando documento de identidade e comprovante de renda de terceiros.
Vazamento entre tenants não é bug de funcionalidade — é incidente de dados
pessoais, com dever de notificação à ANPD e fim da confiança no produto.

A equipe é muito pequena e não terá plantão nem DBA. O isolamento precisa ser
estrutural, porque o mecanismo que depende de disciplina humana em cada query
falha na query número quatrocentos, escrita às onze da noite.

## Decisão

Banco único, schema único, coluna `tenant_id` obrigatória em toda tabela de
negócio, com o isolamento **imposto pela camada de dados** — Row-Level Security
do PostgreSQL, ou o mecanismo equivalente da stack escolhida.

O `tenant_id` do contexto vem sempre da identidade autenticada (sessão, magic
link, payload do job ou resolução do webhook), nunca de parâmetro da
requisição. Consulta sem tenant no contexto não retorna dado.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| Filtro na aplicação (`WHERE tenant_id = ?` manual) | Um esquecimento basta. O modo de falha é silencioso e o custo é catastrófico. Pode complementar, nunca ser o único mecanismo |
| Schema por tenant | Migração multiplicada por cliente; ferramental de migração sofre; ganho de isolamento pequeno frente ao RLS |
| Banco por tenant | Isolamento máximo, custo e operação incompatíveis com uma equipe desta dimensão e com corretor autônomo pagando assinatura pequena |
| Silo por cliente (instância dedicada) | Inviável no modelo de preço e no volume esperado |

## Consequências

**Positivas**
- O caminho seguro é o caminho padrão; vazar exige contornar deliberadamente.
- Uma migração serve a todos os clientes.
- Custo por tenant baixo, compatível com corretor autônomo.
- Cobre naturalmente jobs e webhooks — onde não há usuário logado para "dar
  certo por acaso".

**Negativas**
- Acopla a decisão à capacidade do banco. Se a stack escolhida não oferecer RLS
  ou equivalente confiável, este ADR precisa ser revisto — não contornado.
- Ruído "barulhento": query pesada de um tenant afeta os outros. Aceitável no
  volume previsto; mitigável com índices por `tenant_id` e limites.
- Chaves compostas em toda parte (`(tenant_id, codigo_referencia)`) tornam o
  schema um pouco mais verboso.
- Exige cuidado com toda operação administrativa que legitimamente cruza
  tenants (métricas agregadas do próprio negócio): ela contorna o RLS e precisa
  de caminho separado, explícito e auditado.

**Como saberemos que erramos:** se um cliente grande exigir contratualmente
banco dedicado, ou se o ruído entre tenants começar a aparecer em latência,
escreva um ADR sucessor para isolamento híbrido — sem desmontar o modelo para
todo mundo.
