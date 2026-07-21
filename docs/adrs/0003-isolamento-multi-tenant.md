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

## Emenda — mecanismo de imposição adotado (2026-07-20)

Com a stack decidida ([ADR-0002](0002-escolha-de-stack.md): .NET 10 + EF Core +
Dapper), a **defesa aceita é o filtro explícito imposto por construção**, e não a
RLS do PostgreSQL. A entidade de tenant chama-se `Organizacao`; a coluna de
vínculo é `organizacao_id`, indexada em toda tabela de negócio.

O ponto-chave é que "filtro explícito" aqui **não** é o `WHERE tenant_id = ?`
manual que a tabela de alternativas abaixo reprova. Ele é estrutural:

- **Leitura** só existe através de um repositório/consulta que injeta o filtro
  (`FiltroDaOrganizacao.DaOrganizacao` no EF; cláusula `organizacao_id =
  @OrganizacaoId` obrigatória no SQL Dapper, coberta por teste). Não há caminho
  de leitura que dispense o filtro — esquecê-lo exige contornar deliberadamente.
- **Escrita** passa por um interceptor de `SaveChanges` que carimba a organização
  do contexto e rejeita vínculo divergente. Sem contexto (job/bootstrap), exige
  vínculo explícito — nunca grava "global".
- **Não** se usa query filter global no model creating: o filtro é explícito e
  auditável em cada consulta, mas não depende da disciplina de quem a escreve.

Isso satisfaz o espírito da decisão ("o caminho seguro é o caminho padrão") sem
acoplar o isolamento à conexão de banco. A **RLS do PostgreSQL permanece
disponível como backstop de defesa em profundidade** e será adotada se o modelo
de ameaça ou uma exigência de cliente pedir — sem desmontar o mecanismo atual.

Por que sem RLS agora: a RLS acopla o isolamento à gestão de sessão de conexão
(`SET LOCAL`), custo que só se paga quando a defesa em profundidade for
requisito, não hipótese. A porta fica aberta.

## Emenda — vínculo de escrita definido na construção, não por interceptor (2026-07-21)

A emenda anterior colocou a escrita atrás de um **interceptor de `SaveChanges`** que
carimbava a organização do contexto. Na prática, isso deu uma sensação de "mágica"
(efeito registrado longe, no contêiner) e — mais importante — **acoplava o isolamento
de escrita ao EF Core**: um `SaveChangesInterceptor` morre numa eventual troca de banco
(Mongo, outro provedor), levando junto a garantia.

Isolamento de tenant é responsabilidade da **aplicação e do domínio**, não da
infraestrutura. Então o mecanismo de escrita passa a ser:

- **O vínculo é definido na construção da entidade.** `Imovel.Cadastrar` recebe um
  value object `OrganizacaoId` (invariante: nunca vazio) e a entidade nasce vinculada —
  "parse, don't validate". Sem setter de organização: o vínculo é imutável por
  construção, não por checagem em runtime.
- **Quem cadastra define o tenant, a partir do contexto autenticado** (`OrganizacaoId`
  resolvido no escopo da requisição). A persistência apenas grava; o repositório não
  carimba mais nada.
- **O interceptor de escrita foi removido.** Some também `RegraDeVinculoComOrganizacao`,
  `ErroDeOrganizacaoDivergente` e `ErroDeEscritaSemOrganizacao` — código sem dono.
- **Backstop estrutural que sobrevive à troca de banco:** a coluna `organizacao_id` é
  `NOT NULL` com FK para `organizacoes`. Uma entidade sem tenant é impossível de
  construir; uma com tenant inexistente é rejeitada pela FK no commit.

**Tradeoff assumido:** perde-se a comparação, no boundary de persistência, entre o tenant
da entidade e o tenant do contexto — a barreira que o interceptor fazia contra uma
escrita na organização errada que **burlasse a factory de domínio**. Aceitável porque o
`OrganizacaoId` vem de uma fonte única (a identidade autenticada, resolvida no escopo) e
é obrigatório na construção: não há caminho de escrita que dispense informá-lo, e informá-lo
errado exige contornar deliberadamente a factory. Um guard explícito (entidade × contexto)
antes do commit foi considerado excesso de defensividade para este cenário e fica anotado
como reforço disponível caso o modelo de ameaça peça. A leitura permanece como antes:
filtro explícito e estrutural no repositório.

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
