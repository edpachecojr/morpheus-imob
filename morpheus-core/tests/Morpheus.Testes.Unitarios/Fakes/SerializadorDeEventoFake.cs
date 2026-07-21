using Morpheus.Dominio.Comum;
using Morpheus.Infraestrutura.Persistencia.Outbox;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Serializador de eventos determinístico para os testes do outbox: devolve o nome
/// do tipo e um payload previsível, sem depender do formato real de JSON. Mantém o
/// teste do montador focado na drenagem, não na serialização (F.I.R.S.T.).
/// </summary>
public sealed class SerializadorDeEventoFake : ISerializadorDeEvento
{
    public string TipoDe(IEventoDeDominio evento) => evento.GetType().Name;

    public string Serializar(IEventoDeDominio evento) => $"{{\"tipo\":\"{evento.GetType().Name}\"}}";
}
