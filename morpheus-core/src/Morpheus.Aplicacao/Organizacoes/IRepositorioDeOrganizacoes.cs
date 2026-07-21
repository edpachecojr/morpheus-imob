using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Escrita de organizações. Minúscula de propósito: a organização é a raiz do
/// isolamento e só nasce no cadastro de conta — não há listagem "de todas as
/// organizações", que seria justamente a consulta que atravessa tenants.
/// </summary>
public interface IRepositorioDeOrganizacoes
{
    /// <summary>
    /// Persiste uma organização nova.
    /// Exemplo: <c>await repositorio.AdicionarAsync(organizacao, cancelamento)</c>.
    /// </summary>
    Task AdicionarAsync(Organizacao organizacao, CancellationToken cancelamento);

    /// <summary>
    /// A organização do id, ou <c>null</c> se não existir mais. Único ponto de
    /// leitura por id: o onboarding (E1-F1-H4) é o primeiro caso de uso que
    /// precisa reler a própria organização para alterá-la.
    /// Exemplo: <c>await repositorio.ObterPorIdAsync(organizacaoId, cancelamento)</c>.
    /// </summary>
    Task<Organizacao?> ObterPorIdAsync(Guid id, CancellationToken cancelamento);

    /// <summary>
    /// Persiste uma organização já existente e alterada em memória.
    /// Exemplo: <c>await repositorio.AtualizarAsync(organizacao, cancelamento)</c>.
    /// </summary>
    Task AtualizarAsync(Organizacao organizacao, CancellationToken cancelamento);
}
