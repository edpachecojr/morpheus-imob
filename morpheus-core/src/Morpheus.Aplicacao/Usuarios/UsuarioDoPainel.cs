namespace Morpheus.Aplicacao.Usuarios;

/// <summary>
/// Retrato mínimo de um usuário do painel para os casos de uso de autenticação:
/// quem é, como se chama e se está bloqueado. Não expõe hash de senha, security
/// stamp nem qualquer campo do store do Identity — o caso de uso não precisa
/// deles e o que não é exposto não vaza em log nem em resposta.
/// </summary>
public sealed record UsuarioDoPainel(Guid Id, string Email, string NomeCompleto, bool Bloqueado);
