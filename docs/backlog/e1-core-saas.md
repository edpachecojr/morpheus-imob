# E1 — Core SaaS, multi-tenancy e assinaturas

> **Objetivo:** base sobre a qual tudo é seguro por construção — isolamento de
> dados, identidade e permissão — mais o mecanismo de monetização.
> **Requisitos:** RF1 · **Fases:** 0 e 5 · **Fundação:** [multi-tenancy](../fundacao/multi-tenancy.md), [autenticacao](../fundacao/autenticacao.md), [autorizacao](../fundacao/autorizacao.md)

Este épico não entrega valor visível ao cliente. Entrega a propriedade de que
nenhum outro épico vaza dado, e refazê-lo depois custa migração de dados.

---

## E1-F0 — Esqueleto do projeto — ◐ em andamento

**Bloqueado por:** [ADR-0002](../adrs/0002-escolha-de-stack.md).

> **Pendente para fechar a F0** (ver [estado.md](./estado.md)):
> - **H1** — caminho crítico completo (webhook → fila → job → serviço externo
>   falso → persistência com tenant → leitura mobile) ainda não provado; só a
>   fatia de isolamento com Postgres real está coberta. Faltam também os 3
>   `<a definir>` de [stack.md](../fundacao/stack.md).
> - **H4** — só o lado de escrita foi entregue (eventos de domínio + gravação no
>   outbox, [ADR-0009](../adrs/0009-eventos-de-dominio-e-outbox.md)). Dispatcher,
>   filas, agendador e as garantias de job (retente/idempotência/fila morta)
>   seguem pendentes.

### ◐ E1-F0-H1 `[MVP]` · 3 pts — Spike de stack
**Como** time, **quero** provar o caminho crítico numa stack candidata, **para**
decidir com evidência em vez de preferência.

- **Dado** a stack candidata, **quando** eu implementar webhook → fila → job →
  serviço externo falso → persistência com tenant → leitura em página mobile,
  **então** existe teste automatizado que roda sem rede cobrindo o percurso.
- **E** o resultado é registrado no ADR-0002, cujo status muda para *aceito*.
- **E** [fundacao/stack.md](../fundacao/stack.md) e os `<a definir>` do
  CLAUDE.md são preenchidos.

### ✅ E1-F0-H2 `[MVP]` · 2 pts — Projeto sobe e a suíte roda
**Como** desenvolvedor, **quero** um comando para subir e um para testar,
**para** que ninguém invente processo próprio.

- **Dado** o repositório recém-clonado, **quando** eu seguir o README,
  **então** a aplicação sobe e responde `/health` em menos de 5 minutos de setup.
- **E** um comando único roda toda a suíte.
- **E** o formatador padrão da linguagem está configurado e aplicado.
- **E** a aplicação **falha ao subir** com variável de ambiente faltando,
  informando qual falta e o formato esperado.

### ✅ E1-F0-H3 `[MVP]` · 2 pts — CI barra o que não presta
**Como** time, **quero** verificação automática em todo push, **para** que a
regra não dependa de alguém lembrar na revisão.

- **Dado** um push, **quando** a CI rodar, **então** ela executa formatador,
  análise estática e testes.
- **E** falha em qualquer etapa impede o merge.
- **E** a CI roda sem acesso a serviço externo real.

> **✅ Feito:** `.github/workflows/ci.yml` roda em todo push e PR. Job 1 —
> `dotnet format --verify-no-changes` (formatação + análise), `dotnet build
> -warnaserror` (avisos viram erros) e os **testes unitários** (sem banco, sem
> rede). Job 2 — build da imagem Docker, só depois de o job 1 passar (`needs`).
> Os **testes de integração ficam fora do pipeline de propósito**: exigem Postgres
> via Testcontainers, o que contraria "CI sem serviço externo real". A imagem tem
> `Dockerfile` multi-stage (SDK compila, runtime ASP.NET carrega), com
> `.dockerignore` que barra `.env` de entrar na imagem.

