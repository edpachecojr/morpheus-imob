# ADR-0006 — Triagem por árvore de decisão, não LLM, no MVP

**Status:** Aceito
**Data:** 2026-07-20
**Requisito:** RF3 do [PRD](../prd.md) · [E3](../backlog/e3-triagem-whatsapp.md)

## Contexto

O motor de triagem faz três perguntas ao lead (urgência, forma de pagamento,
faixa de renda) e classifica a temperatura. A tentação óbvia é usar um LLM para
conduzir a conversa livremente.

Mas o MVP existe para validar uma hipótese incerta (P1: *o lead responde a um
bot no WhatsApp*). Somar a essa incerteza a de um modelo generativo torna o
resultado ilegível: se a taxa de conclusão vier baixa, não saberemos se o
problema é a premissa ou o prompt.

Há também riscos concretos de canal: um bot que alucina preço, condição de
pagamento ou disponibilidade de imóvel gera dano comercial e jurídico real, e
mensagem estranha em massa derruba o rating do número na Meta (R2 do PRD).

## Decisão

No MVP a triagem é uma **árvore de decisão determinística**, conduzida por
botões de resposta rápida do WhatsApp e palavras-chave, com classificação de
temperatura por **regra explícita e auditável**.

A interface do domínio (`MotorDeTriagem`) é desenhada para que a implementação
possa ser substituída por uma baseada em LLM sem tocar em nada além dela —
conforme [ADR-0007](0007-integracoes-atras-de-porta.md).

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| LLM conduzindo a conversa livre | Não determinístico, difícil de testar, caro por conversa, risco de alucinar condição comercial, e polui a validação da premissa central |
| Híbrido (árvore + LLM só para interpretar texto livre) | Melhor caminho, mas é otimização de um problema que ainda não medimos. Fica para depois do primeiro sinal de que a árvore trava |
| Formulário web em vez de conversa | Contraria a tese do produto: o lead está no WhatsApp e formulário é fricção |

## Consequências

**Positivas**
- Testável sem rede e sem relógio: entrada conhecida, saída conhecida.
- Custo por conversa próximo de zero.
- Explicável ao corretor: "foi classificado como frio porque respondeu X".
- Barato de construir — semanas de diferença no cronograma.

**Negativas**
- Rígido. Lead que responde fora do script ("depende, tenho FGTS mas...") cai
  no caminho de exceção e vai para transbordo humano. Aceitável: transbordo é
  o comportamento seguro e já é requisito (RF3.5).
- A conversa soa mecânica. Mitigado com texto bem escrito e caminho explícito
  para falar com humano (RNF17).
- Não capta nuance que qualificaria melhor.

**Como saberemos que erramos:** se a taxa de conclusão do fluxo ficar baixa
**e** os logs mostrarem muitos leads caindo no caminho de exceção por
responderem em texto livre, o gargalo é a rigidez — aí o híbrido se justifica
com evidência, não por entusiasmo.
