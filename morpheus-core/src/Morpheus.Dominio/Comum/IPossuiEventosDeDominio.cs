namespace Morpheus.Dominio.Comum;

/// <summary>
/// Contrato de tudo que acumula <see cref="IEventoDeDominio"/> para o outbox drenar
/// na persistência. <see cref="EntidadeBase"/> o implementa para as entidades de
/// domínio; entidades presas a SDKs de terceiros (ex.: usuário do Identity) podem
/// implementá-lo direto, reusando a mesma mecânica. A coleta de eventos na escrita
/// depende deste contrato, não da hierarquia concreta.
/// </summary>
public interface IPossuiEventosDeDominio
{
    /// <summary>Eventos pendentes de drenagem, na ordem em que foram registrados.</summary>
    IReadOnlyCollection<IEventoDeDominio> EventosDeDominio { get; }

    /// <summary>Descarta os eventos já drenados, para não os gravar duas vezes.</summary>
    void LimparEventos();
}
