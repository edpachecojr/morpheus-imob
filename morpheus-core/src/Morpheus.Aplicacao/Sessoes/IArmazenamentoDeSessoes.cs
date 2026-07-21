namespace Morpheus.Aplicacao.Sessoes;

/// <summary>
/// Guarda as sessões do painel no servidor. É o que torna o cookie um
/// identificador opaco em vez de um portador de identidade: sem a linha
/// correspondente, o cookie não vale nada, e revogar é apagar a linha (ADR-0011).
/// </summary>
public interface IArmazenamentoDeSessoes
{
    Task GuardarAsync(SessaoPersistida sessao, CancellationToken cancelamento);

    /// <summary>Substitui o conteúdo de uma sessão existente (renovação de validade).</summary>
    Task RenovarAsync(SessaoPersistida sessao, CancellationToken cancelamento);

    /// <summary>Sessão válida do id informado, ou <c>null</c> se inexistente ou expirada.</summary>
    Task<SessaoPersistida?> BuscarAsync(Guid sessaoId, CancellationToken cancelamento);

    /// <summary>Revoga uma sessão — o aparelho que fez logout.</summary>
    Task RemoverAsync(Guid sessaoId, CancellationToken cancelamento);

    /// <summary>Revoga todas as sessões do usuário — troca de senha, acesso comprometido.</summary>
    Task RemoverDoUsuarioAsync(Guid usuarioId, CancellationToken cancelamento);
}
