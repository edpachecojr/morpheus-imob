# E1 — Core SaaS, multi-tenancy e assinaturas

> **Objetivo:** base sobre a qual tudo é seguro por construção — isolamento de
> dados, identidade e permissão — mais o mecanismo de monetização.
> **Requisitos:** RF1 · **Fases:** 0 e 5 · **Fundação:** [multi-tenancy](../fundacao/multi-tenancy.md), [autenticacao](../fundacao/autenticacao.md), [autorizacao](../fundacao/autorizacao.md)

Este épico não entrega valor visível ao cliente. Entrega a propriedade de que
nenhum outro épico vaza dado, e refazê-lo depois custa migração de dados.

---

## E1-F0 — Esqueleto do projeto

**Bloqueado por:** [ADR-0002](../adrs/0002-escolha-de-stack.md).

### E1-F0-H1 `[MVP]` · 3 pts — Spike de stack
**Como** time, **quero** provar o caminho crítico numa stack candidata, **para**
decidir com evidência em vez de preferência.

- **Dado** a stack candidata, **quando** eu implementar webhook → fila → job →
  serviço externo falso → persistência com tenant → leitura em página mobile,
  **então** existe teste automatizado que roda sem rede cobrindo o percurso.
- **E** o resultado é registrado no ADR-0002, cujo status muda para *aceito*.
- **E** [fundacao/stack.md](../fundacao/stack.md) e os `<a definir>` do
  CLAUDE.md são preenchidos.

### E1-F0-H2 `[MVP]` · 2 pts — Projeto sobe e a suíte roda
**Como** desenvolvedor, **quero** um comando para subir e um para testar,
**para** que ninguém invente processo próprio.

- **Dado** o repositório recém-clonado, **quando** eu seguir o README,
  **então** a aplicação sobe e responde `/health` em menos de 5 minutos de setup.
- **E** um comando único roda toda a suíte.
- **E** o formatador padrão da linguagem está configurado e aplicado.
- **E** a aplicação **falha ao subir** com variável de ambiente faltando,
  informando qual falta e o formato esperado.

### E1-F0-H3 `[MVP]` · 2 pts — CI barra o que não presta
**Como** time, **quero** verificação automática em todo push, **para** que a
regra não dependa de alguém lembrar na revisão.

- **Dado** um push, **quando** a CI rodar, **então** ela executa formatador,
  análise estática e testes.
- **E** falha em qualquer etapa impede o merge.
- **E** a CI roda sem acesso a serviço externo real.

### E1-F0-H4 `[MVP]` · 3 pts — Fila de jobs e agendador
**Como** sistema, **quero** executar trabalho assíncrono e agendado, **para**
sustentar dunning, lembretes e relatórios.

- **Dado** um job enfileirado, **quando** ele falhar, **então** é retentado com
  backoff até um teto e, esgotado, vai para fila morta com alerta.
- **E** o mesmo job processado duas vezes produz **um** efeito (idempotência).
- **E** job sem `tenant_id` no payload falha ao iniciar — nunca roda "global".
- **E** existe um job agendado de exemplo com teste usando relógio falso.

---

## E1-F1 — Tenant e isolamento

### E1-F1-H1 `[MVP]` · 5 pts — Isolamento imposto na camada de dados
**Como** cliente do SaaS, **quero** que meus dados sejam inacessíveis a outros
tenants, **para** confiar documentos dos meus clientes à ferramenta.

- **Dado** dois tenants com dados, **quando** eu consultar no contexto do
  tenant A, **então** nenhuma linha do tenant B retorna.
