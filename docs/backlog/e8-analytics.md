# E8 — Painel e analytics

> **Objetivo:** no MVP, dizer ao corretor **o que fazer agora**. Depois, dizer
> ao dono **como a operação vai**.
> **Requisitos:** RF8 · **Fases:** 1 (operacional), 8 (gestão)

Distinção que organiza o épico: o MVP é do **operador**, não do gestor. Corretor
autônomo não olha dashboard de conversão — ele quer saber o que está travado
agora. Métrica é dor de quem tem equipe, e por isso vem depois.

---

## E8-F1 — Painel operacional `[MVP]`

### E8-F1-H1 `[MVP]` · 3 pts — Tela inicial com o que exige ação
**Como** corretor, **quero** abrir o sistema e ver o que depende de mim,
**para** não precisar procurar.

- **Dado** meu tenant, **quando** eu entrar, **então** vejo, em blocos: itens
  aguardando minha revisão, dossiês parados, leads quentes sem visita agendada
  e visitas de hoje.
- **E** cada bloco leva direto à ação correspondente.
- **E** bloco vazio mostra estado vazio útil, não área em branco.
- **E** a tela carrega em menos de 2s com volume realista de dados.

### E8-F1-H2 `[MVP]` · 3 pts — Lista de dossiês com pendência visível
- **Dado** meus dossiês, **quando** eu abrir a lista, **então** vejo situação,
  participantes, quantos itens faltam e há quanto tempo está parado.
- **E** posso filtrar por situação e ordenar por tempo parado.
- **E** a lista é paginada.

### E8-F1-H3 `[MVP]` · 3 pts — Lista de leads com temperatura e estado
- **Dado** meus leads, **quando** eu abrir a lista, **então** vejo temperatura,
  estado, imóvel de interesse, origem e data do último contato.
- **E** posso filtrar por temperatura e estado.

### E8-F1-H4 `[MVP]` · 2 pts — Agenda do dia
- **Dado** minhas visitas, **quando** eu abrir a agenda, **então** vejo as de
  hoje e amanhã com horário, imóvel, endereço e contato do lead.

### E8-F1-H5 `[MVP]` · 2 pts — Busca global
- **Dado** um termo, **quando** eu buscar, **então** encontro lead, imóvel ou
  dossiê por nome, telefone ou código — sempre restrito ao meu tenant.

---

## E8-F2 — Coleta de métricas `[MVP]`

Não tem interface. Existe para que, ao construir o dashboard na Fase 8, haja
**histórico** para mostrar — instrumentar depois significa começar do zero.

### E8-F2-H1 `[MVP]` · 3 pts — Instrumentar os KPIs do PRD
**Como** time, **quero** registrar os eventos que sustentam os KPIs, **para**
saber se o produto funciona antes de ter dashboard.

- **Dado** os KPIs do [PRD §2.3](../prd.md#23-kpis-rastreados), **quando** os
  fatos correspondentes ocorrerem, **então** ficam registrados com horário e
  tenant: lead recebido, primeira resposta enviada, lead qualificado, visita
  agendada/realizada, dossiê aberto/completo, disparo de cobrança, relatório
  enviado.
- **E** as contra-métricas também são coletadas: opt-out e transbordo por falha
  de classificação.
- **E** o registro **não** contém dado pessoal — só identificadores internos.

### E8-F2-H2 `[MVP]` · 2 pts — Consulta interna dos números do produto
**Como** fundador, **quero** consultar os KPIs sem dashboard, **para** decidir
o roadmap com dado em vez de intuição.

- **Dado** os eventos coletados, **quando** eu rodar a consulta documentada,
  **então** obtenho tempo de primeira resposta (p50/p95), taxa de qualificação
  automática, duração mediana de dossiê e taxa de autoatendimento documental.
- **E** a consulta e sua interpretação estão documentadas no repositório.

---

## E8-F3 — Funil e gestão *(pós-MVP — Fase 8)*

| ID | Estória | Pts | Nota |
| --- | --- | --- | --- |
| E8-F3-H1 | Funil kanban de leads por etapa, com arrastar e soltar | 5 | RF8.2 |
| E8-F3-H2 | Funil de dossiês por etapa | 3 | |
| E8-F3-H3 | Dashboard de conversão: resposta, agendamento, comparecimento, fechamento | 5 | RF8.3 |
| E8-F3-H4 | Comparativo por corretor | 3 | Exige papel `gestor` (E1-F3-H3) |
| E8-F3-H5 | Exportar relatórios em CSV | 2 | |
| E8-F3-H6 | Alertas de anomalia (queda de resposta, pico de opt-out) | 5 | Contra-métricas viram alarme |
