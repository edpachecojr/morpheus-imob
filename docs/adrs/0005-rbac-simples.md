# ADR-0005 — RBAC simples com permissões nomeadas

**Status:** Aceito
**Data:** 2026-07-20
**Detalhe:** [autorizacao.md](../fundacao/autorizacao.md)

## Contexto

O sistema tem quatro papéis previstos (dono, gestor, corretor, secretária), dos
quais **dois** entram no MVP, e um público extra que não tem papel nenhum: o
participante do dossiê acessando por Magic Link, cujo escopo é minúsculo e
fixo (ler e anexar nos próprios itens).

O risco a controlar não é a expressividade das regras — é o endpoint que
esquece de verificar qualquer coisa. Motor de políticas sofisticado não resolve
esse risco; às vezes o agrava, ao tornar difícil enxergar o que uma rota exige.

## Decisão

RBAC simples: papel fixo por usuário, permissões nomeadas granulares, e um
**único ponto de verificação** com assinatura tipada — `pode(usuario,
permissao, recurso)`. Toda rota declara a permissão que exige; rota sem
declaração é negada por padrão, estruturalmente, não por disciplina.

Comparação direta de papel no código de negócio (`if papel == 'dono'`) é
proibida: verifica-se permissão, nunca papel.

O Magic Link não participa do RBAC. Ele carrega escopo fixo e mínimo,
verificado em caminho próprio e separado.

## Alternativas consideradas

| Alternativa | Por que não |
| --- | --- |
| Checar papel direto no código | Quando entrar `gestor`, alguém esquece um lugar. É a origem clássica do bug de escalada |
| ABAC / motor de políticas (OPA, Cedar) | Peso desproporcional a quatro papéis; dificulta ver o que uma rota exige e adiciona infraestrutura para operar |
| ACL por recurso | Nenhum requisito atual pede permissão por recurso individual. Complexidade sem demanda |
| Magic Link como papel do RBAC | Escopos que não se parecem em nada; unir convida a um bug em que o link acessa rota de painel |

## Consequências

**Positivas**
- Simples de entender, simples de testar. Teste negativo por endpoint é trivial
  de escrever e por isso será escrito.
- Adicionar papel é editar uma tabela de permissões, não caçar `if` pelo código.
- Escopos separados impedem a confusão mais perigosa (link público × painel).

**Negativas**
- Não expressa regra de vínculo ("corretor vê só os próprios leads"). Ela vira
  a camada 3 da [autorizacao](../fundacao/autorizacao.md), desligada no MVP —
  mas o campo de responsável é gravado desde o início para ligá-la sem migração.
- Permissão nomeada exige nomear bem. Nomes vagos (`gerenciar_tudo`) reintroduzem
  o problema que a decisão elimina.

**Como saberemos que erramos:** se surgir demanda de permissão por recurso
individual ou por condição dinâmica (horário, valor, hierarquia de equipe) em
mais de dois lugares, é hora de um ADR sucessor. Um caso isolado não justifica
trocar o modelo.
