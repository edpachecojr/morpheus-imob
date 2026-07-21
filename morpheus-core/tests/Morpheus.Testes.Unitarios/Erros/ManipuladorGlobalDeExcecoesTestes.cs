using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Morpheus.Api.Erros;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Erros;

/// <summary>
/// Cobre o tratamento de erro ponta a ponta (E1-F4-H2): exceção não tratada
/// vira ProblemDetails genérico para o cliente e vai completa para o log —
/// os dois lados do mesmo incidente, sem que um vaze no outro.
/// </summary>
public sealed class ManipuladorGlobalDeExcecoesTestes
{
    [Fact]
    public async Task Excecao_nao_tratada_vira_problem_details_generico_sem_detalhe_interno()
    {
        var (manipulador, contexto, _) = Construir();
        var excecao = new InvalidOperationException("segredo-interno: senha=abc123");

        var tratado = await manipulador.TryHandleAsync(contexto, excecao, CancellationToken.None);

        Assert.True(tratado);
        Assert.Equal(StatusCodes.Status500InternalServerError, contexto.Response.StatusCode);
        var corpo = LerCorpo(contexto);
        Assert.DoesNotContain("segredo-interno", corpo);
        Assert.DoesNotContain(nameof(InvalidOperationException), corpo);
    }

    [Fact]
    public async Task Excecao_nao_tratada_e_registrada_no_log_com_a_excecao_completa()
    {
        var (manipulador, contexto, diario) = Construir();
        var excecao = new InvalidOperationException("detalhe interno sensível");

        await manipulador.TryHandleAsync(contexto, excecao, CancellationToken.None);

        Assert.Contains(diario.Entradas, entrada => entrada.Excecao == excecao);
    }

    private static (ManipuladorGlobalDeExcecoes, HttpContext, DiarioDeOperacoesFake<ManipuladorGlobalDeExcecoes>) Construir()
    {
        var diario = new DiarioDeOperacoesFake<ManipuladorGlobalDeExcecoes>();
        var contexto = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
        };
        contexto.Response.Body = new MemoryStream();

        return (new ManipuladorGlobalDeExcecoes(diario), contexto, diario);
    }

    private static string LerCorpo(HttpContext contexto)
    {
        contexto.Response.Body.Seek(0, SeekOrigin.Begin);
        return new StreamReader(contexto.Response.Body).ReadToEnd();
    }
}
