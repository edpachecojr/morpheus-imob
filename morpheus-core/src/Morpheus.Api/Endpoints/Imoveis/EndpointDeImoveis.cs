using Morpheus.Api.Autorizacao;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Api.Endpoints.Imoveis;

/// <summary>
/// Leitura de imóveis da organização do contexto. A rota não recebe — nem aceita —
/// identificador de organização: o tenant sai da sessão, pelo contexto de
/// organização atual, e é aplicado dentro da consulta. Busca, filtro por
/// finalidade/situação e paginação vêm da query string (E2-F1-H2).
/// </summary>
public sealed class EndpointDeImoveis : IEndpoint
{
    public void Mapear(IEndpointRouteBuilder rotas) =>
        rotas.MapGet("/imoveis", Listar)
             .RequerPermissao(PermissoesDoPainel.ImovelLer)
             .WithName("ListarImoveis");

    private static async Task<IResult> Listar(
        [AsParameters] ParametrosDeListagemDeImoveis parametros,
        IConsultaDeImoveisResumidos consulta,
        CancellationToken cancelamento)
    {
        var filtroOuFalha = parametros.ParaFiltro();
        if (filtroOuFalha.Falha)
            return RespostaDeFalha.De(filtroOuFalha.Erro, StatusCodes.Status400BadRequest);

        var filtro = filtroOuFalha.Valor;
        var validacao = filtro.Validar();
        if (validacao.Falha)
            return RespostaDeFalha.De(validacao.Erro, StatusCodes.Status400BadRequest);

        return Results.Ok(await consulta.ListarAsync(filtro, cancelamento));
    }
}
