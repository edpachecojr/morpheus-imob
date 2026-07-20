# morpheus-imob

Suíte operacional automatizada para o mercado imobiliário. Um ecossistema de
fechamento de negócios 24/7 para pequenas imobiliárias e corretores autônomos,
que substitui o CRM estático por um fluxo de trabalho conversacional,
inteligente e assíncrono.

## O problema

Corretor perde negócio por três motivos que não têm nada a ver com o imóvel:
demora para responder o lead, burocracia documental que se arrasta por semanas e
proprietário que não sabe o que está acontecendo com o próprio imóvel.

## Os três módulos

### 1. Triagem e agendamento via WhatsApp

Qualificação instantânea do lead na própria conversa e agendamento autônomo de
visitas, com integração bidirecional ao Google Calendar. O tempo de primeira
resposta vai a zero.

### 2. Dossiê Digital

Esteira documental mobile-first entregue por Magic Links, com motor de cobrança
automática. Certidões e comprovantes são juntados sem intervenção humana.

### 3. Radar do Proprietário

Centraliza os feedbacks de cada visita e gera relatórios quinzenais de
transparência para o proprietário — fidelizando a carteira de imóveis.

## Status

Em definição. A stack técnica e a convenção de estrutura de diretórios ainda
serão fechadas.

## Desenvolvimento

As diretrizes de engenharia deste projeto são canônicas e inegociáveis. Leia
**[CLAUDE.md](./CLAUDE.md)** antes da primeira linha de código.

### Segredos

Repositório público. Nenhuma chave, token ou credencial entra no código. Toda
configuração sensível vem de variável de ambiente, documentada em `.env.example`
apenas com valores de exemplo.
