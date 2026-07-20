# PRD — morpheus-imob

> **Status:** vivo · **Última revisão:** 2026-07-20 · **Fase:** fundação
> Escopo detalhado em [backlog/](backlog/README.md) · sequência em [roadmap.md](roadmap.md)

---

## 1. Contexto e problema

### 1.1 Declaração do problema

Pequenas imobiliárias e corretores autônomos perdem negócio em três pontos
distintos da jornada, todos por **latência humana**, não por falta de demanda:

**a) O lead esfria antes do primeiro contato.**
Leads de portais (Zap, VivaReal, OLX) e de anúncios no Instagram chegam em
horários em que ninguém está atendendo. Quando o corretor responde — horas ou
um dia depois — o interessado já falou com três concorrentes. O corretor
também não sabe, antes de investir tempo, se o lead tem renda, urgência ou
forma de pagamento viável: ele agenda visita para curioso e queima sábado.

**b) A burocracia documental trava o fechamento.**
Depois do "quero fechar", começa a coleta de RG, CPF, comprovante de renda,
comprovante de residência e certidões — via WhatsApp, e-mail e foto tremida.
O corretor vira cobrador: relembra manualmente, perde o controle de quem já
mandou o quê, recebe documento ilegível e refaz o pedido. Processos que
poderiam fechar em 3 dias arrastam por 3 semanas, e alguns morrem no caminho.

**c) O proprietário cancela o contrato por silêncio.**
O dono do imóvel não sabe se houve visita, o que os visitantes acharam, nem
por que não vendeu. Sem informação, ele conclui que a imobiliária não trabalha
e leva o imóvel para a concorrência. O corretor até tem os feedbacks — estão
na cabeça dele e no histórico do WhatsApp, sem virar relatório.

CRMs imobiliários tradicionais são **catálogos estáticos**: guardam imóvel e
contato, mas não agem. Exigem que o humano lembre de fazer as coisas. O
gargalo não é armazenamento — é execução.

### 1.2 Proposta de valor

Um **funcionário digital** que trabalha 24/7 na pré-venda, no burocrático e no
pós-venda: qualifica e agenda sozinho, cobra documento sozinho, e presta
contas ao proprietário sozinho. Não é onde o corretor *anota* o trabalho — é
quem *faz* a parte repetitiva dele.

### 1.3 Público-alvo

| Persona | Quem é | Dor principal | O que ganha |
| --- | --- | --- | --- |
| **Corretor autônomo** (usuário primário no MVP) | Trabalha sozinho, 20–60 leads/mês, vive no WhatsApp do celular | Não consegue responder rápido nem cobrar documento enquanto está na rua | Primeira resposta em segundos e cobrança automática rodando sem ele |
| **Dono de imobiliária pequena** (2–15 corretores) | Compra a ferramenta, não a usa no dia a dia | Não sabe quem está atendendo bem nem por que a carteira encolhe | Distribuição de lead, visibilidade do funil e retenção de proprietário |
| **Secretária / administrativo** | Faz triagem manual e coleta documento | Trabalho repetitivo de cobrança e conferência | Painel de revisão em vez de caixa de entrada |
| **Lead / interessado** (usuário indireto) | Está no WhatsApp, no celular, com pressa | Espera resposta, não recebe | Resposta imediata e agendamento sem ping-pong |
| **Cliente em fechamento** (locatário, comprador, fiador) | Precisa enviar documentos | Não sabe o que falta nem para onde mandar | Link único, checklist visível, upload por foto |
| **Proprietário do imóvel** | Confiou o imóvel à imobiliária | Silêncio total | Relatório periódico com visitas e feedbacks |

### 1.4 Por que agora

O WhatsApp é o canal de fato do mercado imobiliário brasileiro e sua API
oficial (Cloud API) está acessível a pequenos negócios. A automação
conversacional deixou de exigir infraestrutura de empresa grande — o que
antes era diferencial de rede nacional agora cabe numa assinatura mensal.

---

## 2. Objetivos e critérios de sucesso

### 2.1 Objetivos de produto

