using Morpheus.Api.Autorizacao;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Api.Endpoints.Organizacoes;

/// <summary>
/// Onboarding da organização (E1-F1-H4): substitui o nome herdado do fundador
/// pelo nome definitivo. Exige <c>tenant.configurar</c> — só o dono completa o
/// onboarding da organização inteira, diferente dos próprios dados do usuário.
/// </summary>
public sealed class EndpointDeOrganizacao : IEndpoint
{
    public void Mapear(IEndpointRouteBuilder rotas) =>
        rotas.MapPatch("/organizacao", Renomear)
             .RequerPermissao(PermissoesDoPainel.TenantConfigurar)
             .WithName("RenomearOrganizacao");

    private static async Task<IResult> Renomear(
        RequisicaoDeRenomeacaoDaOrganizacao requisicao,
        RenomeacaoDaOrganizacao renomeacao,
        CancellationToken cancelamento)
    {
        var resultado = await renomeacao.ExecutarAsync(requisicao.Nome, cancelamento);
        return resultado.Sucesso
            ? Results.NoContent()
            : RespostaDeFalha.De(resultado.Erro, StatusCodes.Status400BadRequest);
    }
}
