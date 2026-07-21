using Morpheus.Dominio.Comum;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Portadora de eventos que não é entidade de organização nem a própria
/// <c>Organizacao</c> — o caso que o montador do outbox não sabe rotear. Existe só
/// para provar que uma portadora sem tenant resolvível é recusada, cenário que as
/// entidades reais tornam impossível (o tenant é obrigatório na construção).
/// </summary>
public sealed class PortadoraDeEventoSemTenant : IPossuiEventosDeDominio
{
    private readonly List<IEventoDeDominio> _eventos;

    public PortadoraDeEventoSemTenant(IEventoDeDominio evento) => _eventos = [evento];

    public IReadOnlyCollection<IEventoDeDominio> EventosDeDominio => _eventos;

    public void LimparEventos() => _eventos.Clear();
}
