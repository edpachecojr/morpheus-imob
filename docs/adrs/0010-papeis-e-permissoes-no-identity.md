# ADR-0010 — Papéis e permissões no Identity, não em enum

**Status:** Aceito
**Data:** 2026-07-21
**Sucede parcialmente:** [ADR-0005](0005-rbac-simples.md) (que permanece válido na
decisão de *o quê*: RBAC simples com permissão nomeada e ponto único de decisão).
**Detalhe:** [autorizacao.md](../fundacao/autorizacao.md)

## Contexto

A ADR-0005 decidiu **RBAC simples com permissões nomeadas**. A primeira
implementação materializou o papel como um `enum PapelDoUsuario` numa coluna
`usuarios.papel`, antes de existir autenticação.

Ao entrar a autenticação de verdade (E1-F2), a decisão de usar o **IdentityCore
por completo** — store de usuário, hash de senha, bloqueio por tentativas,
tokens de recuperação — trouxe junto as tabelas `roles`, `user_roles`,
`role_claims`, que existem exatamente para guardar papel e permissão.

Manter o enum ao lado delas criaria **duas verdades sobre a mesma coisa**: a
coluna que ninguém consulta na hora de autorizar, e a tabela que o
`UserClaimsPrincipalFactory` realmente lê ao montar a identidade da sessão. Duas
verdades divergem — é questão de tempo.

## Decisão

**O papel do usuário vive nas tabelas do Identity.** O enum `PapelDoUsuario` e a
coluna `usuarios.papel` foram removidos. O domínio guarda apenas os **nomes**
canônicos dos papéis (`PapeisDoUsuario`), que atravessam banco, claim e teste sem
tradução.

**As permissões são claims do papel**, gravadas em `role_claims` com o tipo
`permissao`, e **semeadas por migração** a partir da `MatrizDePermissoes` — que
continua expressa em código, como fonte da semeadura.

Disso decorre a divisão de autoridade:

| | Autoridade | Onde |
| --- | --- | --- |
| **Execução** | `role_claims` no banco | O `UserClaimsPrincipalFactory` traz as claims do papel para dentro da sessão |
| **Revisão** | `MatrizDePermissoes` em código | Mudança de permissão aparece no diff do PR e vira migração |

O ponto único de decisão da ADR-0005 continua único: `IAutorizadorDeAcesso.Pode`,
que hoje só olha a claim. Toda rota declara o que exige com `RequerPermissao` ou
`RequerApenasSessao`, e uma verificação na **subida** derruba o processo se
alguma rota não declarou nenhum dos dois — "negar por padrão" deixa de depender
da disciplina de quem escreve a rota.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| Manter o enum e ignorar as tabelas do Identity | Duas verdades sobre papel. A que autoriza seria a que ninguém edita |
| Papel no Identity, matriz **só** em código | Perde a semeadura versionada e obriga toda decisão de permissão a passar por deploy. Foi a alternativa mais forte; caiu por "Identity por completo" |
| Permissão como claim **do usuário** (`user_claims`) | Trocar o papel de alguém exigiria reescrever linha por usuário. Papel é o agrupador; é nele que a permissão pertence |
| Provedor de política sob demanda (`IAuthorizationPolicyProvider`) | Política inexistente só falharia ao ser exercida, em produção. Registrar todas na subida faz a permissão inventada quebrar no primeiro `dotnet run` |

## Consequências

**Positivas**
- Uma verdade só. O que autoriza é o que está gravado, e o que está gravado veio
  de uma migração revisada.
- Acrescentar `gestor` e `secretaria` (Fase 5) é uma linha em `PapeisDoUsuario`,
  uma na `MatrizDePermissoes` e uma migração — sem tocar em código de rota.
- Endpoint sem declaração de acesso não sobe. A regra 1 de
  [autorizacao.md](../fundacao/autorizacao.md) virou verificação, não convenção.

**Negativas**
- A matriz é **append-only**: os ids das claims semeadas são fixos, então inserir
  permissão no meio da lista renumera as seguintes e produz uma migração maior
  que o necessário. Documentado em `ConfiguracaoDePapeisEPermissoes`.
- As permissões viajam dentro da sessão. Revogar uma permissão de um papel só
  alcança quem já está logado quando a sessão é reemitida — aceitável porque
  mudança de matriz é evento raro e acompanhado de deploy.
- Comparar papel no código continua proibido, mas agora há um segundo jeito
  tentador de errar: `User.IsInRole("dono")`. Continua valendo a regra — verifica-se
  permissão, nunca papel.

**Como saberemos que erramos:** se a matriz passar a mudar com frequência de
configuração (mais de uma vez por trimestre fora de release), ou se aparecer
demanda de permissão por organização, a semeadura por migração vira gargalo e o
sucessor desta ADR move a matriz para dado editável com tela.
