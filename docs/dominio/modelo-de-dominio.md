# Modelo de domínio

Vocabulário em [glossario.md](glossario.md). Este documento descreve **as
entidades, seus limites e suas máquinas de estado** — sem comprometer com
nenhuma tecnologia (a stack está em aberto, [ADR-0002](../adrs/0002-escolha-de-stack.md)).

## Contextos delimitados

O sistema tem quatro contextos com razões de mudança diferentes. Eles se
comunicam por **eventos de domínio**, não por chamada direta a repositório
alheio — é o que permite construir e substituir um sem quebrar os outros.

```mermaid
flowchart LR
    ID[Identidade e Acesso] -.->|contexto de todos| CAP

    CAP[Captação<br/>lead · conversa · triagem]
    AGE[Agenda<br/>visita · disponibilidade]
    DOC[Documentação<br/>dossiê · dunning]
    RAD[Radar<br/>feedback · relatório]

    CAP -->|LeadQualificado| AGE
    AGE -->|VisitaRealizada| RAD
    AGE -->|NegocioIniciado| DOC
    DOC -->|DossieCompletado| RAD
```

| Contexto | Uma razão para mudar | Entidades |
| --- | --- | --- |
| **Identidade e Acesso** | Regras de quem entra e o que pode | Tenant, Conta, Usuário, Papel |
| **Captação** | Regras de qualificação e canal de mensagem | Lead, Conversa, Mensagem, Qualificação |
| **Agenda** | Regras de disponibilidade e calendário | Visita, JanelaDeAtendimento, ContaDeCalendario |
| **Documentação** | Regras de checklist e cobrança | Dossiê, Participante, ItemDoChecklist, Anexo, MagicLink, Disparo |
| **Radar** | Regras de relatório ao proprietário | FeedbackDeVisita, RelatorioDoProprietario |

**Imóvel** e **Proprietário** são compartilhados: vivem num núcleo comum
referenciado por todos, com um único dono de escrita (Captação/cadastro).

## Invariantes globais

Regras que valem em todo lugar. Violá-las é bug de severidade máxima:

1. **Toda entidade pertence a exatamente um tenant.** Não existe linha órfã e
   não existe consulta sem filtro de tenant. Garantido na camada de dados —
   ver [multi-tenancy](../fundacao/multi-tenancy.md).
2. **Nenhuma referência cruza tenant.** Um dossiê nunca aponta para imóvel de
   outro tenant.
3. **Temperatura é derivada, nunca digitada.** Recalcular a partir da
   qualificação deve dar sempre o mesmo resultado.
4. **Anexo é imutável.** Correção é anexo novo; o anterior é arquivado, não
   sobrescrito — a trilha de auditoria depende disso.
5. **Toda transição de estado é registrada** com quem, quando e por quê.
6. **Todo efeito externo é idempotente** por chave de negócio. Reprocessar um
   job nunca envia mensagem duplicada.

## Entidades e relações

```mermaid
erDiagram
    TENANT ||--o{ USUARIO : tem
    TENANT ||--o{ IMOVEL : possui
    TENANT ||--o{ LEAD : recebe

    IMOVEL }o--|| PROPRIETARIO : pertence_a
    IMOVEL ||--o{ VISITA : sedia
    IMOVEL ||--o{ DOSSIE : origina

    LEAD ||--|| CONVERSA : conduz
    LEAD ||--o| QUALIFICACAO : produz
    LEAD ||--o{ VISITA : agenda
    CONVERSA ||--o{ MENSAGEM : contem

    USUARIO ||--o| CONTA_DE_CALENDARIO : conecta
    USUARIO ||--o| JANELA_DE_ATENDIMENTO : define
    USUARIO ||--o{ VISITA : conduz

    VISITA ||--o| FEEDBACK_DE_VISITA : gera
    PROPRIETARIO ||--o{ RELATORIO : recebe

    DOSSIE ||--|{ PARTICIPANTE : envolve
    DOSSIE }o--|| TEMPLATE_DE_CHECKLIST : segue
    PARTICIPANTE ||--|{ ITEM_DO_CHECKLIST : deve
    PARTICIPANTE ||--o{ MAGIC_LINK : acessa_por
    ITEM_DO_CHECKLIST ||--o{ ANEXO : recebe
    DOSSIE ||--o{ DISPARO : cobra_por
```

