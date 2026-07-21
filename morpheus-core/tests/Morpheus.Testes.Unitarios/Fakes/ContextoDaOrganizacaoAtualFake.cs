using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Contexto de organização falso, dirigido pelo teste: simula requisição com uma
/// organização resolvida ou sem sessão, sem depender de banco nem de HttpContext.
/// </summary>
public sealed class ContextoDaOrganizacaoAtualFake : IContextoDaOrganizacaoAtual
{
    private readonly Guid? _organizacaoId;

    private ContextoDaOrganizacaoAtualFake(Guid? organizacaoId) => _organizacaoId = organizacaoId;

    public static ContextoDaOrganizacaoAtualFake Com(Guid organizacaoId) => new(organizacaoId);

    public static ContextoDaOrganizacaoAtualFake SemSessao() => new(null);

    public Task<Guid> ObterOrganizacaoIdAsync(CancellationToken cancelamento) =>
        Task.FromResult(_organizacaoId ?? throw new InvalidOperationException("Sem organização no contexto."));

    public Task<Guid?> ObterOrganizacaoIdOuNuloAsync(CancellationToken cancelamento) =>
        Task.FromResult(_organizacaoId);
}
