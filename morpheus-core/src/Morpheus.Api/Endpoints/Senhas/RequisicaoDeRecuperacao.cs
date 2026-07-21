namespace Morpheus.Api.Endpoints.Senhas;

/// <summary>Pedido de link de redefinição. Responde igual exista ou não a conta.</summary>
public sealed record RequisicaoDeRecuperacao(string Email);
