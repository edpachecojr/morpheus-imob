using Morpheus.Api.Autorizacao;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Api.Endpoints.Imoveis;

/// <summary>
/// Leitura de imóveis da organização do contexto. A rota não recebe — nem aceita —
/// identificador de organização: o tenant sai da sessão, pelo contexto de
/// organização atual, e é aplicado dentro da consulta.
/// </summary>
public sealed class EndpointDeImoveis : IEndpoint
{
    public void Mapear(IEndpointRouteBuilder rotas) =>
        rotas.MapGet("/imoveis", Listar)
             .RequerPermissao(PermissoesDoPainel.ImovelLer)
             .WithName("ListarImoveis");

    private static async Task<IResult> Listar(
        IConsultaDeImoveisResumidos consulta, CancellationToken cancelamento) =>
        Results.Ok(await consulta.ListarAsync(cancelamento));
}
