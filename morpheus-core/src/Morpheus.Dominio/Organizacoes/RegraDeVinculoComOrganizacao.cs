namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Núcleo do isolamento de escrita: dada uma entidade e a organização do
/// contexto, carimba o vínculo ausente e rejeita o divergente. É uma regra pura
/// (todos os insumos são parâmetros) para ser testada sem banco e reusada tanto
/// pelo interceptor de persistência quanto pelas próprias entidades.
/// </summary>
public static class RegraDeVinculoComOrganizacao
{
    /// <summary>
    /// Calcula o vínculo resultante de uma atribuição imutável.
    /// Exemplo: <c>OrganizacaoId = RegraDeVinculoComOrganizacao.AtribuirImutavel(OrganizacaoId, org);</c>.
    /// </summary>
    public static Guid AtribuirImutavel(Guid vinculoAtual, Guid novoVinculo)
    {
        if (novoVinculo == Guid.Empty)
            throw new ErroDeOrganizacaoObrigatoria("AtribuirImutavel");
        if (vinculoAtual == novoVinculo)
            return vinculoAtual;
        if (vinculoAtual != Guid.Empty)
            throw new ErroDeVinculoDeOrganizacaoImutavel(vinculoAtual, novoVinculo);
        return novoVinculo;
    }

    /// <summary>
    /// Garante que a entidade pertence à organização do contexto, carimbando-a
    /// quando ainda sem vínculo. Rejeita contexto vazio e vínculo divergente.
    /// </summary>
    public static void GarantirVinculo(IPertenceOrganizacao entidade, Guid organizacaoDoContexto)
    {
        if (organizacaoDoContexto == Guid.Empty)
            throw new ErroDeOrganizacaoObrigatoria("GarantirVinculo");
        if (entidade.OrganizacaoId == organizacaoDoContexto)
            return;
        if (entidade.OrganizacaoId != Guid.Empty)
            throw new ErroDeOrganizacaoDivergente(entidade.OrganizacaoId, organizacaoDoContexto);
        entidade.AtribuirOrganizacao(organizacaoDoContexto);
    }

    /// <summary>
    /// Variante para o caminho sem usuário logado (job/bootstrap): sem contexto,
    /// exige que a entidade já traga o vínculo explícito — nunca grava "global".
    /// </summary>
    public static void GarantirVinculoComContextoOpcional(IPertenceOrganizacao entidade, Guid? organizacaoDoContexto)
    {
        if (organizacaoDoContexto is Guid contexto)
        {
            GarantirVinculo(entidade, contexto);
            return;
        }

        if (entidade.OrganizacaoId == Guid.Empty)
            throw new ErroDeEscritaSemOrganizacao(entidade.GetType().Name);
    }
}
