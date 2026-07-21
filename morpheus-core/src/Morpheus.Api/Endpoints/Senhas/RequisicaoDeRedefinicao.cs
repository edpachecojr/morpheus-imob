namespace Morpheus.Api.Endpoints.Senhas;

/// <summary>Troca de senha com o token recebido por e-mail, mais a confirmação digitada.</summary>
public sealed record RequisicaoDeRedefinicao(
    string Email, string Token, string NovaSenha, string ConfirmacaoDeSenha);