| # | Objetivo | Critério de sucesso |
| --- | --- | --- |
| O1 | Zerar o tempo de primeira resposta ao lead | p95 do tempo entre chegada do lead e primeira mensagem enviada **< 60s** |
| O2 | Filtrar lead improdutivo antes de consumir a agenda do corretor | ≥ 70% dos leads recebidos são classificados automaticamente sem intervenção humana |
| O3 | Encurtar o ciclo documental | Mediana do tempo entre abertura do dossiê e "completo" **< 5 dias** |
| O4 | Reduzir a cobrança manual de documento | ≥ 60% dos dossiês completados **sem** nenhuma mensagem manual do corretor |
| O5 | Reter carteira de imóveis via transparência | ≥ 1 relatório enviado ao proprietário por imóvel ativo a cada 15 dias |

### 2.2 Objetivos de negócio (validação do SaaS)

| # | Objetivo | Critério de sucesso |
| --- | --- | --- |
| N1 | Validar disposição a pagar | **5 a 10 clientes pagantes** ao fim do MVP |
| N2 | Provar retenção | Churn mensal **< 8%** nos primeiros 3 meses após o MVP |
| N3 | Provar que o produto é usado, não só assinado | ≥ 60% das contas ativas usam ≥ 2 dos 3 módulos semanalmente |

### 2.3 KPIs rastreados

**Produto (medidos em telemetria do próprio sistema):**

- `tempo_primeira_resposta_p50 / p95` — do recebimento do lead ao primeiro envio.
- `taxa_qualificacao_automatica` — leads classificados sem toque humano ÷ total.
- `taxa_agendamento` — visitas agendadas ÷ leads quentes.
- `taxa_comparecimento` — visitas realizadas ÷ visitas agendadas.
- `duracao_dossie_p50` — abertura → status completo.
- `taxa_autoatendimento_documental` — dossiês completos sem mensagem manual.
- `lembretes_ate_conclusao` — quantos disparos de cobrança até o dossiê fechar.
- `taxa_rejeicao_documento` — proxy da qualidade das instruções de upload.
- `cobertura_relatorio_proprietario` — imóveis ativos com relatório nos últimos 15 dias.

**Negócio:** contas pagantes, MRR, churn mensal, ativação (conta que completou
o primeiro dossiê ou o primeiro agendamento em 7 dias).

**Contra-métricas** (o que não pode piorar enquanto otimizamos o resto):

- `taxa_escalonamento_indevido` — lead quente classificado como frio pelo bot.
- `taxa_opt_out_whatsapp` — bloqueios e descadastros; mede se a cobrança
  automática está sendo percebida como spam.
- `qualidade_da_conta_whatsapp` — rating do número na Meta. Queda aqui é
  incidente, não métrica.

### 2.4 Como saberemos que falhou

Sinais que invalidam a tese e exigem replanejamento, não ajuste fino:

- Corretores desligam a automação e voltam a responder manualmente.
- Leads reclamam de "falar com robô" a ponto de derrubar a taxa de agendamento
  abaixo do baseline manual.
- Clientes preferem mandar documento por WhatsApp mesmo com o Magic Link.

---

## 3. Escopo e requisitos

### 3.1 Requisitos funcionais

Cada bloco corresponde a um épico no [backlog](backlog/README.md). `[MVP]`
marca o que entra no primeiro corte vendável.

**RF1 — Contas, tenants e acesso** ([E1](backlog/e1-core-saas.md))
- RF1.1 `[MVP]` Criar conta e autenticar por e-mail/senha e login com Google.
- RF1.2 `[MVP]` Todo dado pertence a exatamente um tenant, com isolamento verificável.
- RF1.3 `[MVP]` Papéis: dono, corretor. (Gestor e secretária: pós-MVP.)
- RF1.4 Assinatura recorrente com bloqueio automático por inadimplência.
- RF1.5 White-label básico (logo e cor) em links públicos e relatórios.

**RF2 — Ingestão de leads e cadastro de imóveis** ([E2](backlog/e2-ingestao.md))
- RF2.1 `[MVP]` Cadastro manual de imóvel: referência, título, endereço, link do portal.
- RF2.2 `[MVP]` Receber lead por link parametrizado (bio do Instagram, botão do site).
- RF2.3 `[MVP]` Receber lead por webhook genérico normalizado.
- RF2.4 Conectores nomeados por portal e Meta Ads.
- RF2.5 Roteamento entre corretores (round-robin, por imóvel, por responsável).
- RF2.6 Importação de imóvel por XML/link de portal.

