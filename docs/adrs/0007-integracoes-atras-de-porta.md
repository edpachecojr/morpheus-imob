# ADR-0007 — Integrações externas atrás de porta do domínio

**Status:** Aceito
**Data:** 2026-07-20
**Requisito:** RNF20 do [PRD](../prd.md) · regra de dependências do [CLAUDE.md](../../CLAUDE.md)

## Contexto

O produto depende de terceiros que **não controlamos e que mudam sozinhos**:
WhatsApp Cloud API (regras de janela de 24h e de template mudam por decisão da
Meta), Google Calendar, storage de objetos, e eventualmente gateway de
pagamento e provedor de e-mail.

Duas consequências práticas. Primeira: já há uma questão em aberto (Q2 do PRD)
sobre usar a Cloud API direta ou um intermediário — trocar depois precisa ser
barato. Segunda: se o SDK do provedor aparecer espalhado pelo código de
negócio, **nenhum teste roda sem rede**, o que contraria diretamente o F.I.R.S.T.
exigido pelo CLAUDE.md.

## Decisão

Todo acesso a serviço externo passa por uma **porta**: interface fina, tipada,
definida pelo domínio em termos do domínio, com o adaptador do provedor como
detalhe substituível. A dependência é sempre injetada, nunca importada no ponto
de uso.

O domínio conhece `EnviadorDeMensagem`, não `MetaWhatsAppClient`. Conhece
`AgendaDoCorretor`, não `google.calendar.v3`. Conhece `ArmazenamentoDeAnexo`,
não o SDK do S3.

Portas previstas:

| Porta | Esconde | Vocabulário do domínio |
| --- | --- | --- |
| `EnviadorDeMensagem` | WhatsApp Cloud API / intermediário | enviar texto, enviar template, enviar botões |
| `RecebedorDeMensagem` | Webhook e formato do provedor | traduz payload externo em `Mensagem` do domínio |
| `AgendaDoCorretor` | Google Calendar | listar ocupações, criar visita, cancelar visita |
| `ArmazenamentoDeAnexo` | S3/R2/MinIO | guardar anexo, gerar link temporário, arquivar |
| `Relogio` | Data e hora do sistema | agora() — sem isto, dunning e expiração não são testáveis |
| `GeradorDeToken` | Fonte de aleatoriedade | token de magic link determinístico em teste |

`Relogio` e `GeradorDeToken` não são serviços externos, mas são fontes de
não-determinismo — pelo mesmo motivo, ficam atrás de porta.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| Usar o SDK direto no código de negócio | Teste passa a exigir rede; troca de provedor vira reescrita; regra da Meta vaza para o domínio |
| Envolver com cópia 1:1 da API do SDK | Wrapper que só repassa não isola nada — o domínio continua pensando como o provedor |
| Camada de anticorrupção completa por integração | Peso desproporcional a esta escala; a porta fina entrega o essencial |

## Consequências

**Positivas**
- Suíte de testes roda sem rede, com fakes nomeados (não stubs inline), como o
  CLAUDE.md exige.
- Trocar provedor de WhatsApp toca um arquivo — o adaptador. Isso mantém a Q2
  em aberto sem custo.
- As regras da Meta (janela de 24h, template) ficam confinadas ao adaptador,
  em vez de espalhadas por regra de negócio.
- Ambiente local e de teste usam adaptador que registra o que teria enviado —
  requisito de [ambientes-e-segredos](../fundacao/ambientes-e-segredos.md).

**Negativas**
- Camada extra de indireção; para integração trivial parece cerimônia.
- Risco de a porta vazar o modelo do provedor por preguiça de traduzir. Isso é
  ponto obrigatório de revisão em PR.
- Recurso específico e valioso de um provedor pode não caber na abstração
  comum. Quando acontecer, prefira estender a porta em termos do domínio a
  furá-la.

**Como saberemos que erramos:** se toda mudança de funcionalidade exigir tocar
porta **e** adaptador **e** domínio, a fronteira está no lugar errado — a porta
está espelhando o provedor em vez de expressar o domínio.
