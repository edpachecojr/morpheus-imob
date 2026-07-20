# Glossário — linguagem ubíqua

Os termos abaixo são os nomes usados **no código, no banco, nos logs e na
conversa com o cliente**. Se um conceito aparece com dois nomes diferentes em
dois módulos, um dos dois está errado.

> Regra: nome de entidade em **português**, alinhado ao vocabulário do
> corretor. Palavra técnica em inglês só onde é jargão de infra (`webhook`,
> `job`, `token`). Ver [ADR-0004](../adrs/0004-linguagem-ubiqua-em-portugues.md).

## Conta e pessoas

| Termo | Definição | Não confundir com |
| --- | --- | --- |
| **Tenant** | Unidade de isolamento de dados: uma imobiliária ou um corretor autônomo. Toda linha do banco pertence a exatamente um tenant. | Usuário — um tenant tem vários |
| **Conta** | O registro comercial do tenant: plano, situação de pagamento, dados de cobrança. | Tenant (isolamento) vs. Conta (comercial) |
| **Usuário** | Pessoa que autentica no painel. Pertence a um tenant e tem um papel. | Lead, Cliente e Proprietário **não** são usuários — nunca autenticam no painel |
| **Papel** | Conjunto de permissões do usuário: `dono`, `gestor`, `corretor`, `secretaria`. | Cargo na vida real |
| **Corretor** | Usuário que atende leads e faz visitas. Tem agenda e janela de atendimento. | Usuário genérico |

## Imóvel e mercado

| Termo | Definição |
| --- | --- |
| **Imóvel** | Unidade anunciada pelo tenant. Tem código de referência único **dentro do tenant**, endereço, finalidade e situação. |
| **Finalidade** | `locacao` ou `venda`. Determina o template documental padrão. |
| **Situação do imóvel** | `disponivel`, `reservado`, `fechado`, `inativo`. |
| **Proprietário** | Dono do imóvel, destinatário do Radar. Contato, não usuário. |
| **Captação** | Ato de trazer o imóvel para a carteira do tenant. |
| **Carteira** | Conjunto de imóveis ativos do tenant. Retê-la é o objetivo do Radar. |

## Funil de leads

| Termo | Definição |
| --- | --- |
| **Lead** | Pessoa que demonstrou interesse em um imóvel. Nasce de uma origem e vira uma conversa. |
| **Origem** | De onde o lead veio: `link_direto`, `webhook_portal`, `manual`. Rastreada sempre — sem origem não há atribuição. |
| **Conversa** | Thread de mensagens entre o sistema/corretor e um contato no WhatsApp. Uma por número por tenant. |
| **Triagem** | Processo automático de perguntar e classificar o lead antes de envolver o corretor. |
| **Qualificação** | As respostas coletadas na triagem: urgência, forma de pagamento, faixa de renda. |
| **Temperatura** | Classificação derivada da qualificação: `quente`, `morno`, `frio`. É **calculada por regra**, nunca digitada. |
| **Transbordo** | Transferência do controle da conversa do bot para o corretor humano. A partir daí o bot cala. |
| **Janela de 24h** | Período após a última mensagem do contato em que o WhatsApp permite mensagem livre. Fora dela, só template aprovado. Restrição da Meta, não escolha nossa. |

## Visita

| Termo | Definição |
| --- | --- |
| **Visita** | Compromisso entre corretor e lead num imóvel, em data e hora. Espelhada como evento no Google Calendar. |
| **Situação da visita** | `agendada`, `confirmada`, `realizada`, `cancelada`, `nao_compareceu`. |
| **Janela de atendimento** | Faixa de horário e dias em que o corretor aceita visita. Restringe o cálculo de horários livres. |
| **Horário livre** | Slot que passou por: dentro da janela, sem conflito na agenda, com antecedência mínima. |
| **Feedback de visita** | Impressão registrada pelo corretor após a visita. Insumo do Radar. |

## Dossiê documental

| Termo | Definição |
| --- | --- |
| **Dossiê** | Processo de coleta documental de um negócio. Tem um imóvel, um ou mais participantes e um checklist. |
| **Participante** | Pessoa que precisa enviar documento: locatário, comprador, fiador, cônjuge. Cada um tem seu próprio Magic Link. |
| **Template de checklist** | Modelo que define quais itens são exigidos por tipo de negócio. Ex.: "Locação Padrão". |
| **Item do checklist** | Documento exigido de um participante. Ex.: "comprovante de renda". |
| **Situação do item** | `pendente`, `enviado`, `aprovado`, `rejeitado`. |
| **Motivo da rejeição** | Texto obrigatório na rejeição, **mostrado ao participante**. Sem ele o cliente reenvia o mesmo erro. |
| **Anexo** | Arquivo enviado para um item. Um item pode ter vários (frente e verso, três meses de extrato). |
| **Magic Link** | URL com token de acesso que permite ao participante ver e completar **apenas o seu** checklist, sem login. Expira e é revogável. |
| **Situação do dossiê** | `aberto`, `em_analise`, `completo`, `cancelado`. |

## Cobrança documental

| Termo | Definição |
| --- | --- |
| **Dunning** | Cobrança automática e educada de pendência documental. Termo mantido em inglês por ser jargão consolidado. |
| **Cadência** | Regra de quando cobrar: intervalo entre disparos e teto de tentativas. |
| **Disparo** | Uma tentativa de cobrança registrada, com canal, resultado e horário. |
| **Estagnação** | Dossiê sem movimentação por N dias. Gera alerta ao corretor, não ao cliente. |

## Radar do Proprietário

| Termo | Definição |
| --- | --- |
| **Radar** | Módulo de transparência com o proprietário. |
| **Relatório do proprietário** | Consolidado periódico de visitas, feedbacks e recomendação para o imóvel. |
| **Recorrência de feedback** | Padrão detectado entre feedbacks (ex.: "preço alto" em 4 de 5 visitas). É o que sustenta a conversa difícil sobre preço. |

## Infraestrutura de domínio

| Termo | Definição |
| --- | --- |
| **Canal** | Meio de comunicação com o exterior: `whatsapp`, `email`. |
| **Mensagem** | Unidade enviada ou recebida num canal, sempre ligada a uma conversa. |
| **Template de mensagem** | Texto pré-aprovado pela Meta, com variáveis, para uso fora da janela de 24h. |
| **Job** | Trabalho assíncrono em fila: disparo, sincronização, geração de relatório. Idempotente por definição. |
| **Evento de domínio** | Fato ocorrido no passado, no pretérito: `LeadQualificado`, `DossieCompletado`. Dispara efeitos sem acoplar módulos. |

## Termos proibidos

Não use estes nomes em código — são vagos e retornam centenas de ocorrências
num grep, violando a regra de nomes do [CLAUDE.md](../../CLAUDE.md):

`data`, `info`, `handler`, `manager`, `service` genérico, `util`, `helper`,
`process`, `item` sem qualificador, `status` sem qualificador, `client` (usar
`Lead`, `Participante` ou `Proprietário` — "cliente" no imobiliário é ambíguo
entre quem compra e quem anuncia).
