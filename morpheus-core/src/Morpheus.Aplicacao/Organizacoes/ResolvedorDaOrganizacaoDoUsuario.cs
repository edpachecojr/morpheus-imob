using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Resolve a organização de um usuário consultando o cache antes do banco. É a
/// primeira estratégia de cache do sistema: o vínculo usuário → organização é
/// lido em toda requisição e muda raramente, então não vale um ida-e-volta ao
/// banco por chamada.
/// </summary>
public sealed class ResolvedorDaOrganizacaoDoUsuario : IResolvedorDaOrganizacaoDoUsuario
{
    private readonly ICacheDeOrganizacaoDoUsuario _cache;
    private readonly IConsultaDaOrganizacaoDoUsuario _consulta;

    public ResolvedorDaOrganizacaoDoUsuario(
        ICacheDeOrganizacaoDoUsuario cache,
        IConsultaDaOrganizacaoDoUsuario consulta)
    {
        _cache = cache;
        _consulta = consulta;
    }

    public async Task<Guid> ResolverAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        if (usuarioId == Guid.Empty)
            throw new ErroDeUsuarioObrigatorio();

        var emCache = await _cache.ObterAsync(usuarioId, cancelamento);
        if (emCache is Guid organizacaoEmCache)
            return organizacaoEmCache;

        var organizacaoId = await _consulta.BuscarOrganizacaoIdAsync(usuarioId, cancelamento)
            ?? throw new ErroDeOrganizacaoDoUsuarioNaoEncontrada(usuarioId);

        await _cache.ArmazenarAsync(usuarioId, organizacaoId, cancelamento);
        return organizacaoId;
    }
}
