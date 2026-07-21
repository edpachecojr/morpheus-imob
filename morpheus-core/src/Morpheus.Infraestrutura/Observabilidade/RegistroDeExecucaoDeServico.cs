using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Morpheus.Infraestrutura.Observabilidade;

/// <summary>
/// Registra início implícito, duração e desfecho de uma operação de serviço, sem
/// que a classe decorada saiba que está sendo observada. É o núcleo compartilhado
/// dos decoradores de log: log entra por composição, nenhum serviço muda para
/// ganhá-lo (OCP). Loga só o nome da operação e a duração — nunca argumentos, que
/// podem carregar dado sensível.
///
/// Exemplo: <c>await RegistroDeExecucaoDeServico.MedirAsync(diario,
/// "listar_imoveis", () => leitor.ListarAsync(ct));</c>
/// </summary>
public static class RegistroDeExecucaoDeServico
{
    public static async Task<T> MedirAsync<T>(ILogger diario, string operacao, Func<Task<T>> executar)
    {
        var cronometro = Stopwatch.StartNew();
        try
        {
            var resultado = await executar();
            diario.LogInformation(
                "Operação {Operacao} concluída em {DuracaoMs}ms", operacao, cronometro.ElapsedMilliseconds);
            return resultado;
        }
        catch (Exception excecao)
        {
            diario.LogError(
                excecao, "Operação {Operacao} falhou após {DuracaoMs}ms", operacao, cronometro.ElapsedMilliseconds);
            throw;
        }
    }
}
