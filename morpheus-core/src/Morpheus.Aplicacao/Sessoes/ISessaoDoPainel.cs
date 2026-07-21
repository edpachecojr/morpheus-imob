namespace Morpheus.Aplicacao.Sessoes;

/// <summary>
/// Ciclo de vida da sessão do painel. A sessão é opaca e vive no servidor
/// (ADR-0011): o cookie carrega só um identificador, então revogar é apagar a
/// linha — e a requisição seguinte não tem o que restaurar.
/// </summary>
public interface ISessaoDoPainel
{
    /// <summary>Abre a sessão do usuário e emite o cookie de sessão da resposta corrente.</summary>
    Task AbrirAsync(Guid usuarioId, CancellationToken cancelamento);

    /// <summary>Encerra a sessão da requisição corrente — só este aparelho.</summary>
    Task EncerrarAsync(CancellationToken cancelamento);

    /// <summary>
    /// Revoga <b>todas</b> as sessões do usuário. É o que faz a troca de senha
    /// derrubar os outros aparelhos, como manda [autenticacao.md](../../../../docs/fundacao/autenticacao.md).
    /// </summary>
    Task EncerrarTodasDoUsuarioAsync(Guid usuarioId, CancellationToken cancelamento);
}
