namespace Morpheus.Aplicacao.Contas;

/// <summary>
/// Dados de um usuário a criar, já validados e com a organização resolvida. É o
/// contrato entre o caso de uso de cadastro e o store de identidade.
/// </summary>
public sealed record NovoUsuarioDoPainel(
    Guid OrganizacaoId,
    string NomeCompleto,
    string Email,
    string Senha,
    string Papel);
