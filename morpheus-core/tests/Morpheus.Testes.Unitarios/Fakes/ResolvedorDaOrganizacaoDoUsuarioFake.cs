using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Resolvedor falso: devolve uma organização fixa para qualquer usuário e conta
/// as resoluções. Isola o teste do contexto da lógica de cache/banco.
/// </summary>
public sealed class ResolvedorDaOrganizacaoDoUsuarioFake : IResolvedorDaOrganizacaoDoUsuario
{
    private readonly Guid _organizacaoId;

    public int QuantidadeDeResolucoes { get; private set; }

    public ResolvedorDaOrganizacaoDoUsuarioFake(Guid organizacaoId) => _organizacaoId = organizacaoId;

    public Task<Guid> ResolverAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        QuantidadeDeResolucoes++;
        return Task.FromResult(_organizacaoId);
    }
}
