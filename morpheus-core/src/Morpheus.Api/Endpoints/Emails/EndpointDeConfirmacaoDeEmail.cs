using Morpheus.Api.Autorizacao;
using Morpheus.Api.Seguranca;
using Morpheus.Aplicacao.Usuarios;

namespace Morpheus.Api.Endpoints.Emails;

/// <summary>
/// Confirmação de e-mail do cadastro (E1-F2-H6). Reenvio exige sessão — quem
/// pede já provou identidade ao logar, sem risco de enumeração. Confirmação é
/// anônima — o link chega pela caixa de entrada, não por uma sessão ativa — e
/// por isso limitada por origem, como as demais rotas anônimas de conta.
/// </summary>
public sealed class EndpointDeConfirmacaoDeEmail : IEndpoint
{
    public void Mapear(IEndpointRouteBuilder rotas)
    {
        rotas.MapPost("/emails/confirmacoes", Reenviar)
             .RequerApenasSessao()
             .WithName("ReenviarConfirmacaoDeEmail");

        rotas.MapPost("/emails/verificacoes", Confirmar)
             .AllowAnonymous()
             .RequireRateLimiting(ConfiguracaoDeLimiteDeRequisicoes.PoliticaDeAutenticacao)
             .WithName("ConfirmarEmail");
    }

    private static async Task<IResult> Reenviar(
        SolicitacaoDeConfirmacaoDeEmail solicitacao, CancellationToken cancelamento)
    {
        await solicitacao.ExecutarAsync(cancelamento);
        return Results.Accepted();
    }

    private static async Task<IResult> Confirmar(
        RequisicaoDeVerificacaoDeEmail requisicao,
        ConfirmacaoDeEmail confirmacao,
        CancellationToken cancelamento)
    {
        var resultado = await confirmacao.ExecutarAsync(requisicao.Email, requisicao.Token, cancelamento);
        return resultado.Sucesso
            ? Results.NoContent()
            : RespostaDeFalha.De(resultado.Erro, StatusCodes.Status400BadRequest);
    }
}
