using Morpheus.Dominio.Comum;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Base das entidades de negócio que pertencem a uma organização. Soma ao que a
/// <see cref="EntidadeBase"/> já dá (identidade, auditoria, eventos) o vínculo de
/// tenant, recebido na construção e imutável depois — sem setter, o vínculo não
/// migra de organização por engano. Entidades presas a SDKs de terceiros (ex.:
/// usuário do Identity, que não controla o próprio construtor) implementam
/// <see cref="IPertenceOrganizacao"/> diretamente.
/// </summary>
public abstract class EntidadeDaOrganizacao : EntidadeBase, IPertenceOrganizacao
{
    public Guid OrganizacaoId { get; private set; }

    protected EntidadeDaOrganizacao(OrganizacaoDona organizacao)
        => OrganizacaoId = organizacao.Valor;

    // Exigido pelo materializador do EF Core; nunca usado pelo domínio. O EF grava
    // OrganizacaoId direto na coluna ao rehidratar.
    protected EntidadeDaOrganizacao()
    {
    }
}
