using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Morpheus.Dominio.Comum;

namespace Morpheus.Infraestrutura.Persistencia.Outbox;

/// <summary>
/// Grava os eventos de domínio no outbox dentro da própria transação do
/// SaveChanges — a metade estrutural do outbox que dá atomicidade: o dado e o
/// evento são persistidos juntos. É um adaptador fino sobre
/// <see cref="MontadorDeMensagensDeOutbox"/>; toda a lógica testável vive lá.
/// <para>
/// Registrado <b>depois</b> do interceptor de vínculo por organização, para que a
/// entidade já esteja carimbada com o tenant quando o evento é drenado.
/// </para>
/// </summary>
public sealed class InterceptorDeGravacaoDeOutbox : SaveChangesInterceptor
{
    private readonly MontadorDeMensagensDeOutbox _montador;

    public InterceptorDeGravacaoDeOutbox(MontadorDeMensagensDeOutbox montador)
        => _montador = montador;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData evento,
        InterceptionResult<int> resultado,
        CancellationToken cancelamento = default)
    {
        if (evento.Context is DbContext banco)
            GravarEventosPendentes(banco);

        return base.SavingChangesAsync(evento, resultado, cancelamento);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData evento,
        InterceptionResult<int> resultado)
    {
        if (evento.Context is DbContext banco)
            GravarEventosPendentes(banco);

        return base.SavingChanges(evento, resultado);
    }

    private void GravarEventosPendentes(DbContext banco)
    {
        var portadoras = banco.ChangeTracker
            .Entries<IPossuiEventosDeDominio>()
            .Select(entrada => entrada.Entity);

        var mensagens = _montador.Drenar(portadoras);
        if (mensagens.Count > 0)
            banco.Set<MensagemDeOutbox>().AddRange(mensagens);
    }
}
