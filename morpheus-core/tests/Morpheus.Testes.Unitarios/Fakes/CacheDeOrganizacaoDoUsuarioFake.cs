using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Cache falso em memória do vínculo usuário → organização. Conta armazenamentos
/// para os testes verificarem que o miss popula o cache uma vez.
/// </summary>
public sealed class CacheDeOrganizacaoDoUsuarioFake : ICacheDeOrganizacaoDoUsuario
{
    private readonly Dictionary<Guid, Guid> _armazenados = new();

    public int QuantidadeDeArmazenamentos { get; private set; }

    public Task<Guid?> ObterAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        var encontrado = _armazenados.TryGetValue(usuarioId, out var organizacaoId);
        return Task.FromResult(encontrado ? organizacaoId : (Guid?)null);
    }

    public Task ArmazenarAsync(Guid usuarioId, Guid organizacaoId, CancellationToken cancelamento)
    {
        _armazenados[usuarioId] = organizacaoId;
        QuantidadeDeArmazenamentos++;
        return Task.CompletedTask;
    }
}
