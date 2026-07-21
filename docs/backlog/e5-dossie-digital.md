# E5 — Dossiê Digital e esteira documental

> **Objetivo:** o cliente envia tudo sozinho, pelo celular, sem login e sem
> alguém cobrando à mão.
> **Requisitos:** RF5 · **Fase:** 1 — **primeiro módulo a ser construído**
> **Segurança:** [autenticacao.md#magic-link](../fundacao/autenticacao.md#magic-link)

Este é o módulo mais sensível do sistema: uma URL sem senha dá acesso a
documento de identidade e comprovante de renda. As regras de Magic Link não são
recomendação — são condição para o módulo existir.

---

## E5-F1 — Templates de checklist

### E5-F1-H1 `[MVP]` · 3 pts — Templates de sistema: locação e venda padrão
**Como** corretor, **quero** um checklist pronto, **para** não montar lista do
zero em cada negócio.

- **Dado** um dossiê novo, **quando** eu escolher "Locação Padrão", **então**
  os itens exigidos são RG/CNH, CPF, comprovante de renda e comprovante de
  residência.
- **E** "Venda Padrão" exige documentos pessoais e certidões negativas básicas.
- **E** cada item tem descrição em linguagem de leigo e um exemplo do que serve.
- **E** o template define quais itens se aplicam a qual tipo de participante.
- **E** alterar um template **não** altera dossiês já abertos — eles guardam
  uma cópia dos itens no momento da abertura.

### E5-F1-H2 · 5 pts — Templates customizáveis pelo tenant
Pós-MVP (Fase 7). RF5.7.

---

## E5-F2 — Dossiê e participantes

### E5-F2-H1 `[MVP]` · 3 pts — Abrir dossiê
**Como** corretor, **quero** abrir a esteira documental de um negócio, **para**
começar a coleta.

- **Dado** um imóvel e um template, **quando** eu abrir o dossiê, **então** ele
  nasce `aberto` com os itens copiados do template.
- **E** o dossiê registra quem abriu e quando.
- **E** posso abrir a partir de um lead existente, herdando nome e telefone.

### E5-F2-H2 `[MVP]` · 3 pts — Adicionar participantes
**Como** corretor, **quero** incluir locatário, fiador e cônjuge, **para** cobrar
os documentos de cada um separadamente.

- **Dado** um dossiê, **quando** eu adicionar um participante com nome, papel e
  WhatsApp, **então** ele recebe **seu próprio** checklist conforme o template.
- **E** um participante nunca enxerga item ou anexo de outro participante.
- **E** posso remover participante enquanto o dossiê não estiver `completo`,
  ficando o registro da remoção.

### E5-F2-H3 `[MVP]` · 3 pts — Situação derivada do dossiê
**Como** corretor, **quero** saber num relance se falta algo, **para** não
conferir item por item.

- **Dado** os itens de todos os participantes, **quando** eu consultar o
  dossiê, **então** sua situação é **derivada**: `aberto` se há pendente ou
  rejeitado, `em_analise` se tudo enviado e algo aguardando revisão, `completo`
  só quando todos aprovados.
- **E** não existe forma de marcar o dossiê como completo à mão.
- **E** rejeitar um item de um dossiê `em_analise` o devolve a `aberto`.
- **E** `DossieCompletado` é publicado na transição para `completo`.

### E5-F2-H4 `[MVP]` · 2 pts — Cancelar dossiê
- **Dado** um dossiê ativo, **quando** eu cancelá-lo com motivo, **então** todos
  os Magic Links são revogados imediatamente e o dunning para.

---

## E5-F3 — Magic Link

### E5-F3-H1 `[MVP]` · 5 pts — Emitir link seguro por participante
**Como** corretor, **quero** mandar um link ao cliente, **para** ele enviar os
documentos sem criar conta.

- **Dado** um participante, **quando** eu emitir o link, **então** o token tem
  ≥ 128 bits de entropia, gerado por fonte criptográfica.
- **E** apenas o **hash** do token é armazenado.
- **E** o escopo é exatamente um participante de um dossiê.
- **E** expira em 7 dias e pode ser reemitido.
- **E** token inválido, expirado, revogado ou de outro tenant recebem a **mesma**
  resposta genérica de "link expirado".
- **E** a página envia `noindex` e não vaza o token em `Referer`.
- **E** há rate limit por token e por IP.

### E5-F3-H2 `[MVP]` · 2 pts — Revogar link
**Como** corretor, **quero** invalidar um link, **para** conter envio ao número
errado.

- **Dado** um link ativo, **quando** eu revogá-lo, **então** o acesso seguinte
  já é negado.
- **E** completar ou cancelar o dossiê revoga automaticamente.

### E5-F3-H3 `[MVP]` · 2 pts — Auditoria de acesso
- **Dado** qualquer acesso ou download pelo link, **quando** ocorrer, **então**
  ficam registrados horário, IP e ação (RNF14).
- **E** o registro **não** contém o token nem conteúdo do documento.

---

## E5-F4 — Envio pelo participante (mobile-first)

### E5-F4-H1 `[MVP]` · 5 pts — Ver o que falta
**Como** locatário, **quero** entender de imediato o que preciso mandar, **para**
resolver de uma vez.

- **Dado** meu link válido, **quando** eu abrir no celular, **então** vejo a
  lista de itens com situação e explicação em linguagem simples.
- **E** a página fica interativa em menos de 3s em 4G num aparelho mediano (RNF3).
- **E** funciona sem instalar app.
- **E** vejo quem é o corretor e como falar com ele.

### E5-F4-H2 `[MVP]` · 5 pts — Enviar documento por foto
**Como** locatário, **quero** fotografar e enviar, **para** não precisar de
scanner nem computador.

- **Dado** um item pendente, **quando** eu enviar arquivo ou foto, **então** ele
  é anexado e o item passa a `enviado`.
- **E** aceita imagem e PDF, até 10MB, com feedback de progresso e retentativa
  em falha de rede (RNF4).
- **E** posso anexar vários arquivos ao mesmo item (frente e verso, 3 meses de
  extrato).
- **E** tipo não suportado é recusado com mensagem que diz o que é aceito.
- **E** o arquivo vai para storage privado, no caminho particionado por tenant,
  e nunca fica publicamente acessível (RNF10).
- **E** o anexo é imutável: reenvio cria anexo novo e arquiva o anterior.

### E5-F4-H3 `[MVP]` · 3 pts — Ver rejeição e reenviar
**Como** locatário, **quero** saber por que meu documento foi recusado, **para**
não errar de novo.

- **Dado** um item rejeitado, **quando** eu abrir o link, **então** vejo o
  motivo escrito pelo corretor, em destaque.
- **E** reenviar devolve o item a `enviado`.

### E5-F4-H4 `[MVP]` · 2 pts — Conclusão visível
- **Dado** que enviei tudo, **quando** a página atualizar, **então** vejo
  confirmação clara de que não falta nada da minha parte.

---

## E5-F5 — Revisão pelo corretor

### E5-F5-H1 `[MVP]` · 3 pts — Painel de revisão
**Como** corretor, **quero** ver o que chegou e o que falta, **para** trabalhar
por fila em vez de caçar no WhatsApp.

- **Dado** meus dossiês, **quando** eu abrir o painel, **então** vejo por dossiê
  o total de itens, pendentes, aguardando revisão e rejeitados.
- **E** consigo filtrar por "aguardando minha revisão".

### E5-F5-H2 `[MVP]` · 3 pts — Aprovar ou rejeitar com motivo
**Como** corretor, **quero** rejeitar explicando, **para** o cliente acertar na
segunda tentativa.

- **Dado** um item `enviado`, **quando** eu aprovar, **então** ele vai a
  `aprovado` e não aceita mais alteração.
- **E** ao rejeitar, o motivo é **obrigatório e não vazio**, e fica visível ao
  participante.
- **E** rejeição publica `ItemRejeitado`, que reinicia a cadência do dunning (E6).
- **E** toda decisão registra autor e horário.

### E5-F5-H3 `[MVP]` · 3 pts — Visualizar e baixar anexo com segurança
- **Dado** um anexo, **quando** eu abri-lo, **então** recebo URL assinada válida
  por no máximo 5 minutos.
- **E** URL expirada não baixa.
- **E** o download é auditado.
- **E** usuário de outro tenant recebe 403.

### E5-F5-H4 · 5 pts — Exportar dossiê consolidado
Pós-MVP (Fase 7). RF5.6 — ZIP estruturado ou PDF mergido.

### E5-F5-H5 · 8 pts — OCR e leitura automática de documento
Pós-MVP (Fase 7). RF5.7. Quebrar antes de executar.

---

## E5-F6 — Retenção e exclusão de documento pessoal

> **Estórias faltantes identificadas na revisão de 2026-07-21.** O PRD é
> explícito: "Retenção e exclusão são requisito, não backlog" (R4) — ou seja,
> ao contrário do resto deste épico, isto **não pode** ser empurrado para
> pós-MVP só porque o corretor já consegue fechar um dossiê sem isso. RG, CPF
> e comprovante de renda são o dado mais sensível de todo o sistema (Q5).

### E5-F6-H1 `[MVP]` · 3 pts — Prazo de retenção por tipo de documento e expurgo automático
**Como** titular do documento, **quero** que ele não fique guardado para
sempre depois que o negócio terminou ou morreu, **para** que meu dado pessoal
não vire passivo de vazamento sem motivo de negócio.

- **Dado** um dossiê `completo` ou `cancelado` há mais que o prazo de retenção
  configurado, **quando** a rotina de expurgo rodar, **então** o conteúdo do
  anexo é apagado do storage, mantendo só o metadado auditável (quem, quando,
  que tipo de documento existiu).
- **E** o prazo é configurável por tipo de documento, não um valor único fixo
  no código.
- **E** apagar o conteúdo **não** apaga a trilha de auditoria de acesso
  (E5-F3-H3) — são dados de naturezas diferentes.
- **E** o teste usa relógio falso; a rotina não toca rede além do storage.

### E5-F6-H2 `[MVP]` · 2 pts — Exclusão antecipada a pedido do titular
**Como** titular do documento, **quero** pedir a exclusão antes do prazo
padrão, **para** exercer meu direito sem esperar o expurgo automático.

- **Dado** um pedido de exclusão identificando o participante, **quando** o
  corretor confirmar o pedido, **então** todos os anexos daquele participante
  são apagados do storage imediatamente.
- **E** dossiê com pendência de aprovação em andamento é encerrado como
  `cancelado` — não fica em limbo sem documento e sem situação.
- **E** o pedido e sua execução ficam registrados (quem pediu, quando, quem
  confirmou), sem guardar o conteúdo do documento excluído.
