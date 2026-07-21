namespace Morpheus.Api.Endpoints.Sessoes;

/// <summary>Credenciais do login por e-mail e senha. Sem campo de tenant, de papel ou de permissão.</summary>
public sealed record RequisicaoDeLogin(string Email, string Senha);
