using Morpheus.Api.Seguranca;
using Morpheus.Aplicacao.Senhas;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Api.Endpoints.Senhas;

/// <summary>
/// Recuperação e redefinição de senha (E1-F2-H3). As duas rotas são anônimas — quem
/// esqueceu a senha não tem sessão — e limitadas por origem, porque um formulário
/// anônimo que consulta o cadastro é alvo natural de varredura.
/// </summary>
public sealed class EndpointDeSenha : IEndpoint
{
    private const string MensagemDeRecuperacao =
        "Se houver conta para este e-mail, o link de redefinição foi enviado.";

    public void Mapear(IEndpointRouteBuilder rotas)
    {
        rotas.MapPost("/senhas/recuperacoes", Recuperar)
             .AllowAnonymous()
             .RequireRateLimiting(ConfiguracaoDeLimiteDeRequisicoes.PoliticaDeAutenticacao)
             .WithName("RecuperarSenha");

        rotas.MapPost("/senhas/redefinicoes", Redefinir)
             .AllowAnonymous()
             .RequireRateLimiting(ConfiguracaoDeLimiteDeRequisicoes.PoliticaDeAutenticacao)
             .WithName("RedefinirSenha");
    }

    private static async Task<IResult> Recuperar(
        RequisicaoDeRecuperacao requisicao,
        SolicitacaoDeRecuperacaoDeSenha solicitacao,
        CancellationToken cancelamento)
    {
        await solicitacao.ExecutarAsync(requisicao.Email, cancelamento);
        return Results.Accepted(value: new RespostaDeRecuperacao(MensagemDeRecuperacao));
    }

    private static async Task<IResult> Redefinir(
        RequisicaoDeRedefinicao requisicao,
        RedefinicaoDeSenha redefinicao,
        CancellationToken cancelamento)
    {
        if (!string.Equals(requisicao.NovaSenha, requisicao.ConfirmacaoDeSenha, StringComparison.Ordinal))
            return RespostaDeFalha.De(ErrosDeCadastro.SenhasNaoConferem, StatusCodes.Status400BadRequest);

        var resultado = await redefinicao.ExecutarAsync(
            requisicao.Email, requisicao.Token, requisicao.NovaSenha, cancelamento);

        return resultado.Sucesso
            ? Results.NoContent()
            : RespostaDeFalha.De(resultado.Erro, StatusCodes.Status400BadRequest);
    }
}