## Máquinas de estado

As transições abaixo são o contrato do domínio. Qualquer transição não
desenhada aqui é inválida e deve falhar com erro explícito, informando o
estado atual e os destinos permitidos.

### Lead

```mermaid
stateDiagram-v2
    [*] --> novo: lead recebido
    novo --> em_triagem: primeira resposta enviada
    em_triagem --> qualificado: fluxo concluído
    em_triagem --> sem_resposta: silêncio > 48h
    em_triagem --> descartado: pede para não ser contatado
    qualificado --> em_atendimento: transbordo ao corretor
    qualificado --> descartado: temperatura fria
    em_atendimento --> convertido: negócio iniciado
    em_atendimento --> perdido: desistiu
    sem_resposta --> em_triagem: respondeu depois
    convertido --> [*]
    perdido --> [*]
    descartado --> [*]
```

- `descartado` por pedido do próprio lead é **terminal e definitivo**: nenhum
  disparo automático posterior, em nenhum módulo. Exigência de LGPD e da
  qualidade do número (R2 do [PRD](../prd.md)).
- `qualificado → em_atendimento` só depois do transbordo; enquanto o bot
  conduz, o corretor não recebe notificação — esse é o ponto do produto.

### Visita

```mermaid
stateDiagram-v2
    [*] --> agendada: horário aceito pelo lead
    agendada --> confirmada: confirma no lembrete
    agendada --> cancelada
    confirmada --> realizada
    confirmada --> nao_compareceu
    agendada --> nao_compareceu
    realizada --> [*]
```

`realizada` e `nao_compareceu` alimentam o Radar; a segunda é métrica de
qualidade da triagem, não só estatística.

### Dossiê

```mermaid
stateDiagram-v2
    [*] --> aberto: criado a partir do template
    aberto --> em_analise: todos os itens enviados
    em_analise --> aberto: algum item rejeitado
    em_analise --> completo: todos os itens aprovados
    aberto --> cancelado
    em_analise --> cancelado
    completo --> [*]
```

O dossiê **não** tem estado próprio digitável: é sempre derivado da situação
dos itens de todos os participantes. Isso mantém uma única fonte de verdade e
elimina a classe de bug "dossiê completo com item pendente".

### Item do checklist

```mermaid
stateDiagram-v2
    [*] --> pendente
    pendente --> enviado: participante anexa
    enviado --> aprovado: corretor aprova
    enviado --> rejeitado: corretor rejeita com motivo
    rejeitado --> enviado: participante reenvia
    aprovado --> [*]
```

Rejeitar **exige** motivo não vazio: sem ele o participante repete o erro e o
ciclo se arrasta — exatamente a dor que o módulo existe para resolver.

## Eventos de domínio

Nomeados no pretérito, carregam id e tenant, publicados após a transação
persistir:

| Evento | Publicado por | Consumido por |
| --- | --- | --- |
| `LeadRecebido` | Captação | Triagem (dispara boas-vindas) |
| `LeadQualificado` | Captação | Agenda (oferece horários), Painel |
| `LeadDescartadoPorSolicitacao` | Captação | Todos (silencia disparos) |
| `VisitaAgendada` | Agenda | Lembretes |
| `VisitaRealizada` | Agenda | Radar (pede feedback) |
| `NegocioIniciado` | Agenda / Painel | Documentação (abre dossiê) |
| `ItemRejeitado` | Documentação | Dunning (reinicia cadência) |
| `DossieCompletado` | Documentação | Dunning (encerra), Painel |
| `FeedbackRegistrado` | Radar | Relatório |

## Decisões de modelagem

| Decisão | Motivo |
| --- | --- |
| Lead e Usuário são entidades separadas | Lead nunca autentica no painel; unir os dois convida a vazamento de permissão |
| Participante tem checklist próprio, não o dossiê | Fiador e locatário mandam coisas diferentes e não podem ver os documentos um do outro |
| Magic Link é entidade, não campo | Precisa de emissão múltipla, expiração, revogação e auditoria de uso |
| Anexo separado de Item | Um documento pode exigir várias páginas ou meses de extrato |
| Temperatura calculada, não armazenada como verdade | Mudou a regra, recalcula o histórico e a métrica continua comparável |
| Situação do dossiê derivada dos itens | Elimina divergência entre agregado e partes |
