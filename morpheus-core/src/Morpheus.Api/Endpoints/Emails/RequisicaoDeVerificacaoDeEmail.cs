namespace Morpheus.Api.Endpoints.Emails;

/// <summary>Corpo do pedido de confirmação de e-mail (E1-F2-H6): o token do link recebido.</summary>
public sealed record RequisicaoDeVerificacaoDeEmail(string Email, string Token);
