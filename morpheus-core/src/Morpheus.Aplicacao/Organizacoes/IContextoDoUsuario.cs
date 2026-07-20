namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Fornece a identidade autenticada da execução corrente. Implementado sobre a
/// sessão HTTP no painel e sobre o payload em jobs de background — os quatro
/// caminhos de entrada convergem para este contrato.
/// </summary>
public interface IContextoDoUsuario
{
    /// <summary>Id do usuário autenticado, ou <c>null</c> quando não há sessão.</summary>
    Guid? UsuarioAutenticadoId { get; }
}