**RF3 — Triagem e qualificação por WhatsApp** ([E3](backlog/e3-triagem-whatsapp.md))
- RF3.1 `[MVP]` Conectar um número de WhatsApp por tenant e receber/enviar mensagens.
- RF3.2 `[MVP]` Responder todo lead novo em < 60s com boas-vindas contextualizada no imóvel.
- RF3.3 `[MVP]` Fluxo de qualificação em árvore de decisão: urgência, forma de pagamento, faixa de renda.
- RF3.4 `[MVP]` Classificar temperatura (quente/morno/frio) por regra determinística e auditável.
- RF3.5 `[MVP]` Transbordar para o corretor humano em lead quente ou pedido explícito.
- RF3.6 Fluxo conversacional configurável pelo tenant.
- RF3.7 Interpretação por LLM em vez de árvore fixa.

**RF4 — Agenda e agendamento autônomo** ([E4](backlog/e4-agenda.md))
- RF4.1 `[MVP]` Conectar Google Calendar do corretor via OAuth.
- RF4.2 `[MVP]` Calcular horários livres respeitando janela de atendimento e duração da visita.
- RF4.3 `[MVP]` Oferecer 3 horários ao lead quente e criar o evento na confirmação.
- RF4.4 `[MVP]` Lembrete automático 2h antes da visita.
- RF4.5 Reagendamento e cancelamento pelo lead, com sincronização bidirecional.
- RF4.6 Buffer de deslocamento entre visitas; Outlook.

**RF5 — Dossiê Digital** ([E5](backlog/e5-dossie-digital.md))
- RF5.1 `[MVP]` Abrir dossiê a partir de um template de checklist.
- RF5.2 `[MVP]` Templates fixos: locação padrão e venda padrão.
- RF5.3 `[MVP]` Gerar Magic Link com expiração e revogação, sem exigir login.
- RF5.4 `[MVP]` Upload mobile-first por foto, com status por item (pendente/enviado/aprovado/rejeitado).
- RF5.5 `[MVP]` Corretor aprova ou rejeita com motivo textual visível ao cliente.
- RF5.6 Exportação consolidada (ZIP estruturado ou PDF mergido).
- RF5.7 Templates customizáveis pelo tenant; OCR e leitura automática.

**RF6 — Cobrança documental automática (dunning)** ([E6](backlog/e6-dunning.md))
- RF6.1 `[MVP]` Rotina diária que identifica dossiês com pendência.
- RF6.2 `[MVP]` Lembrete automático a cada 48h com o link e a lista do que falta.
- RF6.3 `[MVP]` Teto de disparos e parada automática ao completar o dossiê.
- RF6.4 Alerta ao corretor após 5 dias sem movimentação.
- RF6.5 Cadência e texto configuráveis por tenant; canal e-mail além do WhatsApp.

**RF7 — Radar do Proprietário** ([E7](backlog/e7-radar-proprietario.md))
- RF7.1 `[MVP]` Registrar feedback de visita (visitante + comentário) em < 30s.
- RF7.2 `[MVP]` Gerar relatório em texto formatado, pronto para envio ao proprietário.
- RF7.3 Captura por áudio com transcrição.
- RF7.4 Disparo quinzenal automático ao WhatsApp do proprietário.
- RF7.5 Agregação de métricas de visualização dos portais.

**RF8 — Painel e analytics** ([E8](backlog/e8-analytics.md))
- RF8.1 `[MVP]` Lista operacional de leads e dossiês com status e pendência.
- RF8.2 Funil kanban por etapa.
- RF8.3 Dashboard de conversão por corretor.

### 3.2 Requisitos não funcionais

**Desempenho**
- RNF1 Primeira resposta ao lead: p95 < 60s ponta a ponta.
- RNF2 Webhook de entrada responde em < 500ms — processa em background, nunca inline.
- RNF3 Página do Magic Link interativa em < 3s em 4G num celular mediano.
- RNF4 Upload de foto de até 10MB com feedback de progresso e retentativa.

