using Morpheus.Dominio.Comum;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Fato de que o dono completou o onboarding renomeando a organização
/// (E1-F1-H4). Carrega o nome antigo e o novo, e quem fez a mudança — a trilha
/// de auditoria que <see cref="DadosDeAuditoria"/> não guarda (só registra
/// quando, não quem).
/// </summary>
public sealed record OrganizacaoRenomeadaEvento(
    Guid OrganizacaoId,
    string NomeAntigo,
    string NomeNovo,
    Guid AutorId,
    DateTimeOffset OcorridoEm) : IEventoDeDominio;
