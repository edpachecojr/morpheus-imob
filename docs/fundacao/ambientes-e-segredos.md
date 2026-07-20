# Ambientes, configuração e segredos

## O repositório é público

Consequência prática, não aviso genérico:

- **Zero segredo em qualquer lugar do repositório** — código, teste, fixture,
  comentário, mensagem de commit, screenshot em documentação, exemplo de log.
- **Zero dado real** de cliente, imóvel, lead ou conversa. Exemplo usa dado
  fictício óbvio: `AP-101`, `Maria Fictícia`, `+55 11 90000-0000`.
- Segredo que vazar é considerado **comprometido para sempre**: rotacione,
  não apenas remova o commit. Histórico do Git é público e clonado.

## Ambientes

| Ambiente | Para quê | Dados |
| --- | --- | --- |
| `local` | Desenvolvimento na máquina | Seed fictício; provedores externos falsos |
| `test` | Suíte automatizada | Efêmero; **sem rede**, sem relógio real |
| `staging` | Validação antes de produção | Fictício; sandbox dos provedores |
| `production` | Clientes reais | Real, sob LGPD |

Regras entre ambientes:

- **Nunca copie dado de produção para outro ambiente.** Se precisar reproduzir
  um caso, gere fixture equivalente e anônima.
- Nenhum ambiente não-produtivo envia mensagem para número real. O enviador
  fora de produção é um fake nomeado que registra o que teria enviado.
- Cada ambiente tem credenciais próprias. Chave compartilhada entre staging e
  produção anula o valor de separá-los.

## Configuração

- **Toda configuração vem de variável de ambiente.** Sem arquivo de config com
  valor real versionado, sem valor embutido no código.
- **`.env.example` é obrigatório e versionado**, com toda variável documentada
  e valor de exemplo claramente falso.
- **`.env` e derivados no `.gitignore`** — já configurado.
- A aplicação **valida a configuração ao subir** e falha imediatamente, com
  mensagem dizendo qual variável falta e qual o formato esperado. Não suba
  degradado para descobrir a falta no meio de um disparo.
- Variável ausente **nunca** cai num default silencioso em produção.

### Variáveis previstas

Nomes definitivos entram no `.env.example` quando a stack for escolhida.
Categorias esperadas:

```
Aplicação        AMBIENTE, URL_BASE_APP, PORTA, NIVEL_DE_LOG
Banco            URL_DO_BANCO
Fila             URL_DA_FILA
Sessão/tokens    CHAVE_DE_SESSAO, CHAVE_DE_MAGIC_LINK
WhatsApp         WHATSAPP_TOKEN, WHATSAPP_ID_NUMERO, WHATSAPP_SEGREDO_WEBHOOK
Google           GOOGLE_CLIENT_ID, GOOGLE_CLIENT_SECRET, GOOGLE_REDIRECT_URI
Storage          STORAGE_ENDPOINT, STORAGE_BUCKET, STORAGE_CHAVE, STORAGE_SEGREDO
```

## Segredos em produção

- Guardados no cofre do provedor de hospedagem ou em gerenciador dedicado.
  Nunca em variável de CI impressa em log, nunca em chat.
- Rotação: anual por padrão; **imediata** em suspeita de vazamento ou saída de
  quem tinha acesso.
- Acesso mínimo: cada credencial com o menor escopo que funciona.
- Webhook de entrada **valida assinatura** do provedor antes de processar
  qualquer coisa. Endpoint sem verificação de assinatura é entrada aberta.

## Logging

- **JSON estruturado com campos nomeados.** Prosa em log é inútil para busca.
- Campos obrigatórios em toda linha: `timestamp`, `nivel`, `mensagem`,
  `tenant_id`, `request_id` (ou `job_id`).
- **Proibido logar**: token, senha, hash, credencial, número de documento,
  conteúdo de anexo, corpo de mensagem de cliente. Telefone e e-mail apenas
  mascarados.
- Erro carrega contexto útil — o valor que causou a falha e o formato esperado
  — **sem** carregar o dado pessoal em si.
- Texto plano só em saída de CLI para humano.

## LGPD — o mínimo estrutural

Não é backlog futuro; condiciona o modelo de dados desde o primeiro dia:

| Requisito | Como atendemos |
| --- | --- |
| Base legal e finalidade | Registradas por tipo de dado; consentimento no primeiro contato do WhatsApp |
| Minimização | Só coletamos o que o checklist exige; sem campo "por precaução" |
| Retenção | Prazo por tipo de dado, com exclusão automática ao vencer ([Q5 do PRD](../prd.md)) |
| Exclusão a pedido | Rotina que apaga dado pessoal e anexos, preservando registro contábil mínimo |
| Auditoria | Todo acesso e download de documento registrado ([autenticacao](autenticacao.md)) |
| Opt-out | `descartado` por pedido do titular é terminal e silencia todos os módulos |
