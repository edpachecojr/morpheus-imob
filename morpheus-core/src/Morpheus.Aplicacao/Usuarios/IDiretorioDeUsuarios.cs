using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Aplicacao.Usuarios;

/// <summary>
/// Consulta de usuários do painel. A busca por e-mail é o insumo do login e da
/// recuperação de senha; a listagem é sempre restrita à organização do contexto —
/// não existe consulta de usuário sem tenant, nem por parâmetro (ADR-0003).
/// </summary>
public interface IDiretorioDeUsuarios
{
    /// <summary>
    /// Usuário do e-mail informado, ou <c>null</c> se não houver. Nunca revele o
    /// resultado ao cliente.
    /// Exemplo: <c>await diretorio.BuscarPorEmailAsync(email, cancelamento)</c>.
    /// </summary>
    Task<UsuarioDoPainel?> BuscarPorEmailAsync(string email, CancellationToken cancelamento);

    /// <summary>
    /// Usuário do id informado, ou <c>null</c> se não houver. Serve o reenvio de
    /// confirmação de e-mail (E1-F2-H6), que só tem o id da sessão, não o e-mail.
    /// Exemplo: <c>await diretorio.BuscarPorIdAsync(usuarioId, cancelamento)</c>.
    /// </summary>
    Task<UsuarioDoPainel?> BuscarPorIdAsync(Guid id, CancellationToken cancelamento);

    /// <summary>
    /// Usuários da organização do contexto, resolvida por
    /// <see cref="IContextoDaOrganizacaoAtual"/> — nunca de parâmetro da requisição.
    /// Exemplo: <c>await diretorio.ListarDaOrganizacaoAsync(cancelamento)</c>.
    /// </summary>
    Task<IReadOnlyList<UsuarioDoPainel>> ListarDaOrganizacaoAsync(CancellationToken cancelamento);
}
