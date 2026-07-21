using Morpheus.Dominio.Comum;

namespace Morpheus.Infraestrutura.Persistencia.Outbox;

/// <summary>
/// Serializa um <see cref="IEventoDeDominio"/> para o texto gravado no outbox.
/// Interface fina de propriedade nossa: o montador de mensagens depende dela, não
/// do serializador concreto, o que o torna testável sem escolher formato de fio.
/// </summary>
public interface ISerializadorDeEvento
{
    /// <summary>Nome estável do tipo do evento, gravado como discriminador no outbox.</summary>
    string TipoDe(IEventoDeDominio evento);

    /// <summary>Payload do evento com os dados de negócio completos, pronto para persistir.</summary>
    string Serializar(IEventoDeDominio evento);
}
