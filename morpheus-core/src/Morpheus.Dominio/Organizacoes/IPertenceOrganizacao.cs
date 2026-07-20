namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Contrato de toda entidade que pertence a uma <see cref="Organizacao"/>. O
/// vínculo é imutável depois de definido. Repositórios de leitura e o interceptor
/// de escrita usam este contrato para impor o isolamento entre organizações sem
/// depender da disciplina de quem escreve a consulta.
/// </summary>
public interface IPertenceOrganizacao
{
    Guid OrganizacaoId { get; }

    /// <summary>
    /// Vincula a entidade a uma organização. Idempotente para o mesmo id;
    /// rejeita a tentativa de revincular a outra organização.
    /// Exemplo: <c>imovel.AtribuirOrganizacao(contexto.OrganizacaoId)</c>.
    /// </summary>
    void AtribuirOrganizacao(Guid organizacaoId);
}
