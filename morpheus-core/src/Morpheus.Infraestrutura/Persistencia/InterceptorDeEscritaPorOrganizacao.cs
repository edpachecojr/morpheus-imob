using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Infraestrutura.Persistencia;

/// <summary>
/// Impõe o isolamento de escrita antes de cada SaveChanges: toda entidade que
/// pertence a uma organização é carimbada com a organização do contexto, ou tem
/// o vínculo divergente rejeitado. Sem contexto (bootstrap/job), exige que o
/// vínculo já venha explícito. É a metade estrutural da decisão do ADR-0003 que
/// dispensa lembrar do filtro em cada escrita.
/// </summary>
public sealed class InterceptorDeEscritaPorOrganizacao : SaveChangesInterceptor
{
    private readonly IContextoDaOrganizacaoAtual _contexto;

    public InterceptorDeEscritaPorOrganizacao(IContextoDaOrganizacaoAtual contexto)
        => _contexto = contexto;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData evento,
        InterceptionResult<int> resultado,
        CancellationToken cancelamento = default)
    {
        if (evento.Context is DbContext banco)
        {
            var organizacao = await _contexto.ObterOrganizacaoIdOuNuloAsync(cancelamento);
            GarantirVinculoDasEntidades(banco, organizacao);
        }

        return await base.SavingChangesAsync(evento, resultado, cancelamento);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData evento,
        InterceptionResult<int> resultado)
    {
        if (evento.Context is DbContext banco)
        {
            var organizacao = _contexto
                .ObterOrganizacaoIdOuNuloAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            GarantirVinculoDasEntidades(banco, organizacao);
        }

        return base.SavingChanges(evento, resultado);
    }

    private static void GarantirVinculoDasEntidades(DbContext banco, Guid? organizacaoDoContexto)
    {
        var alteradas = banco.ChangeTracker
            .Entries<IPertenceOrganizacao>()
            .Where(entrada => entrada.State is EntityState.Added or EntityState.Modified);

        foreach (var entrada in alteradas)
            RegraDeVinculoComOrganizacao.GarantirVinculoComContextoOpcional(entrada.Entity, organizacaoDoContexto);
    }
}
