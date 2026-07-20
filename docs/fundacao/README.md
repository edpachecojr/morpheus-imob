# Fundação técnica

Bases que precisam estar de pé **antes** de qualquer funcionalidade de negócio.
Fazer isso depois custa uma migração de dados e uma reescrita de autorização.

| Página | Pergunta que responde | Estado |
| --- | --- | --- |
| [stack.md](stack.md) | Com o que construímos? | **Em aberto** |
| [multi-tenancy.md](multi-tenancy.md) | Como um cliente nunca vê o dado do outro? | Decidido (implementação pendente de stack) |
| [autenticacao.md](autenticacao.md) | Como sabemos quem é você? | Decidido |
| [autorizacao.md](autorizacao.md) | O que você pode fazer? | Decidido |
| [ambientes-e-segredos.md](ambientes-e-segredos.md) | Onde roda e como se configura? | Decidido |

## Ordem de construção da fundação

Cada passo depende do anterior. Detalhamento em
[backlog/e1-core-saas.md](../backlog/e1-core-saas.md) e sequência no
[roadmap](../roadmap.md).

1. **Decidir a stack** ([ADR-0002](../adrs/0002-escolha-de-stack.md)) — bloqueia tudo.
2. **Esqueleto executável**: projeto compila, sobe, responde `/health`, tem
   teste rodando em comando único e formatador aplicado. Nada de negócio ainda.
3. **CI**: em todo push roda formatador, análise estática, testes. Vermelho barra merge.
4. **Migrações versionadas** e ambiente local reprodutível.
5. **Tenant e Usuário** com o isolamento já ativo — não se adiciona
   multi-tenancy a um schema pronto sem dor.
6. **Autenticação** (sessão, e-mail/senha, Google).
7. **Autorização** por papel, com teste negativo obrigatório.
8. **Fila de jobs e agendador** — Dunning e lembretes dependem, e a forma como
   o job carrega o tenant precisa estar certa desde o primeiro job.
9. **Observabilidade**: log JSON estruturado com `tenant_id` e `request_id`,
   e um erro tratado ponta a ponta.

**Critério de saída da fundação:** é possível criar dois tenants, autenticar um
usuário em cada, e provar por teste automatizado que nenhum enxerga o dado do
outro — em requisição HTTP **e** em job de background.

## Princípios que a fundação materializa

Derivam do [CLAUDE.md](../../CLAUDE.md), a fonte canônica:

- **Dependência injetada.** Nada de import de SDK no ponto de uso. Se um teste
  precisa de rede para rodar, o desenho está errado.
- **Terceiro atrás de interface fina.** O domínio conhece `EnviadorDeMensagem`,
  não `MetaWhatsAppClient`. Trocar de provedor deve tocar um arquivo.
- **Tipos explícitos.** Sem `any`, sem dicionário genérico como contrato.
- **Segredo só em variável de ambiente**, documentada em `.env.example`.
- **Log JSON estruturado**, sem segredo e sem dado pessoal.
