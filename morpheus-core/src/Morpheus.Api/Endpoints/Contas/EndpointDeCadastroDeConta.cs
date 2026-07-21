using Morpheus.Api.Seguranca;
using Morpheus.Aplicacao.Contas;

namespace Morpheus.Api.Endpoints.Contas;

/// <summary>
/// Cadastro público de conta e tenant (E1-F1-H2). Anônimo por definição — é a
/// porta de entrada de quem ainda não tem conta — e por isso limitado por origem.
/// <para>
/// Três caminhos respondem <b>exatamente</b> a mesma coisa: cadastro criado,
/// e-mail já existente e armadilha de robô preenchida. Qualquer diferença entre
/// eles transformaria o formulário em verificador de "esta pessoa é cliente?".
/// </para>
/// </summary>
public sealed class EndpointDeCadastroDeConta : IEndpoint
{
    private const string MensagemDeRecebimento =
        "Cadastro recebido. Se este e-mail ainda não tiver conta, você receberá as instruções de acesso.";

    public void Mapear(IEndpointRouteBuilder rotas) =>
        rotas.MapPost("/contas", Cadastrar)
             .AllowAnonymous()
             .RequireRateLimiting(ConfiguracaoDeLimiteDeRequisicoes.PoliticaDeAutenticacao)
             .WithName("CadastrarConta");

    private static async Task<IResult> Cadastrar(
        RequisicaoDeCadastro requisicao,
        CadastroDeConta cadastro,
        ILogger<EndpointDeCadastroDeConta> diario,
        CancellationToken cancelamento)
    {
        if (requisicao.ArmadilhaPreenchida)
        {
            diario.LogWarning("Cadastro descartado: armadilha de robô preenchida");
            return Recebido();
        }

        var resultado = await cadastro.ExecutarAsync(requisicao.ParaDados(), cancelamento);
        return resultado.Sucesso
            ? Recebido()
            : RespostaDeFalha.De(resultado.Erro, StatusCodes.Status400BadRequest);
    }

    // Sem id e sem Location: devolver o identificador da conta criada seria
    // justamente a diferença observável que os três caminhos não podem ter.
    private static IResult Recebido() =>
        Results.Json(new RespostaDeCadastro(MensagemDeRecebimento), statusCode: StatusCodes.Status201Created);
}
