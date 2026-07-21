# Autorização

O que você pode fazer, uma vez que já sabemos quem é você
([autenticacao.md](autenticacao.md)).

**Decisão:** RBAC simples — papel fixo por usuário, permissões nomeadas,
verificação centralizada. Sem ACL por recurso, sem motor de políticas.
Trade-offs em [ADR-0005](../adrs/0005-rbac-simples.md).

**Onde isso mora:** o papel do usuário fica nas tabelas do IdentityCore
(`roles`, `user_roles`) e cada permissão é uma claim do papel em `role_claims`,
semeada por migração a partir da matriz abaixo — que continua sendo a fonte em
código. Detalhe e trade-offs em
[ADR-0010](../adrs/0010-papeis-e-permissoes-no-identity.md).

## Camadas

A autorização é sempre a composição de três perguntas, **nesta ordem**:

```
1. Tenant    — este recurso é do tenant do contexto?   (multi-tenancy, camada de dados)
2. Papel     — este papel pode executar esta ação?     (RBAC)
3. Vínculo   — este usuário tem relação com o recurso? (pós-MVP: corretor vê só o dele)
```

A camada 1 nunca é opcional e não vive no código de autorização — ela é
estrutural, em [multi-tenancy](multi-tenancy.md). A 3 fica desligada no MVP
(todo usuário do tenant vê tudo do tenant), mas o dado de responsável já é
gravado desde o início para ligá-la sem migração.

## Papéis

| Papel | Quem é | MVP |
| --- | --- | --- |
| `dono` | Contratou o SaaS. Pode tudo dentro do tenant, inclusive faturamento e usuários | ✅ |
| `corretor` | Atende lead, faz visita, opera dossiê | ✅ |
| `gestor` | Dono sem acesso a faturamento; vê equipe inteira | ❌ |
| `secretaria` | Opera dossiê e agenda; não vê métrica de desempenho | ❌ |

## Permissões

Verificação é sempre por **permissão nomeada**, nunca por comparação de papel
espalhada no código. `if (papel === 'dono')` é dívida: quando surgir `gestor`,
alguém vai esquecer um lugar.

| Permissão | dono | corretor | gestor | secretaria |
| --- | :-: | :-: | :-: | :-: |
| `imovel.ler` / `imovel.escrever` | ✅ | ✅ | ✅ | ✅ |
| `lead.ler` / `lead.atender` | ✅ | ✅ | ✅ | ✅ |
| `visita.agendar` | ✅ | ✅ | ✅ | ✅ |
| `dossie.criar` / `dossie.ler` | ✅ | ✅ | ✅ | ✅ |
| `dossie.aprovar_item` | ✅ | ✅ | ✅ | ✅ |
| `dossie.baixar_anexo` | ✅ | ✅ | ✅ | ✅ |
| `magiclink.emitir` / `magiclink.revogar` | ✅ | ✅ | ✅ | ✅ |
| `relatorio.gerar` / `relatorio.enviar` | ✅ | ✅ | ✅ | ✅ |
| `usuario.gerenciar` | ✅ | ❌ | ✅ | ❌ |
| `tenant.configurar` | ✅ | ❌ | ✅ | ❌ |
| `faturamento.gerenciar` | ✅ | ❌ | ❌ | ❌ |
| `metricas.ler` | ✅ | ❌ | ✅ | ❌ |

## Regras

1. **Negar por padrão.** Rota sem verificação explícita é negada, não liberada.
   Isso é configuração de framework, não disciplina de quem escreve a rota.
   Na prática, duas camadas: política de fallback que exige sessão, e uma
   verificação na **subida** que derruba o processo citando pelo nome qualquer
   rota que não declarou permissão, sessão ou anonimato.
2. **Verificação no servidor, sempre.** Esconder botão é UX, não segurança.
3. **Um único ponto de decisão** com assinatura tipada — `pode(usuario,
   permissao, recurso)`. Não existe segunda forma de perguntar.
4. **Nunca aceite papel, permissão ou `tenant_id` vindos do cliente.**
5. **Negação é logada** com usuário, permissão e recurso: acúmulo de negações é
   sinal de bug de UX ou de tentativa de abuso.
6. **Magic Link não tem papel.** Ele carrega um escopo fixo e minúsculo: ler e
   anexar nos itens do **próprio** participante. Nada mais — nem listar outros
   dossiês, nem ver dados do imóvel além do necessário.
7. **Job de background roda com identidade de sistema**, com tenant explícito e
   escopo mínimo. Nunca herda a sessão de quem enfileirou.

## Testes obrigatórios

Todo endpoint novo ganha **teste negativo** — este é o ponto em que a maioria
dos sistemas falha, porque só se testa o caminho feliz:

| Teste | Prova |
| --- | --- |
| `corretor` recebe 403 em `usuario.gerenciar` e `faturamento.gerenciar` | Papel restringe de fato |
| Requisição sem sessão recebe 401 em toda rota autenticada | Default nega |
| Rota nova sem declaração de permissão é negada | O default é estrutural |
| Magic Link não acessa item de outro participante do mesmo dossiê | Escopo mínimo |
| Magic Link não acessa nenhuma rota do painel | Escopos não se confundem |
| `tenant_id` forjado no corpo da requisição é ignorado | Camada 1 |
| Job sem tenant explícito falha | Caminho assíncrono |
