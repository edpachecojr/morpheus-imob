# E7 — Radar do Proprietário

> **Objetivo:** o proprietário deixa de ficar no escuro — e por isso não leva o
> imóvel para a concorrência.
> **Requisitos:** RF7 · **Fase:** 4

O gargalo aqui é o **corretor**, não o proprietário. Ele já tem os feedbacks na
cabeça; o que falta é um jeito de registrar em menos tempo do que custa
ignorar. Se registrar levar mais de 30 segundos, o módulo não é usado e nada
mais neste épico importa.

---

## E7-F1 — Captura de feedback

### E7-F1-H1 `[MVP]` · 3 pts — Registrar feedback em menos de 30 segundos
**Como** corretor, **quero** anotar a impressão do visitante logo após a visita,
**para** não esquecer até chegar no escritório.

- **Dado** uma visita `realizada`, **quando** eu abrir o registro de feedback,
  **então** preciso preencher apenas nome do visitante (já preenchido a partir
  do lead) e um comentário livre.
- **E** o formulário é mobile-first e salva em um toque.
- **E** o feedback fica ligado à visita, ao imóvel e ao lead.
- **E** posso editar ou apagar meu próprio feedback.

### E7-F1-H2 `[MVP]` · 2 pts — Ser lembrado de registrar
**Como** corretor, **quero** um empurrão após a visita, **para** o registro
virar hábito.

- **Dado** `VisitaRealizada`, **quando** o evento for publicado, **então** o
  corretor recebe uma notificação com link direto para o registro.
- **E** um único lembrete — insistir aqui incomoda o próprio cliente pagante.

### E7-F1-H3 `[MVP]` · 2 pts — Marcar objeções recorrentes
**Como** corretor, **quero** classificar a objeção principal, **para** o
relatório detectar padrão sem depender de ler texto livre.

- **Dado** um feedback, **quando** eu registrá-lo, **então** posso marcar uma ou
  mais objeções de uma lista fixa: preço alto, precisa de reforma, localização,
  tamanho, condição de pagamento, outro.
- **E** a marcação é opcional — exigir mata a meta dos 30 segundos.

### E7-F1-H4 · 5 pts — Registrar feedback por áudio com transcrição
Pós-MVP (Fase 8). RF7.3 — o caminho mais rápido de todos, e o de maior custo.

---

## E7-F2 — Relatório ao proprietário

### E7-F2-H1 `[MVP]` · 3 pts — Gerar relatório em texto pronto
**Como** corretor, **quero** um texto pronto para enviar ao proprietário,
**para** prestar contas sem escrever nada.

- **Dado** um imóvel com visitas registradas, **quando** eu gerar o relatório do
  período, **então** recebo um texto formatado com: total de visitas, lista de
  visitas com data e feedback resumido, e objeções mais frequentes.
- **E** o texto é copiável em um toque e legível dentro do WhatsApp.
- **E** o tom é profissional e não expõe dado pessoal do visitante além do
  primeiro nome.
- **E** imóvel sem visita no período gera um relatório honesto sobre isso — com
  o número de contatos recebidos — em vez de não gerar nada.

### E7-F2-H2 `[MVP]` · 3 pts — Recomendação baseada em recorrência
**Como** proprietário, **quero** entender o que está travando meu imóvel,
**para** decidir sobre preço ou reforma.

- **Dado** feedbacks com objeções marcadas, **quando** uma objeção aparecer em
  metade ou mais das visitas do período, **então** o relatório a destaca com
  uma sugestão de ação correspondente.
- **E** abaixo de três visitas no período, nenhuma recomendação é feita —
  amostra pequena vira conselho errado, e conselho errado sobre preço custa o
  contrato.

### E7-F2-H3 `[MVP]` · 2 pts — Enviar com um clique
- **Dado** um relatório gerado e um proprietário com WhatsApp cadastrado,
  **quando** eu escolher enviar, **então** a mensagem sai pela porta
  `EnviadorDeMensagem`.
- **E** o envio é registrado, alimentando o KPI
  `cobertura_relatorio_proprietario`.
- **E** fora da janela de 24h, usa template aprovado.
- **E** sem WhatsApp cadastrado, o texto continua disponível para cópia manual.

### E7-F2-H4 · 5 pts — Disparo quinzenal automático
Pós-MVP (Fase 8). RF7.4. Depende de confiança acumulada na qualidade do texto —
relatório automático ruim enviado ao proprietário é pior que nenhum.

---

## E7-F3 — Visão do imóvel

### E7-F3-H1 `[MVP]` · 2 pts — Histórico do imóvel
**Como** corretor, **quero** ver tudo que aconteceu com um imóvel, **para**
conversar com o proprietário com dados na mão.

- **Dado** um imóvel, **quando** eu abrir seu histórico, **então** vejo leads
  recebidos, visitas agendadas, realizadas e não comparecidas, e todos os
  feedbacks em ordem cronológica.

### E7-F3-H2 · 5 pts — Cruzar com métricas de visualização dos portais
Pós-MVP (Fase 8). RF7.5. Depende de acesso a dados de cada portal — pode ser
inviável sem parceria; validar antes de planejar.

---

## E7-F4 — Proprietário *(pós-MVP — Fase 8)*

| ID | Estória | Pts |
| --- | --- | --- |
| E7-F4-H1 | Portal do proprietário por Magic Link, com histórico do imóvel | 5 |
| E7-F4-H2 | Preferência de frequência de relatório por proprietário | 2 |