**Confiabilidade**
- RNF5 Toda integração externa (WhatsApp, Google, storage) é acessada por job
  idempotente com retentativa e backoff. Nenhuma perda silenciosa de lead.
- RNF6 Webhook de entrada é idempotente por id de mensagem — reentrega do
  provedor não pode duplicar conversa nem disparar resposta em dobro.
- RNF7 Falha de provedor externo degrada, não derruba: o lead entra na fila e
  é atendido quando o canal voltar, com alerta ao corretor.

**Segurança e privacidade**
- RNF8 Isolamento entre tenants é garantido na camada de dados, não só na
  aplicação — ver [multi-tenancy](fundacao/multi-tenancy.md).
- RNF9 Magic Link: token de alta entropia, expiração, revogação, uso auditado,
  sem enumeração. Ver [autenticacao](fundacao/autenticacao.md).
- RNF10 Documento pessoal fica em storage privado; acesso só por URL assinada
  de curta duração. Nunca em bucket público.
- RNF11 Zero segredo no repositório. Toda credencial vem de variável de
  ambiente documentada em `.env.example`.
- RNF12 Log em JSON estruturado, **sem** token, credencial ou dado pessoal.
- RNF13 LGPD: base legal registrada, consentimento no primeiro contato do
  WhatsApp, retenção definida por tipo de dado e rotina de exclusão a pedido.
- RNF14 Trilha de auditoria para acesso e download de documento pessoal.

**Usabilidade**
- RNF15 Toda interface de cliente final é mobile-first e funciona sem app.
- RNF16 O cliente sempre enxerga o que falta e por que um documento foi rejeitado.
- RNF17 Toda mensagem automática permite falar com humano.
- RNF18 Português do Brasil, tom cordial, sem jargão técnico.

**Manutenibilidade**
- RNF19 O código segue as regras do [CLAUDE.md](../CLAUDE.md) — não negociáveis.
- RNF20 SDK de terceiro fica atrás de interface fina deste projeto; o domínio
  não conhece Meta, Google nem gateway.
- RNF21 Suíte de testes roda em comando único, sem rede e sem relógio real.

### 3.3 Fora do escopo

**Fora do escopo do produto (não faremos, nem depois):**
- Portal público de busca de imóveis para o consumidor final.
- Assinatura eletrônica de contrato e emissão de boleto de aluguel — há
  incumbentes fortes; integramos se preciso, não construímos.
- Gestão financeira, repasse ao proprietário e contabilidade.
- Análise de crédito e consulta a bureau — entregamos o dossiê a quem analisa.
- Aplicativo nativo iOS/Android.
- Marketplace de imóveis entre imobiliárias.

**Fora do escopo do MVP (adiado, não descartado):** RF1.4, RF1.5, RF2.4–2.6,
RF3.6–3.7, RF4.5–4.6, RF5.6–5.7, RF6.4–6.5, RF7.3–7.5, RF8.2–8.3.
Justificativa e sequência em [roadmap.md](roadmap.md).

---

## 4. Premissas, restrições e dependências

### 4.1 Premissas

| # | Premissa | Como validamos | Risco se falsa |
| --- | --- | --- | --- |
| P1 | O lead responde a um bot no WhatsApp se ele for direto e útil | Taxa de conclusão do fluxo de triagem nos primeiros 10 clientes | Alto — invalida o épico de triagem |
| P2 | O cliente prefere subir foto num link a mandar no WhatsApp | Taxa de uso do Magic Link vs. envio manual | Alto — invalida o Dossiê |
| P3 | O corretor confia horários da própria agenda a um bot | Adoção do agendamento autônomo | Médio — vira sugestão em vez de autônomo |
| P4 | Corretor autônomo paga assinatura mensal por automação | 5–10 pagantes ao fim do MVP | Alto — muda o público-alvo para imobiliária |
| P5 | O checklist documental é padronizável em 2 templates | Frequência de pedido de item fora do template | Baixo — antecipa RF5.7 |
| P6 | Usuário e cliente final têm smartphone com internet | Observação de campo | Baixo |

