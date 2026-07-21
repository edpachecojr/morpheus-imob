using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Ponto único que traduz a identidade autenticada na organização do contexto.
/// Sem usuário, a versão estrita falha (default seguro) e a versão opcional
/// devolve <c>null</c> para quem enriquece sem exigir sessão (ex.: escopo de log).
/// </summary>
public sealed class ContextoDaOrganizacaoAtual : IContextoDaOrganizacaoAtual
{
    private readonly IContextoDoUsuario _contextoDoUsuario;
    private readonly IResolvedorDaOrganizacaoDoUsuario _resolvedor;

    public ContextoDaOrganizacaoAtual(
        IContextoDoUsuario contextoDoUsuario,
        IResolvedorDaOrganizacaoDoUsuario resolvedor)
    {
        _contextoDoUsuario = contextoDoUsuario;
        _resolvedor = resolvedor;
    }

    public Task<Guid> ObterOrganizacaoIdAsync(CancellationToken cancelamento)
    {
        var usuarioId = _contextoDoUsuario.UsuarioAutenticadoId
            ?? throw new ErroDeUsuarioNaoAutenticado();
        return _resolvedor.ResolverAsync(usuarioId, cancelamento);
    }

    public async Task<Guid?> ObterOrganizacaoIdOuNuloAsync(CancellationToken cancelamento)
    {
        var usuarioId = _contextoDoUsuario.UsuarioAutenticadoId;
        if (usuarioId is not Guid id)
            return null;
        return await _resolvedor.ResolverAsync(id, cancelamento);
    }
}
