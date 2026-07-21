using Morpheus.Dominio.Comum;

namespace Morpheus.Infraestrutura.Persistencia.Outbox;

/// <summary>
/// Um evento de domínio gravado de forma durável na mesma transação da escrita que
/// o originou — o lado de escrita do Outbox Pattern. Garante que "o dado mudou" e
/// "o evento existe" caem ou sobem juntos, nunca um sem o outro. O envelope carrega
/// o tenant (<see cref="OrganizacaoId"/>), o tipo e o instante; o
/// <see cref="Conteudo"/> traz os dados de negócio completos.
/// <para>
/// A drenagem (dispatcher, filas, pub/sub) está fora de escopo neste MVP:
/// <see cref="ProcessadoEm"/> nasce nulo e existe só para o futuro consumidor
/// marcar o que já publicou, sem exigir migração depois ([ADR-0009]).
/// </para>
/// </summary>
public sealed class MensagemDeOutbox
{
    public Guid Id { get; private set; }
    public Guid OrganizacaoId { get; private set; }
    public string TipoDoEvento { get; private set; }
    public string Conteudo { get; private set; }
    public DateTimeOffset OcorridoEm { get; private set; }
    public DateTimeOffset? ProcessadoEm { get; private set; }

    private MensagemDeOutbox(
        Guid id, Guid organizacaoId, string tipoDoEvento, string conteudo, DateTimeOffset ocorridoEm)
    {
        Id = id;
        OrganizacaoId = organizacaoId;
        TipoDoEvento = tipoDoEvento;
        Conteudo = conteudo;
        OcorridoEm = ocorridoEm;
    }

    // Exigido pelo materializador do EF Core; nunca usado pelo domínio.
    private MensagemDeOutbox()
    {
        TipoDoEvento = string.Empty;
        Conteudo = string.Empty;
    }

    /// <summary>
    /// Cria a mensagem a partir de um evento e do tenant já resolvido. Falha alto
    /// se o tenant vier vazio: outbox sem organização é evento órfão, que nenhum
    /// consumidor saberia rotear.
    /// </summary>
    public static MensagemDeOutbox Registrar(
        Guid organizacaoId, IEventoDeDominio evento, ISerializadorDeEvento serializador)
    {
        if (organizacaoId == Guid.Empty)
            throw new ArgumentException(
                $"Evento '{evento.GetType().Name}' sem organização não pode ir ao outbox; " +
                "esperado um OrganizacaoId não vazio.", nameof(organizacaoId));

        return new MensagemDeOutbox(
            Guid.NewGuid(),
            organizacaoId,
            serializador.TipoDe(evento),
            serializador.Serializar(evento),
            evento.OcorridoEm);
    }
}
