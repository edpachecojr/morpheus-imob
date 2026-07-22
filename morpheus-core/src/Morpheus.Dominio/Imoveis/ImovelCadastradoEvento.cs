using Morpheus.Dominio.Comum;

namespace Morpheus.Dominio.Imoveis;

/// <summary>
/// Fato de que um imóvel entrou no catálogo de uma organização. Carrega os dados
/// de negócio completos do imóvel — não só o id — para que um consumidor (ex.:
/// indexação de busca, notificação ao corretor) processe sem reconsultar a origem.
/// O tenant não é campo do evento: é metadado de envelope, gravado pelo outbox.
/// </summary>
public sealed record ImovelCadastradoEvento(
    Guid ImovelId,
    string CodigoDeReferencia,
    string Titulo,
    FinalidadeDoImovel Finalidade,
    SituacaoDoImovel Situacao,
    string Endereco,
    DateTimeOffset OcorridoEm) : IEventoDeDominio;
