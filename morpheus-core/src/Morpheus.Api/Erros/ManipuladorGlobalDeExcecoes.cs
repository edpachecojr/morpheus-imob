using Microsoft.AspNetCore.Diagnostics;
using Morpheus.Api.Endpoints;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Api.Erros;

/// <summary>
/// Último ponto de captura de exceção não tratada (E1-F4-H2): o cliente recebe
/// um ProblemDetails genérico, sem detalhe interno, e o log recebe a exceção
/// completa com o contexto da requisição — os dois lados do mesmo incidente,
/// cada um com o que precisa saber.
/// <para>
/// Reaproveita <see cref="RespostaDeFalha"/> — o mesmo ponto único que já
/// traduz <c>Resultado</c> de falha em ProblemDetails — em vez de
/// <c>IProblemDetailsService</c>: este último enriquece toda resposta com um
/// <c>traceId</c> por requisição, o que quebraria o contrato de resposta
/// byte-idêntica que a autenticação usa contra enumeração de contas.
/// </para>
/// <para>
/// Exceção de domínio (<c>ErroDeRegraDeNegocio</c> e afins) é invariante que
/// nunca deveria ser violada, não entrada de usuário — por isso cai aqui como
/// 500, não como 400: 400 é reservado para o <c>Resultado</c> de falha que o
/// próprio caso de uso devolveu de propósito.
/// </para>
/// </summary>
public sealed class ManipuladorGlobalDeExcecoes : IExceptionHandler
{
    private static readonly Erro ErroGenerico = new(
        "Api.ErroInesperado", "Ocorreu um erro inesperado. Tente novamente em instantes.");

    private readonly ILogger<ManipuladorGlobalDeExcecoes> _diario;

    public ManipuladorGlobalDeExcecoes(ILogger<ManipuladorGlobalDeExcecoes> diario) => _diario = diario;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext contexto, Exception excecao, CancellationToken cancelamento)
    {
        _diario.LogError(excecao,
            "Erro não tratado em {metodo_http} {rota}", contexto.Request.Method, contexto.Request.Path);

        await RespostaDeFalha.De(ErroGenerico, StatusCodes.Status500InternalServerError).ExecuteAsync(contexto);
        return true;
    }
}
