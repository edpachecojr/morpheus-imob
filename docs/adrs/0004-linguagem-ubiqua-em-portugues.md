# ADR-0004 — Linguagem ubíqua do domínio em português

**Status:** Aceito
**Data:** 2026-07-20
**Detalhe:** [glossario.md](../dominio/glossario.md)

## Contexto

O domínio é o mercado imobiliário brasileiro. Os conceitos centrais —
*captação*, *fiador*, *certidão negativa*, *repasse*, *vistoria*, *dossiê* —
têm significado jurídico e operacional preciso em português e **não têm
tradução fiel** para inglês. `Guarantor` não é fiador; `listing` não é captação.

Traduzir cria um dicionário mental permanente entre a conversa com o cliente e
o código, e é nesse dicionário que os mal-entendidos se instalam: alguém traduz
"cliente" como `Customer` sem perceber que no mercado imobiliário isso é
ambíguo entre quem compra e quem anuncia o imóvel.

## Decisão

Entidades, campos, eventos e estados do domínio são nomeados em **português**,
com o mesmo termo usado pelo corretor. Termos técnicos de infraestrutura sem
domínio de negócio (`webhook`, `job`, `token`, `cache`, `queue`) permanecem em
inglês, por serem jargão consolidado e sem correspondente natural.

Palavras-chave da linguagem, bibliotecas e mensagens de commit seguem sua
própria convenção — assunto de commit em português BR, conforme o CLAUDE.md.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| Tudo em inglês | Exige traduzir conceito jurídico brasileiro; perde precisão e cria ambiguidade em termos como "cliente" |
| Misturar sem regra | O pior dos mundos: `Lead.proprietarioId` ao lado de `Dossie.ownerId`; grep deixa de funcionar |
| Português inclusive na infraestrutura | Fricção desnecessária com bibliotecas e documentação de terceiros |

## Consequências

**Positivas**
- O código lê como a conversa com o corretor. A revisão de regra de negócio
  deixa de exigir tradução.
- Termos ambíguos ficam expostos e resolvidos no [glossário](../dominio/glossario.md)
  em vez de escondidos numa tradução aproximada.

**Negativas**
- Mistura visível na fronteira: `JobDeDisparo`, `WebhookDeMensagem`. Aceito —
  a fronteira é real e o nome expõe isso honestamente.
- Acentuação e cedilha não entram em identificador de código: use `dossie`,
  `codigo_referencia`, `nao_compareceu`. Texto voltado ao usuário mantém a
  ortografia correta.
- Contribuidor que não fala português tem barreira de entrada. Dado o mercado
  atendido, é custo aceitável.

**Como saberemos que erramos:** se aparecerem dois nomes para o mesmo conceito
em módulos diferentes, o glossário não está sendo respeitado — o problema é de
processo, não da decisão.
