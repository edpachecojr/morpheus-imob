# ADR-0011 — Sessão opaca no servidor, com cookie de identificador

**Status:** Aceito
**Data:** 2026-07-21
**Detalhe:** [autenticacao.md](../fundacao/autenticacao.md)

## Contexto

[autenticacao.md](../fundacao/autenticacao.md) decidiu **sessão opaca no
servidor**, recusando JWT sem estado porque precisamos revogar acesso na hora —
usuário desligado, aparelho perdido, senha trocada sob suspeita de invasão.

O caminho de menor esforço com IdentityCore seria o cookie cifrado padrão, em que
a identidade inteira viaja dentro do cookie e o servidor não guarda nada. Ele
atende ao "cookie `HttpOnly`/`Secure`/`SameSite=Lax`" e nada mais: um logout
apenas apaga o cookie no cliente, e um cookie copiado antes disso continua
valendo até expirar. A alternativa nativa — girar o carimbo de segurança da conta —
derruba **todos** os aparelhos do usuário, o que é o comportamento errado para um
logout comum.

## Decisão

O cookie de sessão carrega **apenas um identificador opaco**; o ticket de
autenticação fica no Postgres, na tabela `sessoes`, atrás do `ITicketStore` do
ASP.NET Core.

```
cookie morpheus_sessao=<guid>  →  sessoes  →  ClaimsPrincipal
```

Com isso, revogar é apagar linha, e a granularidade acompanha a intenção:

| Evento | Efeito |
| --- | --- |
| Logout | `DELETE ... WHERE id = @sessao` — só o aparelho que saiu |
| Troca de senha | `DELETE ... WHERE usuario_id = @usuario` — todos os aparelhos |
| Usuário removido | Cascata da FK — nenhuma sessão órfã restaura identidade morta |

A tabela é lida e escrita por **Dapper em conexão própria**, e não pelo
`MorpheusDbContext`, pelo mesmo motivo da resolução de tenant: a sessão é
restaurada antes de existir escopo de requisição, e amarrá-la ao contexto de
dados que ela mesma habilita criaria dependência circular. O EF conhece a tabela
só para gerá-la nas migrações.

A abertura de sessão **não** usa `SignInManager`. Ele embute fluxos que não temos
(dois fatores, login externo, confirmação de e-mail) e esconderia qual conferência
de senha realmente aconteceu. A conferência é do caso de uso; o host apenas monta
a identidade pelo `IUserClaimsPrincipalFactory` e emite o cookie.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| Cookie cifrado do Identity, sem store | Não revoga sessão individual. Logout é ato do cliente, não do servidor |
| Cookie + `SecurityStampValidator` com `ValidationInterval = 0` | Revoga só no atacado: derrubar um aparelho derruba todos. E cobra uma consulta ao banco por requisição sem entregar a granularidade |
| JWT curto com refresh token | Reintroduz a janela de invalidação que a fundação recusou, e troca uma tabela de sessão por uma tabela de refresh — mesma escrita, menos garantia |
| Sessão em Redis | Infraestrutura a mais antes de haver volume que a justifique. O `IArmazenamentoDeSessoes` é a costura por onde ela entra sem tocar em caso de uso |

## Consequências

**Positivas**
- Revogação imediata e na granularidade certa: um aparelho ou todos, conforme o
  evento.
- Cookie roubado sem a linha correspondente não vale nada.
- Trocar o armazenamento (Redis, banco dedicado) é implementar
  `IArmazenamentoDeSessoes`; o resto do sistema não fica sabendo.

**Negativas**
- Uma consulta ao Postgres por requisição autenticada. É o custo direto de poder
  revogar; se doer, o mesmo contrato aceita um cache.
- Sessão expirada não é apagada sozinha: a consulta filtra por validade, então
  ela nunca concede acesso, mas a linha morta ocupa espaço até a varredura
  periódica entrar com o agendador (E1-F0-H4).
- Em ambiente de desenvolvimento o cookie sai sem `Secure`, porque a API roda em
  http — condicional explícita em `ConfiguracaoDeAutenticacao`, nunca em produção.

**Como saberemos que erramos:** se a leitura da sessão aparecer no topo do perfil
de latência, ou se o volume de linhas mortas exigir varredura mais frequente que
diária, a decisão seguinte é mover o armazenamento para cache distribuído — sem
mexer no modelo, só na implementação da porta.
