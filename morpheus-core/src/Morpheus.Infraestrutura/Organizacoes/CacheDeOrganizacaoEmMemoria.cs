using Microsoft.Extensions.Caching.Memory;
using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Infraestrutura.Organizacoes;

/// <summary>
/// Cache em memória do vínculo usuário → organização, com expiração deslizante.
/// Implementa a interface fina do projeto: trocar por um cache distribuído
/// (Redis) não toca o domínio nem a aplicação.
/// </summary>
public sealed class CacheDeOrganizacaoEmMemoria : ICacheDeOrganizacaoDoUsuario
{
    private static readonly TimeSpan ExpiracaoDeslizante = TimeSpan.FromMinutes(30);
    private readonly IMemoryCache _cache;

    public CacheDeOrganizacaoEmMemoria(IMemoryCache cache) => _cache = cache;

    public Task<Guid?> ObterAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        var encontrado = _cache.TryGetValue(Chave(usuarioId), out Guid organizacaoId);
        return Task.FromResult(encontrado ? organizacaoId : (Guid?)null);
    }

    public Task ArmazenarAsync(Guid usuarioId, Guid organizacaoId, CancellationToken cancelamento)
    {
        _cache.Set(Chave(usuarioId), organizacaoId, ExpiracaoDeslizante);
        return Task.CompletedTask;
    }

    private static string Chave(Guid usuarioId) => $"organizacao-do-usuario:{usuarioId}";
}
