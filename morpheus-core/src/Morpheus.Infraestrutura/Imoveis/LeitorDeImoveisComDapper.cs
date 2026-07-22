using Dapper;
using Morpheus.Aplicacao.Comum;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Imoveis;
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

    public async Task<ResultadoPaginado<ImovelResumo>> ListarAsync(
        FiltroDeListagemDeImoveis filtro, CancellationToken cancelamento)
    {
        var organizacaoId = await _organizacao.ObterOrganizacaoIdAsync(cancelamento);
        using var conexao = await _fabrica.AbrirAsync(cancelamento);

        var comando = new CommandDefinition(
            ConsultaDeImoveisDaOrganizacao.ListarResumoPorOrganizacao(),
            ParametrosDaConsulta(organizacaoId, filtro),
            cancellationToken: cancelamento);

        var linhas = (await conexao.QueryAsync<LinhaDeImovelResumo>(comando)).ToList();
        return ParaResultadoPaginado(linhas, filtro);
    }

    private static object ParametrosDaConsulta(Guid organizacaoId, FiltroDeListagemDeImoveis filtro) => new
    {
        OrganizacaoId = organizacaoId,
        Busca = string.IsNullOrWhiteSpace(filtro.Busca) ? null : filtro.Busca.Trim(),
        Finalidade = filtro.Finalidade?.ToString(),
        Situacao = filtro.Situacao?.ToString(),
        filtro.TamanhoDaPagina,
        Offset = (filtro.Pagina - 1) * filtro.TamanhoDaPagina,
    };

    private static ResultadoPaginado<ImovelResumo> ParaResultadoPaginado(
        IReadOnlyList<LinhaDeImovelResumo> linhas, FiltroDeListagemDeImoveis filtro)
    {
        var itens = linhas.Select(linha => linha.ParaResumo()).ToList();
        var total = linhas.Count > 0 ? linhas[0].Total : 0;
        return new ResultadoPaginado<ImovelResumo>(itens, total, filtro.Pagina, filtro.TamanhoDaPagina);
    }

    // Projeção 1:1 da linha do SQL, incluindo o total da janela — separada de
    // ImovelResumo porque este último não carrega detalhe de paginação.
    private sealed record LinhaDeImovelResumo(
        Guid Id,
        string CodigoDeReferencia,
        string Titulo,
        FinalidadeDoImovel Finalidade,
        SituacaoDoImovel Situacao,
        string Endereco,
        int Total)
    {
        public ImovelResumo ParaResumo() => new(Id, CodigoDeReferencia, Titulo, Finalidade, Situacao, Endereco);
    }
}
