# Backlog

Épicos → features → estórias de usuário. Sequência de execução no
[roadmap](../roadmap.md); justificativa de negócio no [PRD](../prd.md).
Progresso de implementação em [estado.md](estado.md).

## Épicos

| # | Épico | No MVP | Fase |
| --- | --- | :-: | --- |
| [E1](e1-core-saas.md) | Core SaaS, multi-tenancy e assinaturas | parcial | 0, 5 |
| [E2](e2-ingestao.md) | Ingestão de leads e cadastro de imóveis | parcial | 1, 2, 6 |
| [E3](e3-triagem-whatsapp.md) | Motor SDR autônomo via WhatsApp | parcial | 2 |
| [E4](e4-agenda.md) | Agenda inteligente e agendamento autônomo | parcial | 3 |
| [E5](e5-dossie-digital.md) | Dossiê Digital e esteira documental | parcial | 1 |
| [E6](e6-dunning.md) | Cobrança documental automática | parcial | 1 |
| [E7](e7-radar-proprietario.md) | Radar do Proprietário | parcial | 4 |
| [E8](e8-analytics.md) | Painel e analytics | parcial | 1, 8 |

## Convenções

**Identificação:** `E<épico>-F<feature>-H<estória>`. Ex.: `E5-F2-H3`. IDs são
permanentes — estória removida some da lista, mas o número não é reciclado.

**Formato da estória:**

> **Como** \<persona do [PRD](../prd.md#13-público-alvo)\>
> **quero** \<capacidade\>
> **para** \<benefício mensurável\>

**Marcação:** `[MVP]` entra no primeiro corte; sem marcação é pós-MVP.

**Critérios de aceite** em Dado/Quando/Então, escritos para virarem teste
automatizado — não descrição de tela. Um critério que não vira teste é
suspeito.

## Definição de pronto

Uma estória só está pronta quando **tudo** abaixo é verdade. Sem exceção por
prazo — a regra existe justamente para o dia em que o prazo aperta.

- [ ] Critérios de aceite passam em teste automatizado.
- [ ] Regras de código do [CLAUDE.md](../../CLAUDE.md) respeitadas: funções de
      4–20 linhas, arquivos < 300, tipos explícitos, sem duplicação, no máximo
      2 níveis de indentação.
- [ ] Teste negativo de autorização quando a estória expõe endpoint.
- [ ] Filtro de tenant verificado — inclusive em job de background.
- [ ] I/O externo atrás de porta ([ADR-0007](../adrs/0007-integracoes-atras-de-porta.md)),
      com fake nomeado no teste. Suíte roda sem rede e sem relógio real.
- [ ] Erro carrega contexto: o valor que falhou e o formato esperado.
- [ ] Log em JSON estruturado com `tenant_id`, sem segredo e sem dado pessoal.
- [ ] Nenhum segredo e nenhum dado real de cliente no diff.
- [ ] Documentação afetada atualizada no **mesmo PR**.
- [ ] Commit em Conventional Commits com assunto em português BR.

## Estimativa

Pontos relativos (1, 2, 3, 5, 8). **8 é sinal de que a estória deve ser
quebrada**, não de que é grande. Estimativas serão revisadas quando a stack
fechar ([ADR-0002](../adrs/0002-escolha-de-stack.md)) — antes disso são
comparativas entre si, não previsão de calendário.
