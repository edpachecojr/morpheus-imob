using Morpheus.Api.Autorizacao;
using Morpheus.Api.Seguranca;
using Morpheus.Aplicacao.Sessoes;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Api.Endpoints.Sessoes;

/// <summary>
/// Abertura e encerramento da sessão do painel (E1-F2-H1 e E1-F2-H4). Uma sessão
/// é um recurso: criá-la é <c>POST /sessoes</c>, encerrá-la é
/// <c>DELETE /sessoes/atual</c>.
/// <para>
/// A recusa de login é sempre a mesma — e-mail inexistente, senha errada e conta
/// bloqueada devolvem o mesmo corpo e o mesmo status. O motivo real vai só para o
/// log, que é onde ele serve para investigar sem servir para enumerar.
/// </para>
/// </summary>
public sealed class EndpointDeSessao : IEndpoint
{
    public void Mapear(IEndpointRouteBuilder rotas)
    {
        rotas.MapPost("/sessoes", Entrar)
             .AllowAnonymous()
             .RequireRateLimiting(ConfiguracaoDeLimiteDeRequisicoes.PoliticaDeAutenticacao)
             .WithName("AbrirSessao");

        rotas.MapDelete("/sessoes/atual", Sair)
             .RequerApenasSessao()
             .WithName("EncerrarSessao");
    }

    private static async Task<IResult> Entrar(
        RequisicaoDeLogin requisicao,
        AutenticacaoDeUsuario autenticacao,
        ILogger<EndpointDeSessao> diario,
        CancellationToken cancelamento)
    {
        var resultado = await autenticacao.ExecutarAsync(requisicao.Email, requisicao.Senha, cancelamento);
        if (resultado.Sucesso)
            return Results.NoContent();

        diario.LogWarning("Login recusado por {motivo_interno}", resultado.Erro.Codigo);
        return RespostaDeFalha.De(
            ErrosDeAutenticacao.CredenciaisInvalidas, StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Sair(ISessaoDoPainel sessao, CancellationToken cancelamento)
    {
        await sessao.EncerrarAsync(cancelamento);
        return Results.NoContent();
    }
}