### 4.2 Restrições

- **R1 — Regras da Meta/WhatsApp.** Fora da janela de 24h só se envia
  *template* aprovado previamente. Isso condiciona toda a cadência de dunning
  e de lembretes: não é escolha de produto, é limite da plataforma.
- **R2 — Qualidade do número.** Bloqueio por usuários derruba o rating e pode
  suspender o envio. Cadência agressiva é risco existencial para o canal.
- **R3 — Cotas do Google Calendar API** e necessidade de verificação do app
  OAuth para escopos sensíveis, com prazo próprio de aprovação.
- **R4 — LGPD.** Tratamos documento de identidade e comprovante de renda: dado
  pessoal, parte dele sensível. Retenção e exclusão são requisito, não backlog.
- **R5 — Repositório público.** Nenhum segredo, nenhum dado real, em nenhuma
  hipótese — inclusive em teste e fixture.
- **R6 — Equipe pequena.** Otimize por simplicidade operacional; evite
  arquitetura distribuída que ninguém tem plantão para sustentar.
- **R7 — Custo por mensagem.** Conversa no WhatsApp tem custo unitário; o
  volume de disparo automático entra no cálculo de margem do plano.

### 4.3 Dependências

| # | Dependência | Bloqueia | Estado | Ação |
| --- | --- | --- | --- | --- |
| D1 | Conta WhatsApp Business API + número verificado | RF3, RF6 | Não iniciada | Iniciar já: verificação é lenta |
| D2 | Templates de mensagem aprovados pela Meta | RF6.2, RF4.4 | Não iniciada | Submeter junto com D1 |
| D3 | Projeto Google Cloud + OAuth verificado | RF4 | Não iniciada | Iniciar antes do épico de agenda |
| D4 | **Escolha de stack** | Todo o desenvolvimento | **Em aberto** | [ADR-0002](adrs/0002-escolha-de-stack.md) |
| D5 | Storage de objetos privado | RF5 | Depende de D4 | Decidir junto com a stack |
| D6 | Gateway de pagamento | RF1.4 | Adiado | Cobrança manual no MVP |
| D7 | 3 a 5 corretores dispostos a testar | Validação de P1–P4 | Não iniciada | Recrutar durante a construção |

---

## 5. Histórico e decisões

Decisões arquiteturais vivem em [adrs/](adrs/README.md) — este é o registro de
decisões de **produto**.

| Data | Decisão | Motivo |
| --- | --- | --- |
| 2026-07-20 | Bases de engenharia canônicas registradas em `CLAUDE.md` | Regra escrita antes do código evita negociar qualidade sob pressão de prazo |
| 2026-07-20 | Público inicial: corretor autônomo, não imobiliária | Ciclo de venda curto, decisor único, valida a tese mais rápido |
| 2026-07-20 | Ordem de construção: Dossiê+Dunning → Triagem+Agenda → Radar | Módulo documental é isolado, tem menos dependência externa e vende sozinho |
| 2026-07-20 | MVP usa árvore de decisão, não LLM, na triagem | Previsível, testável, barato; a incerteza do LLM não deve compor com a incerteza do produto |
| 2026-07-20 | Cobrança do SaaS manual (Pix/link) no MVP | Gateway não valida hipótese nenhuma com 10 clientes |
| 2026-07-20 | Stack em aberto, ADR-0002 como *proposto* | Decisão consciente e datada, não omissão |

### Questões em aberto

| # | Questão | Precisa ser resolvida antes de |
| --- | --- | --- |
| Q1 | Qual stack? | Qualquer linha de código de produção |
| Q2 | Provedor de WhatsApp: Cloud API direta ou intermediário? | Épico 3 |
| Q3 | Preço e empacotamento dos planos | Primeira venda |
| Q4 | Um número de WhatsApp por tenant ou número compartilhado? | Épico 3 — muda o modelo de dados e o custo |
| Q5 | Prazo de retenção de documento após fim do processo | Épico 5 |

### Pesquisa e artefatos

Ainda não há protótipo, pesquisa de usuário nem teste de usabilidade. Quando
houver, linke aqui — este documento é o índice do que sustenta as decisões.
