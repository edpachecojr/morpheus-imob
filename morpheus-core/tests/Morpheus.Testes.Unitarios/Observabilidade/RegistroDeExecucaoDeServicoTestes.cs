using Microsoft.Extensions.Logging;
using Morpheus.Infraestrutura.Observabilidade;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Observabilidade;

/// <summary>
/// Prova o núcleo compartilhado dos decoradores de log: em sucesso, devolve o
/// resultado e registra em <c>Information</c>; em falha, registra em <c>Error</c>
/// com a exceção e propaga — nunca engole o erro.
/// </summary>
public sealed class RegistroDeExecucaoDeServicoTestes
{
    [Fact]
    public async Task Em_sucesso_devolve_o_resultado_e_loga_informacao()
    {
        var diario = new DiarioDeOperacoesFake();

        var resultado = await RegistroDeExecucaoDeServico.MedirAsync(
            diario, "operacao_x", () => Task.FromResult(42));

        Assert.Equal(42, resultado);
        var entrada = Assert.Single(diario.Entradas);
        Assert.Equal(LogLevel.Information, entrada.Nivel);
        Assert.Contains("operacao_x", entrada.Mensagem);
    }

    [Fact]
    public async Task Em_falha_loga_erro_com_excecao_e_propaga()
    {
        var diario = new DiarioDeOperacoesFake();
        var falha = new InvalidOperationException("banco fora");

        var capturada = await Assert.ThrowsAsync<InvalidOperationException>(
            () => RegistroDeExecucaoDeServico.MedirAsync<int>(
                diario, "operacao_y", () => Task.FromException<int>(falha)));

        Assert.Same(falha, capturada);
        var entrada = Assert.Single(diario.Entradas);
        Assert.Equal(LogLevel.Error, entrada.Nivel);
        Assert.Same(falha, entrada.Excecao);
    }
}
