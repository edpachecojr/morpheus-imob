using Dapper;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Infraestrutura.Imoveis;

/// <summary>
/// Leitura performática de imóveis via Dapper. A organização do contexto entra
/// como parâmetro da consulta — o filtro é explícito no SQL montado por
/// <see cref="ConsultaDeImoveisDaOrganizacao"/>, jamais implícito.
/// </summary>
public sealed class LeitorDeImoveisComDapper : IConsultaDeImoveisResumidos
{
    private readonly IFabricaDeConexao _fabrica;
    private readonly IContextoDaOrganizacaoAtual _organizacao;

    public LeitorDeImoveisComDapper(IFabricaDeConexao fabrica, IContextoDaOrganizacaoAtual organizacao)
    {
        _fabrica = fabrica;
        _organizacao = organizacao;
    }

    public async Task<IReadOnlyList<ImovelResumo>> ListarAsync(CancellationToken cancelamento)
    {
        var organizacaoId = await _organizacao.ObterOrganizacaoIdAsync(cancelamento);
        using var conexao = await _fabrica.AbrirAsync(cancelamento);

        var comando = new CommandDefinition(
            ConsultaDeImoveisDaOrganizacao.ListarResumoPorOrganizacao(),
            new { OrganizacaoId = organizacaoId },
            cancellationToken: cancelamento);

        var imoveis = await conexao.QueryAsync<ImovelResumo>(comando);
        return imoveis.ToList();
    }
}
