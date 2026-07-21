using Morpheus.Dominio.Comum;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Base das entidades de negócio que pertencem a uma organização. Soma ao que a
/// <see cref="EntidadeBase"/> já dá (identidade, auditoria, eventos) o vínculo
/// imutável de tenant, concentrando essa regra num só lugar. Entidades presas a
/// SDKs de terceiros (ex.: usuário do Identity) implementam
/// <see cref="IPertenceOrganizacao"/> diretamente reusando a mesma regra.
/// </summary>
public abstract class EntidadeDaOrganizacao : EntidadeBase, IPertenceOrganizacao
{
    public Guid OrganizacaoId { get; private set; }

    public void AtribuirOrganizacao(Guid organizacaoId)
        => OrganizacaoId = RegraDeVinculoComOrganizacao.AtribuirImutavel(OrganizacaoId, organizacaoId);
}
