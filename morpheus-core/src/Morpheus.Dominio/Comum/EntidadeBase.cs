namespace Morpheus.Dominio.Comum;

/// <summary>
/// Raiz de toda entidade de domínio. Concentra o que é comum a todas — identidade
/// única, trilha de <see cref="DadosDeAuditoria"/> e o acúmulo de
/// <see cref="IEventoDeDominio"/> — para que nenhuma entidade reimplemente essa
/// mecânica. A subclasse define os invariantes do seu agregado e registra eventos
/// nas ações de escrita; a base só guarda e entrega.
/// </summary>
public abstract class EntidadeBase : IPossuiEventosDeDominio
{
    private readonly List<IEventoDeDominio> _eventosDeDominio = [];

    public Guid Id { get; protected set; }

    // EF materializa a auditoria pelo mapeamento owned; o domínio sempre a define
    // via factory. O null! evita o falso positivo de nulabilidade no ctor do EF.
    public DadosDeAuditoria Auditoria { get; protected set; } = null!;

    public IReadOnlyCollection<IEventoDeDominio> EventosDeDominio => _eventosDeDominio;

    /// <summary>
    /// Registra um fato já ocorrido para o outbox drenar na próxima escrita.
    /// Protegido: só o próprio agregado decide o que virou evento.
    /// </summary>
    protected void RegistrarEvento(IEventoDeDominio evento) => _eventosDeDominio.Add(evento);

    public void LimparEventos() => _eventosDeDominio.Clear();
}
