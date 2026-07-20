namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Cache do vínculo usuário → organização. Interface fina de propriedade do
/// projeto sobre o mecanismo concreto (memória hoje, distribuído amanhã) — o
/// domínio e a aplicação não conhecem o SDK de cache.
/// </summary>
public interface ICacheDeOrganizacaoDoUsuario
{
    Task<Guid?> ObterAsync(Guid usuarioId, CancellationToken cancelamento);

    Task ArmazenarAsync(Guid usuarioId, Guid organizacaoId, CancellationToken cancelamento);
}
