namespace Morpheus.Api.Autorizacao;

/// <summary>
/// Metadado que marca a permissão que uma rota exige. É o que permite provar, na
/// subida, que nenhuma rota escapou sem declaração — a regra "negar por padrão"
/// vira verificação estrutural em vez de disciplina de quem escreve a rota
/// ([autorizacao.md](../../../../docs/fundacao/autorizacao.md), regra 1).
/// </summary>
public sealed record PermissaoExigida(string Permissao) : IDeclaracaoDeAcesso;
