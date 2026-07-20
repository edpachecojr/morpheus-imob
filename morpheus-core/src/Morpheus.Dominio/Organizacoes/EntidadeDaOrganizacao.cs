namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Base para entidades de negócio que pertencem a uma organização. Concentra a
/// lógica de vínculo imutável num só lugar para não duplicá-la em cada entidade.
/// Entidades presas a SDKs de terceiros (ex.: usuário do Identity) implementam
/// <see cref="IPertenceOrganizacao"/> diretamente reusando a mesma regra.
/// </summary>
public abstract class EntidadeDaOrganizacao : IPertenceOrganizacao
{
    public Guid OrganizacaoId { get; private set; }

    public void AtribuirOrganizacao(Guid organizacaoId)
        => OrganizacaoId = RegraDeVinculoComOrganizacao.AtribuirImutavel(OrganizacaoId, organizacaoId);
}