- **E** consulta **sem** tenant no contexto falha ou retorna vazio.
- **E** escrita com `tenant_id` divergente do contexto é rejeitada.
- **E** FK apontando para entidade de outro tenant é rejeitada pelo banco.
- **E** os testes de [multi-tenancy](../fundacao/multi-tenancy.md#testes-obrigatórios) passam integralmente.

### E1-F1-H2 `[MVP]` · 2 pts — Criação de conta e tenant
**Como** corretor autônomo, **quero** criar minha conta, **para** começar a usar
sem falar com ninguém.

- **Dado** um e-mail não cadastrado, **quando** eu me cadastrar, **então** são
  criados tenant, conta e usuário com papel `dono`.
- **E** e-mail já cadastrado retorna mensagem genérica, sem revelar existência.
- **E** o tenant nasce com configuração padrão utilizável (fuso, janela de
  atendimento padrão).

### E1-F1-H3 `[MVP]` · 1 pt — Chave única por tenant
**Como** cliente, **quero** usar meus próprios códigos de referência, **para**
não depender do que outro cliente já usou.

- **Dado** o imóvel `AP-101` no tenant A, **quando** o tenant B cadastrar
  `AP-101`, **então** ambos coexistem.
- **E** `AP-101` duplicado **dentro** do mesmo tenant é rejeitado com erro que
  cita o código conflitante.

---

## E1-F2 — Autenticação

Regras completas em [autenticacao.md](../fundacao/autenticacao.md).

### E1-F2-H1 `[MVP]` · 3 pts — Login por e-mail e senha
**Como** usuário, **quero** entrar com e-mail e senha, **para** acessar o painel.

- **Dado** credenciais válidas, **quando** eu logar, **então** recebo sessão em
  cookie `HttpOnly`, `Secure`, `SameSite=Lax`.
- **E** senha errada, e-mail inexistente e conta bloqueada retornam a **mesma**
  resposta, em tempo aproximadamente igual.
- **E** a senha é armazenada com Argon2id (ou bcrypt de custo adequado).
- **E** há rate limit por IP e por conta, com atraso progressivo.

### E1-F2-H2 `[MVP]` · 3 pts — Login com Google
**Como** corretor, **quero** entrar com minha conta Google, **para** não criar
mais uma senha.

- **Dado** conta Google com e-mail verificado igual ao cadastrado, **quando** eu
  logar, **então** vinculo ao usuário existente.
- **E** e-mail **não** verificado pelo provedor nunca vincula automaticamente.
- **E** primeiro login por Google sem cadastro prévio cria conta e tenant.

### E1-F2-H3 `[MVP]` · 2 pts — Recuperação de senha
**Como** usuário, **quero** redefinir minha senha, **para** recuperar acesso.

- **Dado** um pedido de recuperação, **quando** enviado, **então** a resposta é
  genérica, exista ou não o e-mail.
- **E** o token é de uso único e expira em 30 minutos.
- **E** trocar a senha invalida todas as outras sessões ativas.

### E1-F2-H4 `[MVP]` · 1 pt — Encerrar sessão
- **Dado** uma sessão ativa, **quando** eu sair, **então** ela é revogada no
  servidor e a requisição seguinte recebe 401.

### E1-F2-H5 · 3 pts — MFA por TOTP
Pós-MVP. Requisito de venda para imobiliária com equipe.

---

## E1-F3 — Autorização

Matriz de permissões em [autorizacao.md](../fundacao/autorizacao.md).

### E1-F3-H1 `[MVP]` · 3 pts — Verificação central por permissão nomeada
**Como** time, **quero** um único ponto de decisão de permissão, **para** que
adicionar papel não vire caça a `if` pelo código.

- **Dado** um endpoint, **quando** ele declarar a permissão exigida, **então**
  a verificação passa pelo ponto único `pode(usuario, permissao, recurso)`.
- **E** endpoint **sem** declaração é negado por padrão.
- **E** requisição sem sessão recebe 401 em toda rota autenticada.
- **E** `corretor` recebe 403 em `usuario.gerenciar` e `faturamento.gerenciar`.
- **E** `tenant_id` enviado no corpo da requisição é ignorado.
- **E** toda negação é logada com usuário, permissão e recurso.

### E1-F3-H2 `[MVP]` · 2 pts — Papéis dono e corretor
- **Dado** os papéis do MVP, **quando** eu consultar as permissões, **então**
  elas conferem com a matriz documentada.
- **E** existe teste negativo para cada permissão restrita.

### E1-F3-H3 · 3 pts — Gestor, secretária e convite de equipe
Pós-MVP (Fase 5). Inclui convite por e-mail com aceite e revogação de acesso.

### E1-F3-H4 · 5 pts — Vínculo: corretor vê apenas os próprios registros
Pós-MVP. Camada 3 da autorização. O campo de responsável já é gravado desde o
MVP para não exigir migração.

---

## E1-F4 — Observabilidade

### E1-F4-H1 `[MVP]` · 2 pts — Log JSON estruturado
**Como** operador, **quero** log pesquisável, **para** investigar incidente sem
adivinhar.

- **Dado** qualquer linha de log, **quando** emitida, **então** é JSON com
  `timestamp`, `nivel`, `mensagem`, `tenant_id` e `request_id`/`job_id`.
- **E** um teste automatizado prova que nenhum log contém token, senha, hash ou
  dado pessoal não mascarado.

### E1-F4-H2 `[MVP]` · 2 pts — Tratamento de erro ponta a ponta
- **Dado** um erro não tratado, **quando** ocorrer, **então** o usuário recebe
  mensagem útil sem detalhe interno, e o log registra o contexto completo.
- **E** o erro carrega o valor que causou a falha e o formato esperado.

### E1-F4-H3 · 3 pts — Métricas de produto instrumentadas
Pós-MVP formal, mas os KPIs do PRD §2.3 devem começar a ser coletados na Fase 1.

---

## E1-F5 — Assinatura e cobrança *(pós-MVP — Fase 5)*

| ID | Estória | Pts |
| --- | --- | --- |
| E1-F5-H1 | Assinar plano por gateway com cobrança recorrente | 5 |
| E1-F5-H2 | Bloquear acesso automaticamente por inadimplência, preservando os dados | 3 |
| E1-F5-H3 | Trocar de plano com cobrança proporcional | 3 |
| E1-F5-H4 | Emitir e consultar histórico de faturas | 2 |

No MVP a cobrança é manual (Pix ou link), por decisão registrada no PRD §5.

---

## E1-F6 — White-label *(pós-MVP — Fase 5)*

| ID | Estória | Pts |
| --- | --- | --- |
| E1-F6-H1 | Subir logo e cor do tenant | 2 |
| E1-F6-H2 | Aplicar identidade no Magic Link e nos relatórios | 3 |
