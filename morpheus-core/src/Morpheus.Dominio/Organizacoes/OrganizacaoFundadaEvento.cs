using Morpheus.Dominio.Comum;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Fato de que uma organização (o tenant) nasceu. É o gancho de boas-vindas e
/// futuro upsell: por isso carrega o nome e o fuso, e não só o id — quem envia a
/// mensagem de boas-vindas precisa saber em que horário local escrever, sem
/// reconsultar a conta recém-criada. Plano contratado entra com a assinatura (E1-F5).
/// </summary>
public sealed record OrganizacaoFundadaEvento(
    Guid OrganizacaoId,
    string Nome,
    string FusoHorario,
    DateTimeOffset OcorridoEm) : IEventoDeDominio;
