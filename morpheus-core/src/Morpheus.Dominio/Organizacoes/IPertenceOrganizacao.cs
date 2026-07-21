namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Contrato de toda entidade que pertence a uma <see cref="Organizacao"/>. O
/// vínculo é definido na construção (recebe um <see cref="OrganizacaoDona"/>) e é
/// imutável — não há operação de revínculo. Repositórios de leitura e a montagem
/// do outbox usam este contrato para impor o isolamento entre organizações sem
/// depender da disciplina de quem escreve a consulta.
/// </summary>
public interface IPertenceOrganizacao
{
    Guid OrganizacaoId { get; }
}
