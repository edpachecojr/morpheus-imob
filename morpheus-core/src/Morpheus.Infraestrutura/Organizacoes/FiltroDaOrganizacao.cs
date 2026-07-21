using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Infraestrutura.Organizacoes;

/// <summary>
/// Aplica o filtro explícito de organização a uma consulta EF. Toda leitura de
/// entidade da organização passa por aqui — o filtro é explícito (não é query
/// filter global de model creating), mas estrutural: o repositório não expõe
/// consulta sem ele, então esquecê-lo exige contornar deliberadamente.
/// </summary>
public static class FiltroDaOrganizacao
{
    public static IQueryable<T> DaOrganizacao<T>(this IQueryable<T> consulta, Guid organizacaoId)
        where T : IPertenceOrganizacao
    {
        if (organizacaoId == Guid.Empty)
            throw new ErroDeOrganizacaoObrigatoria("FiltroDaOrganizacao");
        return consulta.Where(entidade => entidade.OrganizacaoId == organizacaoId);
    }
}
