namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Cache do vínculo usuário → organização. Interface fina de propriedade do
/// projeto sobre o mecanismo concreto (memória hoje, distribuído amanhã) — o
/// domínio e a aplicação não conhecem o SDK de cache.
/// </summary>
public interface ICacheDeOrganizacaoDoUsuario
{
    /// <summary>
    /// Id da organização em cache para o usuário, ou <c>null</c> se não houver.
    /// Exemplo: <c>await cache.ObterAsync(usuarioId, cancelamento)</c>.
    /// </summary>
    Task<Guid?> ObterAsync(Guid usuarioId, CancellationToken cancelamento);

    /// <summary>
    /// Grava o vínculo usuário → organização em cache.
    /// Exemplo: <c>await cache.ArmazenarAsync(usuarioId, organizacaoId, cancelamento)</c>.
    /// </summary>
    Task ArmazenarAsync(Guid usuarioId, Guid organizacaoId, CancellationToken cancelamento);
}