### ◐ E1-F0-H4 `[MVP]` · 3 pts — Orientação a eventos e outbox (lado de escrita)
**Como** sistema, **quero** registrar de forma durável e atômica todo fato de
escrita, **para** sustentar reações assíncronas (dunning, lembretes, relatórios)
sem perder o fato entre gravar e anunciar.

> **Reformulada.** A ideia original — "fila de jobs e agendador" — amadureceu para
> a fundação orientada a eventos que a antecede ([ADR-0009](../adrs/0009-eventos-de-dominio-e-outbox.md)).
> A fila/agendador em si continua pendente (ver "Fora de escopo" abaixo).

- **Dado** uma entidade, **quando** ela nasce, **então** carrega identidade,
  auditoria (`CriadoEm`/`AtualizadoEm`) e uma lista de eventos de domínio, tudo
  concentrado numa entidade base.
- **E** uma ação de escrita (`Imovel.Cadastrar`, `Organizacao.Fundar`) registra um
  evento com os **dados de negócio completos**, não só um id.
- **E** o `SaveChanges` grava o evento na tabela `mensagens_outbox` **na mesma
  transação** do dado — uma escrita rejeitada não deixa evento órfão.
- **E** o evento carrega o tenant no envelope; outbox sem organização é recusado.

> **✅ Feito:** `EntidadeBase` + value object `DadosDeAuditoria`; eventos
> `ImovelCadastradoEvento`, `OrganizacaoFundadaEvento` e `UsuarioCadastradoEvento`;
> override explícito de `SaveChanges` no `MorpheusDbContext` gravando na mesma
> transação, com o montador puro testado por unidade e a atomicidade provada por
> teste de integração.
>
> **Fora de escopo (estória futura):** dispatcher/publicadores, filas e consumidores,
> agendador, e as garantias de job (retente com backoff, fila morta com alerta,
> idempotência, job agendado de exemplo com relógio falso). A coluna `processado_em`
> já existe, nula, como gancho para essa drenagem.

---

## E1-F1 — Tenant e isolamento — ◐ em andamento

### ✅ E1-F1-H1 `[MVP]` · 5 pts — Isolamento imposto na camada de dados
**Como** cliente do SaaS, **quero** que meus dados sejam inacessíveis a outros
tenants, **para** confiar documentos dos meus clientes à ferramenta.

- **Dado** dois tenants com dados, **quando** eu consultar no contexto do
  tenant A, **então** nenhuma linha do tenant B retorna.
