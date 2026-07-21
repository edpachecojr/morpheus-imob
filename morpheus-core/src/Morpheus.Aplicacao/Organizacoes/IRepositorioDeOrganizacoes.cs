using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Escrita de organizações. Minúscula de propósito: a organização é a raiz do
/// isolamento e só nasce no cadastro de conta — não há listagem "de todas as
/// organizações", que seria justamente a consulta que atravessa tenants.
/// </summary>
public interface IRepositorioDeOrganizacoes
{
    Task AdicionarAsync(Organizacao organizacao, CancellationToken cancelamento);
}
