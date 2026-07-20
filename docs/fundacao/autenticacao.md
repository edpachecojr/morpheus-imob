# Autenticação

Quem é você. Permissões são outro assunto: [autorizacao.md](autorizacao.md).

O sistema tem **três públicos com necessidades de identidade opostas**, e
tratá-los com o mesmo mecanismo seria o erro estrutural mais caro do projeto:

| Público | Mecanismo | Por quê |
| --- | --- | --- |
| **Usuário do painel** (corretor, dono) | Sessão com e-mail/senha ou Google | Uso recorrente, dado sensível de vários clientes, precisa de credencial |
| **Participante do dossiê** (locatário, fiador) | [Magic Link](#magic-link) | Usa uma vez. Exigir cadastro aqui mata a taxa de conclusão — que é o KPI do módulo |
| **Lead no WhatsApp** | Número de telefone verificado pelo canal | Não existe login; a identidade é a que a Meta entrega |

## Sessão do painel

- **Sessão opaca no servidor**, em cookie `HttpOnly`, `Secure`, `SameSite=Lax`.
  Não JWT sem estado: precisamos revogar sessão na hora (usuário desligado,
  aparelho perdido) e JWT stateless torna isso um problema de invalidação.
- Senha com hash de **Argon2id** (ou bcrypt com custo adequado). Nunca SHA.
- **Login com Google** pelo padrão OIDC. O e-mail verificado do provedor
  vincula à conta existente; e-mail não verificado nunca vincula.
- Rate limit por IP e por conta no login, com atraso progressivo.
- **Resposta genérica** em falha de login e em recuperação de senha: nunca
  revelar se o e-mail existe.
- Recuperação de senha por token de uso único, expiração de 30 min, invalidado
  no uso e a cada troca de senha.
- Toda sessão ativa some quando a senha muda.

**Adiado:** MFA, SSO corporativo, convite de equipe com aceite. Registrados no
[E1](../backlog/e1-core-saas.md); não bloqueiam o MVP com um corretor autônomo.

## Magic Link

O ponto mais delicado do sistema: uma URL que dá acesso a documento de
identidade **sem senha**. Regras não negociáveis:

| Regra | Valor | Motivo |
| --- | --- | --- |
| Entropia do token | ≥ 128 bits, gerador criptográfico | Adivinhação inviável |
| Armazenamento | Apenas o **hash** do token no banco | Vazamento do banco não vira acesso |
| Escopo | Um participante, um dossiê | Fiador não vê documento do locatário |
| Expiração | 7 dias, renovável por reenvio | Link em histórico de WhatsApp é eterno; o acesso não pode ser |
| Revogação | Imediata, pelo corretor | Link enviado ao número errado precisa morrer agora |
| Encerramento automático | Ao completar ou cancelar o dossiê | Sem acesso residual |
| Auditoria | Todo acesso e todo download registrados | Trilha de RNF14 |
| Indexação | `noindex`, sem token em `Referer` | Link de dossiê fora do Google |
| Rate limit | Por token e por IP | Contém abuso e enumeração |
| Resposta a token inválido | Página genérica de "link expirado" | Não distinguir inexistente de expirado |

**Não faremos**: token no fragmento da URL como "segurança", link permanente,
ou reaproveitar o mesmo token entre participantes. Se o valor do acesso subir
(ex.: passar a expor contrato assinado), reavalie com segundo fator por SMS.

## Acesso a arquivo

Nenhum documento é servido por URL pública ou adivinhável. O fluxo é sempre:

```
requisição autenticada (sessão ou magic link)
  → autorização verifica dono e tenant
  → gera URL assinada de curta duração (≤ 5 min)
  → cliente baixa direto do storage
```

## Segredos

Nenhuma credencial no repositório — ele é público
([ambientes-e-segredos](ambientes-e-segredos.md)). Chaves de assinatura de
sessão e token vêm de variável de ambiente e são rotacionáveis sem deslogar
todo mundo de uma vez.

## Testes obrigatórios

- Login falha com senha errada, com e-mail inexistente e com conta bloqueada —
  todas com a **mesma** resposta e o mesmo tempo aproximado.
- Token de magic link expirado, revogado, malformado e de outro tenant: todos
  negados, com a mesma resposta.
- Sessão revogada perde acesso na requisição seguinte.
- Troca de senha derruba as outras sessões.
- URL assinada expirada não baixa o arquivo.
- Nenhum log contém token, senha ou hash — teste automatizado sobre a saída.
