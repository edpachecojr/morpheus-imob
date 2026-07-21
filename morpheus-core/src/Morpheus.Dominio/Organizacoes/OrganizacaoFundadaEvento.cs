using Morpheus.Dominio.Comum;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Fato de que uma organização (o tenant) nasceu. É o gancho de boas-vindas e
/// futuro upsell: por isso carrega o nome, e não só o id — quando houver contato e
/// plano contratado (E1-F1-H2), este evento os transporta para uma comunicação
/// personalizada sem o consumidor precisar reconsultar a conta recém-criada.
/// </summary>
public sealed record OrganizacaoFundadaEvento(
    Guid OrganizacaoId,
    string Nome,
    DateTimeOffset OcorridoEm) : IEventoDeDominio;
