namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Resolve o id da organização de um usuário, preferindo o cache ao banco.
/// Separado de <see cref="IContextoDaOrganizacaoAtual"/> para ser testável a
/// partir de um id de usuário puro, sem depender de sessão.
/// </summary>
public interface IResolvedorDaOrganizacaoDoUsuario
{
    /// <summary>
    /// Id da organização do usuário, do cache ou, na ausência, do banco.
    /// Exemplo: <c>await resolvedor.ResolverAsync(usuarioId, cancelamento)</c>.
    /// </summary>
    Task<Guid> ResolverAsync(Guid usuarioId, CancellationToken cancelamento);
}
