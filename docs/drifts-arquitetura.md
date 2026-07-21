# Drifts de arquitetura — revisão geral

Levantamento pontual do que foi implementado até `85622c3` frente às regras de
`CLAUDE.md`. Cada item: problema, arquivo, princípio ferido, resolução.

## 1. Pastas por tipo técnico, não por agregado — reclassificado, sem mudança

**Princípio:** "pastas por agregado (`Organizacoes`, `Imoveis`, `Usuarios`), não
por tipo técnico" (Estrutura de diretórios).

Nas três camadas fora do domínio, parte das pastas de topo agrupa por
preocupação técnica em vez de agregado de negócio:

- `morpheus-core/src/Morpheus.Api/{Autorizacao,Observabilidade,Seguranca,Identidade,Configuracao}/`
- `morpheus-core/src/Morpheus.Infraestrutura/{Identidade,Observabilidade,Persistencia}/`
- `morpheus-core/src/Morpheus.Aplicacao/{Autorizacao,Comum}/`

**Decisão (revisada com o autor do projeto):** mantida a estrutura atual. Cada
uma dessas pastas é um concern transversal coeso — uma única razão para mudar
— e várias delas existem justamente para envolver um SDK de terceiros
(Identity, EF Core, rate limiter do ASP.NET) atrás de interface fina, que é
outra regra explícita do `CLAUDE.md` ("Envolva bibliotecas de terceiros atrás
de uma interface fina"). Forçá-las para dentro de `Organizacoes`/`Imoveis`/
`Usuarios` espalharia rate-limiting, observabilidade e persistência por várias
pastas de negócio, ferindo SRP e DRY mais do que resolvendo. O próprio
`Morpheus.Dominio`, citado como referência de conformidade, já segue o mesmo
padrão com `Erros`, `Comum` e `Resultados` — pastas por capacidade, não só por
entidade de negócio. Não é mais tratado como drift.

## 2. Arquivo com dois tipos públicos — resolvido

**Princípio:** "Um arquivo por tipo público" (Estrutura de diretórios).

`CodificacaoDeHashArgon2id.cs` declarava `CodificacaoDeHashArgon2id` e
`HashArgon2idDecodificado` no mesmo arquivo. O record foi extraído para
`morpheus-core/src/Morpheus.Infraestrutura/Identidade/HashArgon2idDecodificado.cs`.

## 3. Funções acima de 20 linhas — resolvido

**Princípio:** "Funções: 4 a 20 linhas. Passou disso, divida." (Estilo de código).

Todas as funções apontadas foram divididas por sub-responsabilidade, mantendo
os comentários originais:

- `CadastroDeConta.ExecutarAsync` → extraídos `ValidarEntradaAsync` e
  `TratarCorridaDeEmail`.
- `ConfiguracaoDeInfraestrutura.AdicionarIdentidade` → extraído
  `AdicionarServicosDeIdentidade`.
- `ConfiguracaoDeOrganizacao.Configure` → extraídos `MapearConfiguracaoOperacional`
  e `MapearMetadados`.
- `ConfiguracaoDeImovel.Configure` → extraídos `MapearMetadados` e
  `MapearIndicesEChaves`.
- `ConfiguracaoDeUsuarioDaOrganizacao.Configure` → extraído
  `MapearVinculoComOrganizacao`.

## 4. Docstrings públicas sem exemplo de uso (ou ausentes) — resolvido

**Princípio:** "Docstrings em funções públicas: intenção + um exemplo de uso."
(Comentários).

Padrão sistêmico corrigido em todas as interfaces de `Morpheus.Aplicacao`,
`Morpheus.Infraestrutura` e `Morpheus.Dominio` que tinham métodos sem
docstring própria ou sem exemplo de chamada (`<c>...</c>`):

- `IRepositorioDeImoveis`, `IArmazenamentoDeSessoes`, `IDiretorioDeUsuarios`
  (achados originais).
- `ICacheDeOrganizacaoDoUsuario`, `IEnvioDeEmailDeRecuperacao`,
  `IRepositorioDeOrganizacoes`, `IConsultaDaOrganizacaoDoUsuario`,
  `IResolvedorDaOrganizacaoDoUsuario`, `IConsultaDeImoveisResumidos`,
  `IFabricaDeConexao`, `IPertenceOrganizacao` (achados na segunda varredura,
  mesmo padrão sistêmico não coberto na primeira passada).

## Segunda varredura

Sweep completa de `morpheus-core/src` após a correção dos itens 2–4: nenhuma
função nova acima de 20 linhas, nenhum arquivo com múltiplos tipos públicos,
nenhum arquivo autoral acima de 300 linhas, nenhum uso de `any`/dicionário
como contrato público, nenhuma nomenclatura genérica (`Manager`/`Helper`/
`Util`/`Handler`/`data`) e nenhuma indentação acima de 2 níveis. O único gap
remanescente foram as 8 interfaces do item 4, já corrigidas.
