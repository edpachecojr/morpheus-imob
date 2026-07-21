using System.Text.Json;
using Morpheus.Dominio.Comum;

namespace Morpheus.Infraestrutura.Persistencia.Outbox;

/// <summary>
/// Serializa o evento em JSON pelo seu tipo concreto — assim todos os campos do
/// evento (não os da interface) entram no payload, garantindo os dados completos
/// que um consumidor distribuído precisa. O tipo é gravado pelo nome curto da
/// classe; basta a um futuro dispatcher casar nome com o contrato.
/// </summary>
public sealed class SerializadorDeEventoComSystemTextJson : ISerializadorDeEvento
{
    private static readonly JsonSerializerOptions Opcoes = new(JsonSerializerDefaults.Web);

    public string TipoDe(IEventoDeDominio evento) => evento.GetType().Name;

    public string Serializar(IEventoDeDominio evento)
        => JsonSerializer.Serialize(evento, evento.GetType(), Opcoes);
}
