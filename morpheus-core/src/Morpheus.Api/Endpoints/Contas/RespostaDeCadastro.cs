namespace Morpheus.Api.Endpoints.Contas;

/// <summary>Resposta única do cadastro, idêntica para conta criada, e-mail repetido e robô.</summary>
public sealed record RespostaDeCadastro(string Mensagem);
