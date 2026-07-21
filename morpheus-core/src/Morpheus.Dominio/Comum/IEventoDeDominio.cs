namespace Morpheus.Dominio.Comum;

/// <summary>
/// Fato de negócio já ocorrido, relevante além da própria entidade que o gerou —
/// "imóvel cadastrado", "conta criada", "contrato assinado". Nasce de uma ação de
/// escrita e é a unidade que a resiliência (outbox) grava para consumo posterior.
/// <para>
/// Contrato mínimo intencional: cada evento carrega seu próprio instante de
/// ocorrência. Os <b>dados de negócio completos</b> (nome, e-mail, plano...) são
/// campos do evento concreto — nunca só um id —, para que um consumidor distribuído
/// processe a mensagem sem precisar reconsultar a origem. O tenant é metadado de
/// envelope, resolvido na gravação do outbox, não campo do evento.
/// </para>
/// </summary>
public interface IEventoDeDominio
{
    /// <summary>Instante em que o fato ocorreu, cravado por relógio injetado na origem.</summary>
    DateTimeOffset OcorridoEm { get; }
}
