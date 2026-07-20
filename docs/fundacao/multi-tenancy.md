# Multi-tenancy

**Decisão:** banco único, schema único, coluna `tenant_id` em toda tabela de
negócio, com isolamento **imposto na camada de dados** — não confiado ao
cuidado de quem escreve a query.
Trade-offs em [ADR-0003](../adrs/0003-isolamento-multi-tenant.md).

## Por que isolamento no dado, e não só na aplicação

Isolamento por `WHERE tenant_id = ?` espalhado pelo código funciona até o dia
em que alguém esquece um. Esse dia chega. O custo do esquecimento aqui não é um
bug — é vazamento de documento de identidade de um cliente para o concorrente
dele, com dever de notificação à ANPD.

Portanto: **a query sem filtro de tenant não deve retornar nada errado — ela
deve ser impossível.** Row-Level Security no PostgreSQL, ou o mecanismo
equivalente da stack escolhida, com um teste que prova que funciona.

## Regras

1. **Toda tabela de negócio tem `tenant_id` não nulo.** Exceções: tabelas
   globais de configuração sem dado de cliente (ex.: catálogo de templates de
   checklist do sistema). Exceção precisa ser justificada em PR.
2. **Chave única é composta com o tenant.** O código de referência do imóvel é
   único *por tenant*, não globalmente — dois clientes usam "AP-101" sem
   conflito.
3. **Chave estrangeira nunca cruza tenant.** Garantido por constraint composta,
   não por validação na aplicação.
4. **O tenant vem do contexto autenticado**, jamais de parâmetro da requisição.
   `tenant_id` no corpo ou na query string de um endpoint autenticado é vetor
   de escalada — se aparecer num PR, é bloqueio.
5. **Job de background carrega o tenant explicitamente** no payload e o aplica
   ao contexto antes de tocar o banco. É onde vazamento passa despercebido:
   não há usuário logado para "dar certo por acaso".
6. **Toda linha de log tem `tenant_id`.** Sem ele não se investiga incidente.
7. **Storage é particionado por tenant** no caminho do objeto
   (`{tenant_id}/{dossie_id}/{anexo_id}`) e o acesso é sempre por URL assinada
   de curta duração, nunca por bucket público.

## Como o tenant chega ao contexto

```
Requisição HTTP  → sessão autenticada → tenant do usuário → contexto → dados
Magic Link       → token válido       → tenant do participante → contexto → dados
Job em fila      → payload do job     → tenant do job → contexto → dados
Webhook externo  → identificação da conta de canal → tenant → contexto → dados
```

Os quatro caminhos convergem para **um único ponto** que define o tenant do
contexto. Um ponto, uma responsabilidade, um lugar para auditar.

O caminho do webhook merece atenção: a requisição chega **sem** usuário. O
tenant é resolvido pelo número de WhatsApp de destino ou pela chave do
endpoint. Se não resolver, a mensagem vai para uma quarentena e alerta — nunca
para um tenant "padrão".

## Testes obrigatórios

Sem estes testes verdes, a fundação não está pronta:

| Teste | Prova |
| --- | --- |
| Consulta com contexto do tenant A não retorna linha do tenant B | Isolamento de leitura |
| Consulta **sem** contexto de tenant falha ou retorna vazio | O default é seguro, não permissivo |
| Escrita com `tenant_id` divergente do contexto é rejeitada | Isolamento de escrita |
| FK apontando para entidade de outro tenant é rejeitada | Integridade referencial entre tenants |
| Job sem tenant no payload falha ao iniciar, não roda "global" | Cobre o caminho assíncrono |
| Mesmo código de referência de imóvel em dois tenants coexiste | Unicidade composta correta |
| Magic Link do tenant A não acessa dossiê do tenant B | Isolamento no caminho sem login |

## Adiado (pós-MVP)

- **Hierarquia dentro do tenant** — equipes com corretores vendo apenas os
  próprios leads. Modelar cedo demais engessa; o dado de quem é responsável já
  fica registrado desde o MVP para não exigir migração depois.
- **Banco dedicado por tenant** para cliente grande. Só se aparecer exigência
  contratual; não antecipe.
