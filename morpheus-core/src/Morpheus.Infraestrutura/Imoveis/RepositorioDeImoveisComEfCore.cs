using Microsoft.EntityFrameworkCore;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Imoveis;
using Morpheus.Infraestrutura.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Infraestrutura.Imoveis;

/// <summary>
/// Escrita e leitura transacional de imóveis via EF Core. A leitura passa pelo
/// filtro explícito <see cref="FiltroDaOrganizacao.DaOrganizacao"/>; a escrita
/// carimba a organização do contexto antes de persistir. O interceptor ainda
/// valida o vínculo no SaveChanges como segunda barreira.
/// </summary>
public sealed class RepositorioDeImoveisComEfCore : IRepositorioDeImoveis
{
    private readonly MorpheusDbContext _banco;
    private readonly IContextoDaOrganizacaoAtual _organizacao;

    public RepositorioDeImoveisComEfCore(MorpheusDbContext banco, IContextoDaOrganizacaoAtual organizacao)
    {
        _banco = banco;
        _organizacao = organizacao;
    }

    public async Task AdicionarAsync(Imovel imovel, CancellationToken cancelamento)
    {
        var organizacaoId = await _organizacao.ObterOrganizacaoIdAsync(cancelamento);
        imovel.AtribuirOrganizacao(organizacaoId);
        await _banco.Imoveis.AddAsync(imovel, cancelamento);
        await _banco.SaveChangesAsync(cancelamento);
    }

    public async Task<IReadOnlyList<Imovel>> ListarDaOrganizacaoAsync(CancellationToken cancelamento)
    {
        var organizacaoId = await _organizacao.ObterOrganizacaoIdAsync(cancelamento);
        return await _banco.Imoveis
            .DaOrganizacao(organizacaoId)
            .AsNoTracking()
            .OrderBy(imovel => imovel.CodigoDeReferencia)
            .ToListAsync(cancelamento);
    }

    public async Task<Imovel?> BuscarPorCodigoAsync(string codigoDeReferencia, CancellationToken cancelamento)
    {
        var organizacaoId = await _organizacao.ObterOrganizacaoIdAsync(cancelamento);
        return await _banco.Imoveis
            .DaOrganizacao(organizacaoId)
            .AsNoTracking()
            .SingleOrDefaultAsync(imovel => imovel.CodigoDeReferencia == codigoDeReferencia, cancelamento);
    }
}
