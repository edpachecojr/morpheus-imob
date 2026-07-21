namespace Morpheus.Api.Autorizacao;

/// <summary>
/// Marca que uma rota declarou explicitamente o acesso que exige. Existe para que
/// a verificação de subida distinga "rota protegida" de "rota que ninguém
/// classificou" — a segunda é a que abre buraco, e é a que derruba o processo.
/// </summary>
public interface IDeclaracaoDeAcesso;
