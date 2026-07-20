using Morpheus.Dominio.Erros;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Escrita cujo vínculo de organização diverge da organização do contexto
/// autenticado. É a barreira que impede um tenant de gravar no dado de outro.
/// </summary>
public sealed class ErroDeOrganizacaoDivergente : ErroDeRegraDeNegocio
{
    public Guid OrganizacaoDaEntidade { get; }
    public Guid OrganizacaoDoContexto { get; }

    public ErroDeOrganizacaoDivergente(Guid organizacaoDaEntidade, Guid organizacaoDoContexto)
        : base($"Escrita rejeitada: entidade da organização {organizacaoDaEntidade} não " +
               $"pertence à organização do contexto {organizacaoDoContexto}.")
    {
        OrganizacaoDaEntidade = organizacaoDaEntidade;
        OrganizacaoDoContexto = organizacaoDoContexto;
    }
}
