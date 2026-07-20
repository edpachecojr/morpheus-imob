# Wiki do morpheus-imob

Documentação viva do projeto. Se o código divergir do que está aqui, **um dos
dois está errado** — corrija o que estiver desatualizado no mesmo PR.

## Por onde começar

| Se você quer... | Leia |
| --- | --- |
| Entender o produto e o problema | [PRD](prd.md) |
| Falar a mesma língua do negócio | [Glossário](dominio/glossario.md) |
| Entender as entidades e seus ciclos de vida | [Modelo de domínio](dominio/modelo-de-dominio.md) |
| Saber o que vem primeiro | [Roadmap](roadmap.md) |
| Pegar trabalho para fazer | [Backlog](backlog/README.md) |
| Saber por que algo foi decidido assim | [ADRs](adrs/README.md) |
| Entender as bases técnicas | [Fundação](fundacao/README.md) |

## Estrutura

```
docs/
├── README.md                  ← você está aqui
├── prd.md                     produto: problema, objetivos, escopo, KPIs
├── roadmap.md                 MVP vs. visão completa, fases e marcos
├── dominio/
│   ├── glossario.md           linguagem ubíqua
│   └── modelo-de-dominio.md   entidades, agregados, máquinas de estado
├── fundacao/
│   ├── README.md              índice das bases técnicas
│   ├── stack.md               estado da escolha de stack
│   ├── multi-tenancy.md       modelo de isolamento de dados
│   ├── autenticacao.md        identidade: quem é você
│   ├── autorizacao.md         permissões: o que você pode fazer
│   └── ambientes-e-segredos.md configuração, .env, deploy
├── adrs/                      decisões arquiteturais registradas
└── backlog/                   épicos → features → estórias de usuário
```

## Regras desta pasta

1. **Documento vivo.** Toda decisão que altera o comportamento do sistema
   atualiza a página correspondente aqui. Doc desatualizado é pior que doc
   ausente — ele mente com autoridade.
2. **Decisão arquitetural vira ADR.** Não enterre decisão em comentário de PR.
   Veja [adrs/README.md](adrs/README.md).
3. **Uma responsabilidade por página.** Página acima de ~300 linhas se divide,
   pelo mesmo motivo que um arquivo de código se divide.
4. **Sem dado real de cliente.** O repositório é público. Exemplos usam dados
   fictícios. Veja [ambientes-e-segredos](fundacao/ambientes-e-segredos.md).
5. **Escreva o porquê.** O código mostra o quê; a wiki existe para o resto.

## Estado atual do projeto

| Dimensão | Estado |
| --- | --- |
| Fase | Fundação — documentação e definição de arquitetura |
| Stack | **Não decidida** — ver [ADR-0002](adrs/0002-escolha-de-stack.md) |
| Código de produção | Nenhum ainda |
| Última revisão desta wiki | 2026-07-20 |
