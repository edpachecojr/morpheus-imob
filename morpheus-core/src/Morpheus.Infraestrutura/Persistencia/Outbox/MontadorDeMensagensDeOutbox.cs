using Morpheus.Dominio.Comum;
using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Infraestrutura.Persistencia.Outbox;

/// <summary>
/// Drena os eventos das entidades alteradas e os converte em
/// <see cref="MensagemDeOutbox"/>, resolvendo o tenant de cada uma. Lógica pura
/// (sem EF, sem banco): recebe as entidades, devolve as mensagens e limpa os
/// eventos drenados — o que a torna testável em milissegundos, sem Postgres.
/// </summary>
public sealed class MontadorDeMensagensDeOutbox
{
    private readonly ISerializadorDeEvento _serializador;

    public MontadorDeMensagensDeOutbox(ISerializadorDeEvento serializador)
        => _serializador = serializador;

    /// <summary>
    /// Constrói as mensagens de outbox das entidades com eventos pendentes e
    /// esvazia esses eventos, para não os gravar de novo na próxima escrita.
    /// </summary>
    public IReadOnlyList<MensagemDeOutbox> Drenar(IEnumerable<IPossuiEventosDeDominio> entidades)
    {
        var mensagens = new List<MensagemDeOutbox>();

        foreach (var entidade in entidades)
        {
            if (entidade.EventosDeDominio.Count == 0)
                continue;

            var organizacaoId = ResolverTenant(entidade);
            foreach (var evento in entidade.EventosDeDominio)
                mensagens.Add(MensagemDeOutbox.Registrar(organizacaoId, evento, _serializador));

            entidade.LimparEventos();
        }

        return mensagens;
    }

    // O tenant do evento é o da entidade que o gerou: a organização a que ela
    // pertence, ou — no caso da própria organização, raiz do isolamento — o seu id.
    private static Guid ResolverTenant(IPossuiEventosDeDominio entidade) => entidade switch
    {
        IPertenceOrganizacao pertence => pertence.OrganizacaoId,
        Organizacao organizacao => organizacao.Id,
        _ => throw new InvalidOperationException(
            $"Entidade '{entidade.GetType().Name}' emite eventos mas não expõe tenant; " +
            "implemente IPertenceOrganizacao ou trate-a como raiz de isolamento.")
    };
}