- **E** consulta **sem** tenant no contexto falha ou retorna vazio.
- **E** escrita com `tenant_id` divergente do contexto é rejeitada.
- **E** FK apontando para entidade de outro tenant é rejeitada pelo banco.
- **E** os testes de [multi-tenancy](../fundacao/multi-tenancy.md#testes-obrigatórios) passam integralmente.

### ✅ E1-F1-H2 `[MVP]` · 2 pts — Criação de conta e tenant
**Como** corretor autônomo, **quero** criar minha conta, **para** começar a usar
sem falar com ninguém.

- **Dado** um e-mail não cadastrado, **quando** eu me cadastrar, **então** são
  criados tenant, conta e usuário com papel `dono`.
- **E** e-mail já cadastrado retorna mensagem genérica, sem revelar existência.
- **E** o tenant nasce com configuração padrão utilizável (fuso, janela de
  atendimento padrão).

> **✅ Feito:** `POST /contas` (anônimo, com limite por origem). Organização e
> usuário dono nascem na **mesma transação** (`IExecucaoTransacional`), com o papel
> gravado em `user_roles` do Identity. A organização nasce com o nome do fundador e
> `ConfiguracaoDaOrganizacao.Padrao()` — fuso `America/Sao_Paulo` e janela 9h–18h.
> Ambos os eventos (`OrganizacaoFundadaEvento`, `UsuarioCadastradoEvento`) entram no
> outbox na mesma transação.
>
> **Anti-enumeração:** cadastro criado, e-mail já existente e **armadilha de robô**
> (campo `paginaPessoal`, descartado antes de tocar banco ou hash) devolvem `201`
> com corpo byte a byte idêntico — provado por teste. A força da senha é validada
> **antes** da checagem de existência, senão a diferença de resposta viraria oráculo
> de contas.
>
> **Pendente (onboarding):** renomear a organização, completar dados de usuário e
> de organização e confirmar e-mail. Nada disso bloqueia o primeiro uso.

### ✅ E1-F1-H3 `[MVP]` · 1 pt — Chave única por tenant
**Como** cliente, **quero** usar meus próprios códigos de referência, **para**
não depender do que outro cliente já usou.

- **Dado** o imóvel `AP-101` no tenant A, **quando** o tenant B cadastrar
  `AP-101`, **então** ambos coexistem.
- **E** `AP-101` duplicado **dentro** do mesmo tenant é rejeitado com erro que
  cita o código conflitante.

---

## E1-F2 — Autenticação — ◐ em andamento

Regras completas em [autenticacao.md](../fundacao/autenticacao.md).
Sessão opaca no servidor: [ADR-0011](../adrs/0011-sessao-opaca-no-servidor.md).

### ✅ E1-F2-H1 `[MVP]` · 3 pts — Login por e-mail e senha
**Como** usuário, **quero** entrar com e-mail e senha, **para** acessar o painel.

- **Dado** credenciais válidas, **quando** eu logar, **então** recebo sessão em
  cookie `HttpOnly`, `Secure`, `SameSite=Lax`.
- **E** senha errada, e-mail inexistente e conta bloqueada retornam a **mesma**
  resposta, em tempo aproximadamente igual.
- **E** a senha é armazenada com Argon2id (ou bcrypt de custo adequado).
- **E** há rate limit por IP e por conta, com atraso progressivo.

> **✅ Feito:** `POST /sessoes` emite cookie `morpheus_sessao` — `HttpOnly`,
> `SameSite=Lax`, `Secure` fora de desenvolvimento — carregando apenas um
> identificador opaco; o ticket vive na tabela `sessoes` ([ADR-0011](../adrs/0011-sessao-opaca-no-servidor.md)).
> Senha com **Argon2id** (19 MiB, 2 passagens, OWASP 2025) substituindo o PBKDF2
> padrão do Identity, com regravação automática quando o custo sobe.
>
> **Tempo igual entre recusas:** os três modos — e-mail inexistente, senha errada e
> conta bloqueada — derivam uma chave Argon2id antes de responder, inclusive quando
> não há conta (`ConsumirTempoEquivalenteAsync` contra um hash de referência
> aleatório do processo). Os códigos de erro internos diferem para o log distinguir
> ataque de esquecimento; a resposta HTTP é idêntica, provado por teste.
>
> **Rate limit:** por origem (janela fixa configurável, padrão 20/min) nas rotas de
> autenticação, e por conta pelo bloqueio do Identity (5 erros → 15 min). O
> **atraso progressivo** do texto original foi deliberadamente não implementado: um
> atraso proporcional às falhas *da conta* responderia mais rápido para e-mail
> inexistente, que não tem contador — reintroduzindo exatamente o oráculo de
> enumeração que o critério anterior proíbe. Bloqueio por conta + teto por origem
> cobrem o mesmo risco sem esse efeito colateral.

### E1-F2-H2 `[MVP]` · 3 pts — Login com Google
**Como** corretor, **quero** entrar com minha conta Google, **para** não criar
mais uma senha.

- **Dado** conta Google com e-mail verificado igual ao cadastrado, **quando** eu
  logar, **então** vinculo ao usuário existente.
- **E** e-mail **não** verificado pelo provedor nunca vincula automaticamente.
- **E** primeiro login por Google sem cadastro prévio cria conta e tenant.

> **Adiada por decisão explícita**, para não misturar OIDC com a fundação de
> sessão. Entra sobre o que já está pronto: o `IUserLoginStore` do Identity e a
> `ISessaoDoPainel` não mudam.

### ✅ E1-F2-H3 `[MVP]` · 2 pts — Recuperação de senha
**Como** usuário, **quero** redefinir minha senha, **para** recuperar acesso.

- **Dado** um pedido de recuperação, **quando** enviado, **então** a resposta é
  genérica, exista ou não o e-mail.
- **E** o token é de uso único e expira em 30 minutos.
- **E** trocar a senha invalida todas as outras sessões ativas.

> **✅ Feito:** `POST /senhas/recuperacoes` responde `202` com o mesmo corpo exista
> ou não a conta; `POST /senhas/redefinicoes` troca a senha e apaga **todas** as
> sessões do usuário. Token do provedor do Identity, com `TokenLifespan` de 30 min;
> o uso único vem do carimbo de segurança, que gira na troca e invalida qualquer
> token pendente. Token forjado e conta inexistente devolvem resposta idêntica.
>
> **Pendente:** o envio real do e-mail. A porta `IEnvioDeEmailDeRecuperacao` está
> definida e hoje é atendida por `EnvioDeRecuperacaoRegistradoEmLog`, que registra
> o destinatário mascarado e **nunca o token**. Trocar pelo provedor transacional é
> registrar outra implementação da porta.

### ✅ E1-F2-H4 `[MVP]` · 1 pt — Encerrar sessão
- **Dado** uma sessão ativa, **quando** eu sair, **então** ela é revogada no
  servidor e a requisição seguinte recebe 401.

> **✅ Feito:** `DELETE /sessoes/atual` apaga a linha em `sessoes` — revogação de
> verdade no servidor, não apagar cookie no cliente. Derruba **apenas** o aparelho
> que saiu; os demais seguem ativos, provado por teste com dois clientes.

### E1-F2-H5 · 3 pts — MFA por TOTP
Pós-MVP. Requisito de venda para imobiliária com equipe.

---

## E1-F3 — Autorização — ◐ em andamento

Matriz de permissões em [autorizacao.md](../fundacao/autorizacao.md).
Papéis e permissões no Identity: [ADR-0010](../adrs/0010-papeis-e-permissoes-no-identity.md).

### ✅ E1-F3-H1 `[MVP]` · 3 pts — Verificação central por permissão nomeada
**Como** time, **quero** um único ponto de decisão de permissão, **para** que
adicionar papel não vire caça a `if` pelo código.

- **Dado** um endpoint, **quando** ele declarar a permissão exigida, **então**
  a verificação passa pelo ponto único `pode(usuario, permissao, recurso)`.
- **E** endpoint **sem** declaração é negado por padrão.
- **E** requisição sem sessão recebe 401 em toda rota autenticada.
- **E** `corretor` recebe 403 em `usuario.gerenciar` e `faturamento.gerenciar`.
- **E** `tenant_id` enviado no corpo da requisição é ignorado.
- **E** toda negação é logada com usuário, permissão e recurso.

> **✅ Feito:** ponto único `IAutorizadorDeAcesso.Pode(usuario, permissao)`, atrás
> de uma política registrada por permissão do catálogo — política inexistente
> quebra na **subida**, não em produção. A rota declara com `RequerPermissao(...)`
> ou `RequerApenasSessao()`; `ValidadorDeDeclaracaoDePermissao` percorre os
> endpoints na subida e **derruba o processo** citando pelo nome qualquer rota que
> não declarou nem anonimato — "negar por padrão" virou verificação, não convenção.
> Sem sessão, 401 em toda rota autenticada (política de fallback).
>
> **`tenant_id` do corpo:** nenhum contrato de requisição da API tem campo de
> organização, tenant, papel ou permissão — provado por teste de reflexão sobre o
> assembly, o que é mais forte que ignorar em tempo de execução. O tenant sai
> sempre de `IContextoDaOrganizacaoAtual`.
>
> **Negação logada** com id do usuário (nunca e-mail), permissão e rota.
>
> **Camada 3 (vínculo)** segue fora — é a E1-F3-H4, pós-MVP.

### ✅ E1-F3-H2 `[MVP]` · 2 pts — Papéis dono e corretor
- **Dado** os papéis do MVP, **quando** eu consultar as permissões, **então**
  elas conferem com a matriz documentada.
- **E** existe teste negativo para cada permissão restrita.

> **✅ Feito:** o enum `PapelDoUsuario` foi **removido**; papel vive em
> `roles`/`user_roles` do Identity e as permissões em `role_claims`, semeadas por
> migração a partir da `MatrizDePermissoes` ([ADR-0010](../adrs/0010-papeis-e-permissoes-no-identity.md)).
> Teste negativo por permissão restrita (`usuario.gerenciar`, `faturamento.gerenciar`,
> `tenant.configurar`, `metricas.ler`) no nível da matriz, e teste de integração com
> host real provando que o corretor recebe **403** em `/organizacao/usuarios` onde o
> dono recebe 200.

### E1-F3-H3 · 3 pts — Gestor, secretária e convite de equipe
Pós-MVP (Fase 5). Inclui convite por e-mail com aceite e revogação de acesso.

> **Preparado, não entregue.** Acrescentar `gestor` e `secretaria` é uma linha em
> `PapeisDoUsuario`, uma na `MatrizDePermissoes` e uma migração — nenhum código de
> rota muda ([ADR-0010](../adrs/0010-papeis-e-permissoes-no-identity.md)). O convite
> em si (e-mail, aceite, revogação) continua fora do MVP.

### E1-F3-H4 · 5 pts — Vínculo: corretor vê apenas os próprios registros
Pós-MVP. Camada 3 da autorização. O campo de responsável já é gravado desde o
MVP para não exigir migração.

> Fora de escopo por decisão da fundação: no MVP todo usuário do tenant vê tudo do
> tenant. `IAutorizadorDeAcesso` ganha o parâmetro de recurso quando esta estória
> entrar — acrescentá-lo agora, ignorado, só daria falsa sensação de cobertura.

---

## E1-F4 — Observabilidade — ◐ em andamento

### ✅ E1-F4-H1 `[MVP]` · 2 pts — Log JSON estruturado
**Como** operador, **quero** log pesquisável, **para** investigar incidente sem
adivinhar.

- **Dado** qualquer linha de log, **quando** emitida, **então** é JSON com
  `timestamp`, `nivel`, `mensagem`, `tenant_id` e `request_id`/`job_id`.
- **E** um teste automatizado prova que nenhum log contém token, senha, hash ou
  dado pessoal não mascarado.

> **✅ Feito:** Serilog + OpenTelemetry com sinks console (CLEF) e Seq local,
> exportação OTLP agnóstica de fornecedor ([ADR-0008](../adrs/0008-observabilidade-agnostica.md)),
> config no appsettings via `IOptions`. Cada linha traz `@t`/`@l`/`@mt` (CLEF, no
> lugar dos nomes literais — decisão registrada no ADR), correlação `TraceId`/
> `SpanId` e `dd.trace_id`/`dd.span_id` (decimal, para Datadog), e `organizacao_id`
> (o tenant_id) via `LogContext` quando há sessão. Log transversal entra por
> decorador no DI (OCP), sem mediator. `RedatorDeCamposSensiveis` mascara campos
> sensíveis, com teste anti-vazamento.
>
> **Observação:** `organizacao_id` só aparece com sessão autenticada; até a
> autenticação (E1-F2) existir, o campo fica ausente por construção.

### ☐ E1-F4-H2 `[MVP]` · 2 pts — Tratamento de erro ponta a ponta
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
