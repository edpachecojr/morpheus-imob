using Morpheus.Aplicacao.Comum;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Transação de mentira que registra o desfecho: confirmou ou desfez. Não desfaz
/// gravação nenhuma — o que o teste precisa provar é que o caso de uso devolveu
/// falha de dentro do bloco, e portanto a transação real desfaria.
/// </summary>
public sealed class ExecucaoTransacionalFake : IExecucaoTransacional
{
    public bool Confirmou { get; private set; }
    public bool Desfez { get; private set; }

    public async Task<Resultado> ExecutarAsync(
        Func<CancellationToken, Task<Resultado>> operacao, CancellationToken cancelamento)
    {
        var resultado = await operacao(cancelamento);
        Confirmou = resultado.Sucesso;
        Desfez = resultado.Falha;
        return resultado;
    }
}
